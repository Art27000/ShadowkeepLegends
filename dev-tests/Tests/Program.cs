using System;
using System.Collections.Generic;
using System.Linq;
using Shadowkeep.Battle;

// Игрок/ИИ не нужен: тесты управляют ходами напрямую.
class AlwaysHit : IRandom { public int Next(int min, int max) => max - 1; }

static class T
{
    static int failed, passed;
    static void Check(string name, bool ok, string detail = "")
    {
        if (ok) passed++; else { failed++; Console.WriteLine($"  FAIL {name} {detail}"); }
    }
    static void Eq(string name, int expected, int actual) => Check(name, expected == actual, $"(ожидали {expected}, получили {actual})");

    static Combatant Dummy(int agi = 1, int hp = 1000)
    {
        var d = new Combatant { Name = "Манекен", Team = Team.Enemies, FlatMaxHp = hp, BaseAgility = agi, BaseStrength = 0, BaseEndurance = 0, WeaponDamage = 0 };
        d.Actions.Add(AttackAction.Instance);
        d.RestoreFull();
        return d;
    }

    static Dictionary<HeroClass, int> Lv(params (HeroClass, int)[] l) => l.ToDictionary(x => x.Item1, x => x.Item2);

    // Герой атакует один раз; затем ход манекена и возврат к герою.
    static int HitOnce(BattleEngine e, Combatant hero, Combatant target)
    {
        int before = target.Hp;
        e.Perform(AttackAction.Instance, target);
        e.EndTurn(); e.EndTurn();
        return before - target.Hp;
    }

    static void Main()
    {
        // 1. Пример из ТЗ
        var rogue = SpecContent.CreateHero("Р", 1, 3, 2, Lv((HeroClass.Bandit, 1)), SpecContent.Dagger);
        Eq("ТЗ: здоровье разбойника 6", 6, rogue.MaxHp);
        Eq("ТЗ: кинжал + сила = 3", 3, rogue.WeaponDamage + rogue.Strength);

        // 2. Варвар 1 / воин 1: порыв + ярость, затем ярость, затем -1
        var hero = SpecContent.CreateHero("В", 1, 2, 1, Lv((HeroClass.Barbarian, 1), (HeroClass.Knight, 1)), SpecContent.Club);
        var dm = Dummy(); var e = new BattleEngine(new[] { hero }, new[] { dm }, new AlwaysHit()); e.Start();
        Eq("ход 1: дубина3+сила1+порыв3+ярость2", 9, HitOnce(e, hero, dm));
        Eq("ход 2: 3+1+ярость2", 6, HitOnce(e, hero, dm));
        Eq("ход 3: 3+1+ярость2", 6, HitOnce(e, hero, dm));
        Eq("ход 4: 3+1-1", 3, HitOnce(e, hero, dm));

        // 3. Слайм: рубящее обнуляется, сила и порыв работают
        var knight = SpecContent.CreateHero("К", 1, 2, 1, Lv((HeroClass.Knight, 1)), SpecContent.Sword);
        var slime = SpecContent.CreateEnemy("Слайм"); slime.BaseAgility = 1; slime.BaseStrength = 0;
        e = new BattleEngine(new[] { knight }, new[] { slime }, new AlwaysHit()); e.Start();
        Eq("слайм: сила1 + порыв3, меч не считается", 4, HitOnce(e, knight, slime));

        // 4. Скелет: дробящее x2
        var barb = SpecContent.CreateHero("Б", 1, 2, 1, Lv((HeroClass.Barbarian, 1)), SpecContent.Club);
        var skel = SpecContent.CreateEnemy("Скелет"); skel.FlatMaxHp = 1000; skel.RestoreFull();
        e = new BattleEngine(new[] { barb }, new[] { skel }, new AlwaysHit()); e.Start();
        Eq("скелет: (3+1+2)*2", 12, HitOnce(e, barb, skel));

        // 5. Дракон: 3-й ход +3
        var target = SpecContent.CreateHero("Т", 0, 1, 0, Lv((HeroClass.Bandit, 1)), SpecContent.Dagger);
        target.FlatMaxHp = 1000; target.RestoreFull(); target.Effects.Clear();
        var dragon = SpecContent.CreateEnemy("Дракон");
        e = new BattleEngine(new[] { target }, new[] { dragon }, new AlwaysHit()); e.Start();
        var dmg = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (e.Current == target) e.EndTurn();
            int b = target.Hp; e.Perform(AttackAction.Instance, target); dmg.Add(b - target.Hp); e.EndTurn();
        }
        Check("дракон: 7,7,10,7", dmg.SequenceEqual(new[] { 7, 7, 10, 7 }), "(" + string.Join(",", dmg) + ")");

