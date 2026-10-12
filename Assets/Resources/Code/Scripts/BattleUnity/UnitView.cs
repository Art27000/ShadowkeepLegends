using System.Collections.Generic;
using Shadowkeep.Battle;
using UnityEngine;

/// <summary>
/// Только «картинка» бойца: проигрывает анимации и подсказывает интерфейсу, где у него голова.
/// Никакой боевой логики: правила живут в BattleCore. Добавляется на заспавненный префаб из кода.
/// </summary>
public sealed class UnitView : MonoBehaviour
{
    public Combatant Combatant { get; private set; }

    private Animator animator;
    private readonly Dictionary<string, AnimatorControllerParameterType> parameters =
        new Dictionary<string, AnimatorControllerParameterType>();

    public void Init(Combatant combatant)
    {
        Combatant = combatant;
        animator = GetComponentInChildren<Animator>();
        if (animator == null) return;
        foreach (var p in animator.parameters) parameters[p.name] = p.type;
    }

    public bool Has(string parameter) => parameters.ContainsKey(parameter);

    public Vector3 HeadWorldPosition()
    {
        var r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            Bounds b = r.bounds;
            return new Vector3(b.center.x, b.max.y, transform.position.z);
        }
        return transform.position + Vector3.up * 1.5f;
    }

    public void PlayAttack(bool special)
    {
        if (special && Has("SpecialAttack")) { Fire("SpecialAttack"); return; }
        if (Has("Attack2") && Random.value < 0.5f) Fire("Attack2");
        else Fire("Attack");
    }

    public void PlayHurt() => Fire("Hurt");

    /// <summary>У Bandit параметр Death — bool, у остальных trigger: подстраиваемся под контроллер.</summary>
    public void PlayDeath()
    {
        if (animator == null || !Has("Death")) return;
        if (parameters["Death"] == AnimatorControllerParameterType.Bool) animator.SetBool("Death", true);
        else animator.SetTrigger("Death");
    }

    private void Fire(string trigger)
    {
        if (animator != null && Has(trigger) && parameters[trigger] == AnimatorControllerParameterType.Trigger)
            animator.SetTrigger(trigger);
    }
}
