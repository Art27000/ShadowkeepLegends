using System;

namespace Shadowkeep.Battle
{
    /// <summary>Команда в бою. Игрок выбирает её кнопкой, ИИ врага — кодом.</summary>
    public interface IBattleAction
    {
        string Name { get; }
        int ApCost { get; }
        int Cooldown { get; }          // ходов перезарядки после использования
        TargetRule Target { get; }
        bool IsAvailable(Combatant actor);
        void Execute(BattleEngine engine, Combatant actor, Combatant target);
    }

    /// <summary>Обычная атака: 2 ОД, то есть одна атака за ход и ещё 1 ОД на предмет или защиту.</summary>
    public sealed class AttackAction : IBattleAction
    {
        public static readonly AttackAction Instance = new AttackAction();
        public string Name => "Атака";
        public int ApCost => 2;
        public int Cooldown => 0;
        public TargetRule Target => TargetRule.SingleEnemy;
        public bool IsAvailable(Combatant actor) => true;
        public void Execute(BattleEngine e, Combatant actor, Combatant target) => e.Attack(actor, target);
    }

    /// <summary>Защита: 1 ОД, снижает входящий урон на GuardReduction до начала своего следующего хода.</summary>
    public sealed class DefendAction : IBattleAction
    {
        public static readonly DefendAction Instance = new DefendAction();
        public string Name => "Защита";
        public int ApCost => 1;
        public int Cooldown => 0;
        public TargetRule Target => TargetRule.Self;
        public bool IsAvailable(Combatant actor) => true;
        public void Execute(BattleEngine e, Combatant actor, Combatant target) => e.Guard(actor);
    }

    /// <summary>Пример активной способности: мощный удар за 3 ОД с перезарядкой.</summary>
    public sealed class PowerStrikeAction : IBattleAction
    {
        private readonly int bonus;
        public PowerStrikeAction(int bonus = 3, int cooldown = 3) { this.bonus = bonus; Cooldown = cooldown; }
        public string Name => "Мощный удар";
        public int ApCost => 3;
        public int Cooldown { get; }
        public TargetRule Target => TargetRule.SingleEnemy;
        public bool IsAvailable(Combatant actor) => true;
        public void Execute(BattleEngine e, Combatant actor, Combatant target) => e.Attack(actor, target, c => c.Bonus += bonus);
    }

    /// <summary>Зелье лечения: 1 ОД, ограниченное число зарядок.</summary>
    public sealed class PotionAction : IBattleAction
    {
        private readonly int heal;
        public int Charges { get; private set; }
        public PotionAction(int heal, int charges) { this.heal = heal; Charges = charges; }
        public string Name => "Зелье";
        public int ApCost => 1;
        public int Cooldown => 0;
        public TargetRule Target => TargetRule.Self;
        public bool IsAvailable(Combatant actor) => Charges > 0;
        public void Execute(BattleEngine e, Combatant actor, Combatant target) { Charges--; e.Heal(actor, heal); }
    }

    public sealed class ChosenAction
    {
        public IBattleAction Action; public Combatant Target;
        public ChosenAction(IBattleAction action, Combatant target) { Action = action; Target = target; }
    }

    /// <summary>Тот, кто выбирает действие: ИИ врага, режим «Авто» героя или интерфейс игрока.</summary>
    public interface IActionChooser
    {
        /// <summary>Вернуть действие или null, чтобы закончить ход.</summary>
        ChosenAction Choose(BattleEngine engine, Combatant actor);
    }
}