        // 6. Очки действий
        var h = SpecContent.CreateHero("Г", 1, 3, 1, Lv((HeroClass.Knight, 1)), SpecContent.Sword);
        var potion = new PotionAction(4, 2); var ps = new PowerStrikeAction();
        h.Actions.Add(potion); h.Actions.Add(ps);
        dm = Dummy(1); e = new BattleEngine(new[] { h }, new[] { dm }, new AlwaysHit()); e.Start();
        Eq("ОД в начале хода", 3, h.ActionPoints);
        e.Perform(AttackAction.Instance, dm);
        Eq("после атаки осталось 1 ОД", 1, h.ActionPoints);
        Check("второй атаки нет", !e.CanPerform(AttackAction.Instance, dm));
        Check("зелье доступно за 1 ОД", e.CanPerform(potion, h));
        h.Hp = 1; e.Perform(potion, h);
        Eq("зелье лечит 4", 5, h.Hp); Eq("заряд потрачен", 1, potion.Charges); Eq("ОД 0", 0, h.ActionPoints);
        e.EndTurn(); e.EndTurn();
        Check("мощный удар за 3 ОД", e.CanPerform(ps, dm));
        e.Perform(ps, dm); Eq("перезарядка 3", 3, h.CooldownOf(ps));
        for (int i = 0; i < 2; i++) { e.EndTurn(); e.EndTurn(); }
        Check("перезарядка ещё идёт (3-й ход)", !e.CanPerform(ps, dm));
        e.EndTurn(); e.EndTurn();
        Check("способность снова готова (4-й ход)", e.CanPerform(ps, dm));
        // защита
        var g = SpecContent.CreateHero("З", 1, 3, 1, Lv((HeroClass.Knight, 1)), SpecContent.Sword);
        var foe = SpecContent.CreateEnemy("Гоблин"); foe.BaseAgility = 1;
        e = new BattleEngine(new[] { g }, new[] { foe }, new AlwaysHit()); e.Start();
        e.Perform(DefendAction.Instance, g); e.EndTurn();
        int hp0 = g.Hp; e.Perform(AttackAction.Instance, g);
        Eq("защита: гоблин 2+1=3, минус 2", 1, hp0 - g.Hp);

        // 7. Порядок ходов
        var h1 = SpecContent.CreateHero("Г", 1, 2, 1, Lv((HeroClass.Knight, 1)), SpecContent.Sword);
        var e1 = SpecContent.CreateEnemy("Гоблин"); e1.BaseAgility = 2; var e2 = SpecContent.CreateEnemy("Призрак");
        e = new BattleEngine(new[] { h1 }, new[] { e1, e2 }, new AlwaysHit()); e.Start();
        var seq = new List<string> { e.Current.Name }; e.EndTurn(); seq.Add(e.Current.Name); e.EndTurn(); seq.Add(e.Current.Name);
        Check("порядок: Призрак(3), затем герой раньше гоблина при ничьей 2:2", seq.SequenceEqual(new[] { "Призрак", "Г", "Гоблин" }), string.Join(",", seq));

        // 8. Зависание (Варвар 3 против Голема): движок обязан остановить бой
        var b3 = SpecContent.CreateHero("Б3", 1, 1, 3, Lv((HeroClass.Barbarian, 3)), SpecContent.Club);
        var golem = SpecContent.CreateEnemy("Голем");
        e = new BattleEngine(new[] { b3 }, new[] { golem }, new AlwaysHit()); e.Start();
        var outcome = e.RunToEnd(new AttackOnlyAi(), new AttackOnlyAi());
        Check("зависание распознано", outcome == BattleOutcome.Stalemate, outcome.ToString());

