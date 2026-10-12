using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shadowkeep.Battle;
using UnityEngine;

public sealed class UnitSetup
{
    public Combatant Combatant;
    public GameObject Prefab;
    public Vector3 Position;
}

/// <summary>
/// Связывает движок боя (BattleCore) с Unity: создаёт бойцов на сцене, ведёт бой по ходам,
/// проигрывает события анимациями и ждёт ввода игрока. Правил боя здесь нет.
/// </summary>
public sealed class BattleController : MonoBehaviour
{
    public event System.Action<BattleOutcome> Finished;
    public event System.Action RestartRequested;

    public BattleEngine Engine { get; private set; }
    public PlayerTurnInput TurnInput { get; private set; }
    public bool Auto { get; private set; }
    public int Speed { get; private set; } = 1;

    private readonly Dictionary<Combatant, UnitView> views = new Dictionary<Combatant, UnitView>();
    private readonly Queue<BattleEvent> events = new Queue<BattleEvent>();
    private readonly IActionChooser heroAi = new SmartAi();
    private readonly IActionChooser enemyAi = new AttackOnlyAi();
    private BattleHud hud;
    private Coroutine routine;

    public void ToggleAuto() { Auto = !Auto; }
    public void ToggleSpeed() { Speed = Speed == 1 ? 2 : 1; }
    public void RequestRestart() { RestartRequested?.Invoke(); }

    public void Begin(IList<UnitSetup> heroes, IList<UnitSetup> enemies, int seed)
    {
        Cleanup();

        foreach (var s in heroes) Spawn(s);
        foreach (var s in enemies) Spawn(s);

        Engine = new BattleEngine(heroes.Select(h => h.Combatant), enemies.Select(e => e.Combatant), new SystemRandom(seed));
        TurnInput = new PlayerTurnInput(Engine);
        Engine.Raised += e => events.Enqueue(e);

        hud = BattleHud.Create(this);
        foreach (var view in views.Values) hud.Register(view);

        routine = StartCoroutine(Run());
    }

    public void Cleanup()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        foreach (var view in views.Values) if (view != null) Destroy(view.gameObject);
        views.Clear();
        events.Clear();
        if (hud != null) Destroy(hud.gameObject);
        hud = null;
    }

    private void Spawn(UnitSetup s)
    {
        var go = Instantiate(s.Prefab, s.Position, s.Prefab.transform.rotation);
        go.name = s.Combatant.Name;
        // Старые боевые скрипты на префабе не нужны: вся логика теперь в BattleCore.
        // Destroy срабатывает до первого Start, поэтому им не нужны назначенные данные.
        foreach (var legacy in go.GetComponents<CharacterBase>()) Destroy(legacy);
        var view = go.AddComponent<UnitView>();
        view.Init(s.Combatant);
        views[s.Combatant] = view;
    }

    private IEnumerator Run()
    {
        Engine.Start();
        while (true)
        {
            yield return Drain();
            if (Engine.Outcome != BattleOutcome.Ongoing) break;

            var actor = Engine.Current;
            bool playerControlled = actor.Team == Team.Heroes && !Auto;

            if (!playerControlled)
            {
                yield return Wait(0.3f);
                var chooser = actor.Team == Team.Heroes ? heroAi : enemyAi;
                var choice = chooser.Choose(Engine, actor);
                if (choice == null || !Engine.CanPerform(choice.Action, choice.Target)) Engine.EndTurn();
                else Engine.Perform(choice.Action, choice.Target);
                continue;
            }

            // Ход игрока: если делать нечего, ход заканчивается сам.
            if (actor.ActionPoints <= 0 || !actor.Actions.Any(a => Engine.CanUse(a)))
            {
                Engine.EndTurn();
                continue;
            }

            TurnInput.Reset();
            hud.ShowPlayerTurn(actor);
            yield return new WaitUntil(() => TurnInput.Ready != null || TurnInput.EndTurnRequested || Auto);
            hud.HidePlayerTurn();

            if (Auto) continue;
            if (TurnInput.EndTurnRequested) { Engine.EndTurn(); continue; }

            var cmd = TurnInput.TakeReady();
            if (cmd != null && Engine.CanPerform(cmd.Action, cmd.Target)) Engine.Perform(cmd.Action, cmd.Target);
        }

        yield return Drain();
        hud.ShowResult(Engine.Outcome);
        Finished?.Invoke(Engine.Outcome);
    }

    private IEnumerator Drain()
    {
        while (events.Count > 0) yield return Present(events.Dequeue());
    }

    private WaitForSeconds Wait(float seconds) => new WaitForSeconds(seconds / Speed);

    private UnitView ViewOf(Combatant c)
    {
        UnitView v;
        return c != null && views.TryGetValue(c, out v) ? v : null;
    }

    private IEnumerator Present(BattleEvent e)
    {
        switch (e.Type)
        {
            case BattleEventType.TurnStarted:
                hud.SetActiveUnit(e.Actor);
                hud.SetTurnLabel(e.Actor, Engine.Round);
                yield return Wait(0.15f);
                break;

            case BattleEventType.Missed:
                ViewOf(e.Actor)?.PlayAttack(false);
                hud.Log($"{e.Actor.Name} → {e.Target.Name}: промах");
                yield return Wait(0.35f);
                hud.Float(e.Target, "Промах", Color.white);
                yield return Wait(0.35f);
                break;

            case BattleEventType.Hit:
                ViewOf(e.Actor)?.PlayAttack(e.Text == "special");
                yield return Wait(0.35f);
                ViewOf(e.Target)?.PlayHurt();
                hud.SetHp(e.Target, e.TargetHp);
                hud.Float(e.Target, e.Amount > 0 ? "-" + e.Amount : "0", e.Amount > 0 ? new Color(1f, 0.35f, 0.3f) : Color.gray);
                hud.Log($"{e.Actor.Name} → {e.Target.Name}: {e.Amount} урона");
                yield return Wait(0.45f);
                break;

            case BattleEventType.Healed:
                hud.SetHp(e.Actor, e.TargetHp);
                hud.Float(e.Actor, "+" + e.Amount, new Color(0.4f, 1f, 0.5f));
                hud.Log($"{e.Actor.Name} лечится на {e.Amount}");
                yield return Wait(0.4f);
                break;

            case BattleEventType.Guarded:
                hud.Float(e.Actor, "Защита", new Color(0.5f, 0.8f, 1f));
                hud.Log($"{e.Actor.Name} встаёт в защиту");
                yield return Wait(0.3f);
                break;

            case BattleEventType.Died:
                ViewOf(e.Target)?.PlayDeath();
                hud.OnDied(e.Target);
                hud.Log($"{e.Target.Name} повержен");
                yield return Wait(0.8f);
                break;
        }
    }
}
