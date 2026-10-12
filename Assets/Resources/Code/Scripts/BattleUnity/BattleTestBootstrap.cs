using System.Collections.Generic;
using Shadowkeep.Battle;
using UnityEngine;

/// <summary>
/// Быстрый запуск боя для проверки: положи на пустой объект тестовой сцены, назначь героя и врагов и нажми Play.
/// Не зависит от старого GameController.
/// </summary>
public sealed class BattleTestBootstrap : MonoBehaviour
{
    [Header("Герой")]
    public PlayerData heroData;
    [Tooltip("Уровни по классам, в сумме от 1 до 3 (мультикласс).")]
    public int knightLevels = 1, banditLevels = 0, barbarianLevels = 0;

    [Header("Предметы героя")]
    public int potions = 2;
    public int potionHeal = 4;
    public bool powerStrike = true;

    [Header("Враги (можно несколько)")]
    public EnemyData[] enemies;

    [Header("Расстановка")]
    public Transform heroSpawn;
    public Transform enemySpawn;
    [Tooltip("Смещение каждого следующего врага.")]
    public Vector3 enemyStep = new Vector3(1.8f, 0f, 0f);

    private BattleController controller;

    private void Start()
    {
        controller = new GameObject("BattleController").AddComponent<BattleController>();
        controller.RestartRequested += StartBattle;
        StartBattle();
    }

    private void StartBattle()
    {
        if (heroData == null || heroData.weapon == null || enemies == null || enemies.Length == 0)
        {
            Debug.LogError("BattleTestBootstrap: назначь героя (с оружием) и хотя бы одного врага.");
            return;
        }
        if (heroSpawn == null || enemySpawn == null)
        {
            Debug.LogError("BattleTestBootstrap: назначь точки спавна героя и врагов.");
            return;
        }

        var levels = new Dictionary<HeroClass, int>();
        if (knightLevels > 0) levels[HeroClass.Knight] = knightLevels;
        if (banditLevels > 0) levels[HeroClass.Bandit] = banditLevels;
        if (barbarianLevels > 0) levels[HeroClass.Barbarian] = barbarianLevels;
        if (levels.Count == 0) levels[HeroClass.Knight] = 1;

        var hero = CombatantFactory.CreateHero(heroData.name, levels, heroData.weapon);
        if (potions > 0) hero.Actions.Add(new PotionAction(potionHeal, potions));
        if (powerStrike) hero.Actions.Add(new PowerStrikeAction());

        var heroSetups = new List<UnitSetup>
        {
            new UnitSetup { Combatant = hero, Prefab = heroData.prefab, Position = heroSpawn.position + heroData.spawnOffset }
        };

        var counts = new Dictionary<string, int>();
        foreach (var d in enemies) counts[d.enemyName] = counts.ContainsKey(d.enemyName) ? counts[d.enemyName] + 1 : 1;

        var enemySetups = new List<UnitSetup>();
        var seen = new Dictionary<string, int>();
        for (int i = 0; i < enemies.Length; i++)
        {
            var d = enemies[i];
            var c = CombatantFactory.CreateEnemy(d);
            if (counts[d.enemyName] > 1)
            {
                seen[d.enemyName] = seen.ContainsKey(d.enemyName) ? seen[d.enemyName] + 1 : 1;
                c.Name = d.enemyName + " " + seen[d.enemyName];
            }
            enemySetups.Add(new UnitSetup
            {
                Combatant = c,
                Prefab = d.prefab,
                Position = enemySpawn.position + d.spawnOffset + enemyStep * i
            });
        }

        controller.Begin(heroSetups, enemySetups, System.Environment.TickCount);
    }
}