        // 9. Бой с тремя гоблинами
        var solo = SpecContent.CreateHero("Г", 2, 2, 2, Lv((HeroClass.Knight, 2)), SpecContent.Sword);
        var goblins = Enumerable.Range(0, 3).Select(_ => SpecContent.CreateEnemy("Гоблин")).ToList();
        e = new BattleEngine(new[] { solo }, goblins, new SystemRandom(5)); e.Start();
        outcome = e.RunToEnd(new AttackOnlyAi(), new AttackOnlyAi());
        Check("бой 1 на 3 завершается", outcome != BattleOutcome.Ongoing, outcome.ToString());

        // 10. Сверка с Python-симулятором (правила ТЗ, арена из 5 боёв, трофей если урон выше)
        ParityVsPython();

        Console.WriteLine($"\nТесты: пройдено {passed}, провалено {failed}");
        Environment.Exit(failed == 0 ? 0 : 1);
    }

    static bool ArenaRun(HeroClass[] build, Random r, IRandom rng, bool smart)
    {
        int str = r.Next(1, 4), agi = r.Next(1, 4), end = r.Next(1, 4);
        var levels = new Dictionary<HeroClass, int> { [build[0]] = 1 };
        var weapon = SpecContent.StartWeapon(build[0]);
        var pending = new Queue<HeroClass>(build.Skip(1));
        var potion = new PotionAction(4, 2);
        for (int wins = 0; wins < 5; wins++)
        {
            string name = SpecContent.EnemyNames[r.Next(SpecContent.EnemyNames.Length)];
            var hero = SpecContent.CreateHero("Герой", str, agi, end, levels, weapon);
            if (smart) { hero.Actions.Add(potion); hero.Actions.Add(new PowerStrikeAction()); }
            var eng = new BattleEngine(new[] { hero }, new[] { SpecContent.CreateEnemy(name) }, rng);
            eng.Start();
            IActionChooser ai = smart ? (IActionChooser)new SmartAi() : new AttackOnlyAi();
            if (eng.RunToEnd(ai, new AttackOnlyAi()) != BattleOutcome.HeroesWon) return false;
            if (pending.Count > 0 && levels.Values.Sum() < 3) { var c = pending.Dequeue(); levels[c] = (levels.TryGetValue(c, out var v) ? v : 0) + 1; }
            var rw = SpecContent.EnemyReward(name); if (rw.Damage > weapon.Damage) weapon = rw;
        }
        return true;
    }

    static void ParityVsPython()
    {
        var classes = (HeroClass[])Enum.GetValues(typeof(HeroClass));
        var builds = from a in classes from b in classes from c in classes select new[] { a, b, c };
        var r = new Random(99); var rng = new SystemRandom(100);
        const int N = 3000;
        var byClass = classes.ToDictionary(c => c, c => new List<double>());
        var smartByClass = classes.ToDictionary(c => c, c => new List<double>());
        foreach (var b in builds)
        {
            byClass[b[0]].Add(Enumerable.Range(0, N).Count(_ => ArenaRun(b, r, rng, false)) / (double)N);
            smartByClass[b[0]].Add(Enumerable.Range(0, N).Count(_ => ArenaRun(b, r, rng, true)) / (double)N);
        }
        double all = byClass.Values.SelectMany(x => x).Average();
        Console.WriteLine($"\nСверка с Python-симулятором (Python: всего 27.1%, Bandit 23.6%, Knight 23.9%, Barbarian 33.8%)");
        Console.WriteLine($"C# (только атака): всего {all:P1} | " + string.Join("  ", classes.Select(c => $"{c} {byClass[c].Average():P1}")));
        Check("шанс прохождения совпадает с Python (±1.5 п.п.)", Math.Abs(all - 0.271) < 0.015, $"({all:P1})");
        Check("Barbarian ≈ 33.8% (±2.5 п.п.)", Math.Abs(byClass[HeroClass.Barbarian].Average() - 0.338) < 0.025);
        Check("Bandit ≈ 23.6% (±2.5 п.п.)", Math.Abs(byClass[HeroClass.Bandit].Average() - 0.236) < 0.025);
        double smart = smartByClass.Values.SelectMany(x => x).Average();
        Console.WriteLine($"C# («умный» игрок: зелья x2 + мощный удар): всего {smart:P1} | " + string.Join("  ", classes.Select(c => $"{c} {smartByClass[c].Average():P1}")));
    }
}
