using System.Collections;
using System.Collections.Generic;
using Shadowkeep.Battle;
using UnityEngine;

/// <summary>
/// Только «картинка» бойца: проигрывает анимации и подсказывает интерфейсу, где у него голова.
/// Никакой боевой логики: правила живут в BattleCore. Добавляется на заспавненный префаб из кода.
/// </summary>
/// <summary>Как подсвечен боец при выборе цели.</summary>
public enum HighlightMode { None, Selectable, Hovered }

public sealed class UnitView : MonoBehaviour
{
    public Combatant Combatant { get; private set; }

    // Имена состояния смерти в контроллерах проекта (у Гоблина, Слайма и Призрака они свои).
    private static readonly string[] DeathStateNames = { "Death", "death", "DeathGoblin", "Blue Death - Animation" };
    private static readonly string[] CombatTriggers = { "Attack", "Attack2", "SpecialAttack", "Hurt" };

    private Animator animator;
    private bool dead;
    private SpriteRenderer[] sprites = new SpriteRenderer[0];
    private Color[] originalColors = new Color[0];
    private HighlightMode highlight;
    private float animationSpeed = 1f;
    private readonly Dictionary<string, AnimatorControllerParameterType> parameters =
        new Dictionary<string, AnimatorControllerParameterType>();

    public void Init(Combatant combatant)
    {
        Combatant = combatant;
        sprites = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) originalColors[i] = sprites[i].color;

        animator = GetComponentInChildren<Animator>();
        if (animator == null) return;
        foreach (var p in animator.parameters) parameters[p.name] = p.type;
        animator.speed = animationSpeed;
    }

    /// <summary>Скорость только анимаций этого бойца (режим x2). Время игры и интерфейс не затрагиваются.</summary>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = speed;
        if (animator != null) animator.speed = speed;
    }

    public bool Has(string parameter) => parameters.ContainsKey(parameter);

    /// <summary>Область всех видимых спрайтов бойца в мире.</summary>
    public Bounds WorldBounds()
    {
        bool has = false;
        Bounds total = new Bounds(transform.position, Vector3.one);
        foreach (var sr in sprites)
        {
            if (sr == null || !sr.enabled || sr.sprite == null) continue;
            if (!has) { total = sr.bounds; has = true; }
            else total.Encapsulate(sr.bounds);
        }
        return total;
    }

    public Vector3 HeadWorldPosition()
    {
        Bounds b = WorldBounds();
        return new Vector3(b.center.x, b.max.y, transform.position.z);
    }

    /// <summary>Область бойца на экране в пикселях: по ней определяется клик при выборе цели.</summary>
    public Rect ScreenRect(Camera cam)
    {
        Bounds b = WorldBounds();
        Vector3 p1 = cam.WorldToScreenPoint(b.min), p2 = cam.WorldToScreenPoint(b.max);
        return Rect.MinMaxRect(Mathf.Min(p1.x, p2.x), Mathf.Min(p1.y, p2.y), Mathf.Max(p1.x, p2.x), Mathf.Max(p1.y, p2.y));
    }

    // ---------- Подсветка выбора цели ----------

    public void SetHighlight(HighlightMode mode)
    {
        if (dead) mode = HighlightMode.None;
        if (highlight == mode) return;
        highlight = mode;
        if (mode == HighlightMode.None) RestoreColors();
    }

    private void Update()
    {
        if (highlight == HighlightMode.None) return;
        if (highlight == HighlightMode.Hovered)
            Tint(new Color(1f, 0.5f, 0.35f), 0.55f);
        else
            Tint(new Color(1f, 0.95f, 0.4f), 0.25f + 0.2f * Mathf.Sin(Time.unscaledTime * 6f)); // пульсация выбираемых целей
    }

    private void Tint(Color tint, float amount)
    {
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = Color.Lerp(originalColors[i], new Color(tint.r, tint.g, tint.b, originalColors[i].a), amount);
    }

    private void RestoreColors()
    {
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = originalColors[i];
    }

    /// <summary>Отклик на выбор цели: короткая вспышка и «толчок» размера.</summary>
    public void PlaySelected()
    {
        if (!dead) StartCoroutine(SelectedPop());
    }

    private IEnumerator SelectedPop()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = scale * 1.08f;
        Tint(new Color(1f, 0.35f, 0.3f), 0.7f);
        yield return new WaitForSecondsRealtime(0.12f);
        transform.localScale = scale;
        if (highlight == HighlightMode.None) RestoreColors();
    }

    public void PlayAttack(bool special)
    {
        if (dead) return;
        if (special && Has("SpecialAttack")) { Fire("SpecialAttack"); return; }
        if (Has("Attack2") && Random.value < 0.5f) Fire("Attack2");
        else Fire("Attack");
    }

    public void PlayHurt() { if (!dead) Fire("Hurt"); }

    /// <summary>
    /// Смерть проигрывается сразу: сбрасываем ожидающие боевые триггеры и принудительно переходим в состояние смерти,
    /// не дожидаясь конца текущей атаки. Повторный вызов ничего не делает.
    /// </summary>
    public void PlayDeath()
    {
        if (dead || animator == null) return;
        dead = true;
        highlight = HighlightMode.None;
        RestoreColors();

        foreach (var t in CombatTriggers)
            if (Has(t) && parameters[t] == AnimatorControllerParameterType.Trigger) animator.ResetTrigger(t);

        bool forced = false;
        foreach (var stateName in DeathStateNames)
        {
            int hash = Animator.StringToHash(stateName);
            if (animator.HasState(0, hash))
            {
                animator.CrossFadeInFixedTime(hash, 0.05f, 0);
                forced = true;
                break;
            }
        }

        if (!Has("Death")) return;
        // У Bandit параметр Death - bool, у остальных trigger. Если состояние уже включено принудительно,
        // trigger не ставим, чтобы анимация смерти не запустилась второй раз.
        if (parameters["Death"] == AnimatorControllerParameterType.Bool) animator.SetBool("Death", true);
        else if (!forced) animator.SetTrigger("Death");
    }

    private void Fire(string trigger)
    {
        if (animator != null && Has(trigger) && parameters[trigger] == AnimatorControllerParameterType.Trigger)
            animator.SetTrigger(trigger);
    }
}
