using System.Linq;

namespace Shadowkeep.Battle
{
    /// <summary>Только атакует самого слабого противника. Для врагов и как базовый «Авто»; один удар за ход, как в ТЗ.</summary>
    public sealed class AttackOnlyAi : IActionChooser
    {
        public ChosenAction Choose(BattleEngine engine, Combatant actor)
        {
            var target = engine.Opponents(actor).OrderBy(o => o.Hp).FirstOrDefault();
            if (target == null || !engine.CanPerform(AttackAction.Instance, target)) return null;
            return new ChosenAction(AttackAction.Instance, target);
        }
    }

    /// <summary>Чуть умнее: лечится при низком здоровье, использует способности, потом атакует, оставшееся ОД тратит на защиту.</summary>
    public sealed class SmartAi : IActionChooser
    {
        public ChosenAction Choose(BattleEngine engine, Combatant actor)
        {
            var target = engine.Opponents(actor).OrderBy(o => o.Hp).FirstOrDefault();
            if (target == null) return null;

            foreach (var a in actor.Actions.OfType<PotionAction>())
                if (actor.Hp * 2 <= actor.MaxHp && engine.CanPerform(a, actor)) return new ChosenAction(a, actor);

            foreach (var a in actor.Actions.OfType<PowerStrikeAction>())
                if (engine.CanPerform(a, target)) return new ChosenAction(a, target);

            if (engine.CanPerform(AttackAction.Instance, target)) return new ChosenAction(AttackAction.Instance, target);
            if (engine.CanPerform(DefendAction.Instance, actor) && !actor.Guarding) return new ChosenAction(DefendAction.Instance, actor);
            return null;
        }
    }
}
