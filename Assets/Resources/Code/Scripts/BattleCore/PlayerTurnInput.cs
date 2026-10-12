using System.Collections.Generic;
using System.Linq;

namespace Shadowkeep.Battle
{
    /// <summary>
    /// Ввод игрока на его ходу без привязки к Unity: «выбрал действие -> (выбрал цель) -> готово».
    /// Интерфейс только вызывает эти методы и читает состояние, поэтому логика проверяется тестами.
    /// </summary>
    public sealed class PlayerTurnInput
    {
        private readonly BattleEngine engine;

        /// <summary>Действие выбрано, ждём цель (иначе null).</summary>
        public IBattleAction PendingAction { get; private set; }
        /// <summary>Готовая команда (действие + цель), которую нужно выполнить (иначе null).</summary>
        public ChosenAction Ready { get; private set; }
        public bool EndTurnRequested { get; private set; }

        public PlayerTurnInput(BattleEngine engine) { this.engine = engine; }

        public void Reset() { PendingAction = null; Ready = null; EndTurnRequested = false; }

        public IEnumerable<Combatant> ValidTargets(IBattleAction a)
        {
            var actor = engine.Current;
            if (actor == null) return Enumerable.Empty<Combatant>();
            switch (a.Target)
            {
                case TargetRule.SingleEnemy: return engine.Opponents(actor).ToList();
                case TargetRule.SingleAlly: return engine.Combatants.Where(c => c.Team == actor.Team && c.IsAlive).ToList();
                case TargetRule.Self: return new[] { actor };
                default: return Enumerable.Empty<Combatant>();
            }
        }

        /// <summary>Игрок нажал кнопку действия. false, если сейчас его использовать нельзя.</summary>
        public bool SelectAction(IBattleAction a)
        {
            if (!engine.CanUse(a)) return false;
            PendingAction = null;
            if (a.Target == TargetRule.AllEnemies) { Ready = new ChosenAction(a, null); return true; }

            var targets = ValidTargets(a).ToList();
            if (targets.Count == 0) return false;
            if (targets.Count == 1) { Ready = new ChosenAction(a, targets[0]); return true; } // единственная цель выбирается сама
            PendingAction = a;
            return true;
        }

        public bool PickTarget(Combatant target)
        {
            if (PendingAction == null || !ValidTargets(PendingAction).Contains(target)) return false;
            Ready = new ChosenAction(PendingAction, target);
            PendingAction = null;
            return true;
        }

        public void Cancel() { PendingAction = null; }
        public void RequestEndTurn() { EndTurnRequested = true; }

        public ChosenAction TakeReady() { var r = Ready; Ready = null; return r; }
    }
}
