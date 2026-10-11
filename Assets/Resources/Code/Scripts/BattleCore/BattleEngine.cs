using System;
using System.Collections.Generic;
using System.Linq;

namespace Shadowkeep.Battle
{
    /// <summary>
    /// Движок боя без Unity. Правила по ТЗ: порядок ходов по ловкости (при равенстве герои первыми),
    /// бросок попадания, урон = оружие + сила, эффекты атакующего, затем эффекты цели.
    /// Каждый ход даёт ОД; действия тратят ОД; ход кончается по EndTurn или когда делать нечего.
    /// </summary>
    public sealed class BattleEngine
    {
        public const int GuardReduction = 2;

        private readonly List<Combatant> all = new List<Combatant>();
        private readonly IRandom rng;
        private readonly Queue<Combatant> order = new Queue<Combatant>();

        public int MaxRounds = 100;
        public int Round { get; private set; }
        public Combatant Current { get; private set; }
        public BattleOutcome Outcome { get; private set; } = BattleOutcome.Ongoing;
        public event Action<BattleEvent> Raised;

        public IReadOnlyList<Combatant> Combatants => all;
        public IEnumerable<Combatant> Heroes => all.Where(c => c.Team == Team.Heroes);
        public IEnumerable<Combatant> Enemies => all.Where(c => c.Team == Team.Enemies);
        public IEnumerable<Combatant> Opponents(Combatant of) => all.Where(c => c.Team != of.Team && c.IsAlive);

        public BattleEngine(IEnumerable<Combatant> heroes, IEnumerable<Combatant> enemies, IRandom rng)
        {
            this.rng = rng;
            all.AddRange(heroes.Select(h => { h.Team = Team.Heroes; return h; }));
            all.AddRange(enemies.Select(e => { e.Team = Team.Enemies; return e; }));
        }

        public void Start()
        {
            foreach (var c in all) { c.TurnNumber = 0; c.Guarding = false; c.Cooldowns.Clear(); }
            Round = 0; Outcome = BattleOutcome.Ongoing; order.Clear();
            BeginNextTurn();
        }

        // ---- Управление ходом ----

        public bool CanPerform(IBattleAction a, Combatant target)
        {
            var actor = Current;
            if (Outcome != BattleOutcome.Ongoing || actor == null || !actor.IsAlive) return false;
            if (actor.ActionPoints < a.ApCost || actor.CooldownOf(a) > 0 || !a.IsAvailable(actor)) return false;
            switch (a.Target)
            {
                case TargetRule.SingleEnemy: return target != null && target.IsAlive && target.Team != actor.Team;
                case TargetRule.SingleAlly: return target != null && target.IsAlive && target.Team == actor.Team;
                case TargetRule.Self: return target == null || target == actor;
                default: return true; // AllEnemies
            }
        }

        public void Perform(IBattleAction a, Combatant target)
        {
            if (!CanPerform(a, target)) throw new InvalidOperationException($"{Current?.Name}: действие «{a.Name}» сейчас недоступно.");
            var actor = Current;
            actor.ActionPoints -= a.ApCost;
            if (a.Cooldown > 0) actor.Cooldowns[a] = a.Cooldown;
            if (a.Target == TargetRule.AllEnemies)
                foreach (var t in Opponents(actor).ToList()) a.Execute(this, actor, t);
            else
                a.Execute(this, actor, target ?? actor);
            CheckOutcome();
        }

        public void EndTurn()
        {
            if (Outcome != BattleOutcome.Ongoing) return;
            BeginNextTurn();
        }

        /// <summary>Ход выбирает ИИ: действия идут, пока он не вернёт null или ОД не кончатся.</summary>
        public void RunTurn(IActionChooser chooser)
        {
            while (Outcome == BattleOutcome.Ongoing)
            {
                var choice = chooser.Choose(this, Current);
                if (choice == null || !CanPerform(choice.Action, choice.Target)) break;
                Perform(choice.Action, choice.Target);
            }
            EndTurn();
        }

        public BattleOutcome RunToEnd(IActionChooser heroes, IActionChooser enemies)
        {
            while (Outcome == BattleOutcome.Ongoing)
                RunTurn(Current.Team == Team.Heroes ? heroes : enemies);
            return Outcome;
        }

        // ---- Правила, которые вызывают действия ----

        public void Attack(Combatant a, Combatant d, Action<DamageContext> extra = null)
        {
            // Шаг 1: бросок 1..(ловк. атакующего + ловк. цели); промах, если бросок <= ловкости цели.
            int roll = rng.Next(1, a.Agility + d.Agility + 1);
            if (roll <= d.Agility) { Raise(BattleEventType.Missed, a, d, 0); return; }

            var ctx = new DamageContext(a, d);                 // шаг 2: оружие + сила
            foreach (var e in a.Effects) e.OnAttack(ctx);      // шаг 3: эффекты атакующего
            extra?.Invoke(ctx);
            if (d.Guarding) ctx.Reduction += GuardReduction;
            foreach (var e in d.Effects) e.OnDefense(ctx);     // шаг 4: эффекты цели

            int dmg = ctx.FinalDamage;                          // шаг 5
            d.Hp = Math.Max(0, d.Hp - dmg);
            Raise(BattleEventType.Hit, a, d, dmg, ctx.IsSpecialAttack ? "special" : null);
            if (!d.IsAlive) Raise(BattleEventType.Died, a, d, 0);
        }

        public void Heal(Combatant actor, int amount)
        {
            int before = actor.Hp; actor.Heal(amount);
            Raise(BattleEventType.Healed, actor, actor, actor.Hp - before);
        }

        public void Guard(Combatant actor) { actor.Guarding = true; Raise(BattleEventType.Guarded, actor, actor, GuardReduction); }

        // ---- Внутреннее ----

        private void BeginNextTurn()
        {
            while (true)
            {
                if (order.Count == 0)
                {
                    Round++;
                    if (Round > MaxRounds) { Finish(BattleOutcome.Stalemate); return; }
                    int i = 0;
                    foreach (var c in all.Where(x => x.IsAlive)
                                         .Select(x => new { x, idx = i++ })
                                         .OrderByDescending(p => p.x.Agility)
                                         .ThenBy(p => p.x.Team)   // при равной ловкости герои первыми
                                         .ThenBy(p => p.idx)
                                         .Select(p => p.x))
                        order.Enqueue(c);
                }
                var next = order.Dequeue();
                if (!next.IsAlive) continue;

                Current = next;
                next.TurnNumber++;
                next.ActionPoints = next.ActionPointsPerTurn;
                next.Guarding = false;
                foreach (var key in next.Cooldowns.Keys.ToList())
                    if (next.Cooldowns[key] > 0) next.Cooldowns[key]--;
                Raise(BattleEventType.TurnStarted, next, null, next.ActionPoints);
                return;
            }
        }

        private void CheckOutcome()
        {
            if (!Enemies.Any(e => e.IsAlive)) Finish(BattleOutcome.HeroesWon);
            else if (!Heroes.Any(h => h.IsAlive)) Finish(BattleOutcome.EnemiesWon);
        }

        private void Finish(BattleOutcome outcome)
        {
            Outcome = outcome;
            Raise(BattleEventType.Ended, null, null, 0, outcome.ToString());
        }

        private void Raise(BattleEventType type, Combatant actor, Combatant target, int amount, string text = null)
            => Raised?.Invoke(new BattleEvent { Type = type, Actor = actor, Target = target, Amount = amount, Text = text });
    }
}
