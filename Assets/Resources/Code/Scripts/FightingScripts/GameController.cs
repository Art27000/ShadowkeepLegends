using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("OBJECTS")]
    public GameObject playerObj;
    public GameObject monsterObj;
    public EnemyData[] monsters;
    public HeroesContainer hc;
    PlayerController heroCont;
    EnemyController monsterCont;
    public Canvas canv;
    Button WeaponPrefab;
    public Text weaponName;
    public Text strength;
    public Text agility;
    public Text endurance;
    public Button attack;
    public Text logs;

    [Header("MENU")]
    public GameObject panel;
    public GameObject looseMenu;
    public static bool isRestart = false;
    private CharacterClass index;
    public GameObject chooseMenu;
    public GameObject winMenu;
    public GameObject weaponChooseMenu;
    [Header("UI")]
    public Text HpHero;
    public Text HpMonster;
    public Image HpHeroImage;
    public Image HpMonsterImage;
    public Image weaponImg;
    public Text rewardWeaponName;
    
    CharacterBase attacker;
    CharacterBase defender;
    CharacterBase monster;
    CharacterBase player;
    private float targetHeroFill;
    private float targetMonsterFill;
    private int currentIndex = 0;
    private int[] shuffledIndexes;
    Weapon rewardWeapon = null;

    public static GameController Instance { get; internal set; }

    private void ShuffleMonsters()
    {
        // создаём массив индексов [0..monsters.Length-1]
        shuffledIndexes = Enumerable.Range(0, monsters.Length).ToArray();

        // перемешиваем (Fisher–Yates)
        for (int i = 0; i < shuffledIndexes.Length; i++)
        {
            int rand = Random.Range(i, shuffledIndexes.Length);
            (shuffledIndexes[i], shuffledIndexes[rand]) = (shuffledIndexes[rand], shuffledIndexes[i]);
        }

        currentIndex = 0;
    }
    //SPAWNERS
    public void SpawnHero()
    {
        GameObject prefab = hc.heroes[(int)MenuController.index].prefab;
        Vector2 pos = prefab.transform.position;
        Quaternion rot = prefab.transform.rotation;
        playerObj = Instantiate(prefab, pos, rot);
        heroCont = playerObj.GetComponent<PlayerController>();
        heroCont.SetHc(hc);
        heroCont.LevelUp(MenuController.index);
        heroCont.GenerateStats();
        Debug.Log("Hero totalLevel: " + heroCont.TotalLevel);
    }
    public void SpawnMonster()
    {
        // если ещё не перемешивали
        if (shuffledIndexes == null)
            ShuffleMonsters();

        // если все уже выбраны
        if (currentIndex >= shuffledIndexes.Length)
        {
            winMenu.SetActive(true);
            Debug.LogWarning("Все монстры уже были выбраны!");
            return;

        }

        int ind = shuffledIndexes[currentIndex];
        currentIndex++;
        monsterObj = Instantiate(monsters[ind].prefab);

        monsterCont = monsterObj.GetComponent<EnemyController>();
        if (monsterCont == null)
        {
            Debug.LogError($"Prefab {monsters[ind].prefab.name} has no EnemyController-derived component");
            return;
        }
        monsterCont.GenerateStats();
        Debug.Log(monsterCont.GetMaxHp() + " полученное макс.хп");
    }
    //Setter
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    private void Start()
    {
        //SPAWNING HERO
        SpawnHero();
        //SPAWNING MONSTER
        SpawnMonster();
        InitBattle();
        SetWeaponAndStats();
    }
    public void SetWeaponAndStats()
    {
        if (heroCont.GetWeapon() != null && canv != null && WeaponPrefab == null)
        {
            WeaponPrefab = Instantiate(heroCont.GetWeapon(), canv.transform);
            weaponName.text = heroCont.getWeaponName();
        }
        else
            Debug.LogError("Weapon или Canvas не назначены!");
        strength.text = "Strength: " + heroCont.strength;
        agility.text = "Agility: " + heroCont.agility;
        endurance.text = "Endurance: " + heroCont.endurance;
    }
    private void InitBattle()
    {
        this.player = heroCont;
        this.monster = monsterCont;

        // Определяем, кто ходит первым
        if (player.agility >= monster.agility)
        {
            attacker = player;
            defender = monster;
        }
        else
        {
            attacker = monster;
            defender = player;
        }
        SwitchHpPerc();
    }
    public void OnChooseLevelClick(int ind)
    {
        index = (CharacterClass)ind;
        heroCont.LevelUp((CharacterClass)ind);
        chooseMenu.SetActive(false);
        SwitchHpPerc();
        SetWeaponAndStats();
        heroCont.HealFull();
    }
    public void OnAttackClick()
    {
        StartCoroutine(HeroTurn());
        attack.gameObject.SetActive(false);
    }
    public void OnPauseClick()
    {
        if (panel.activeInHierarchy)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }
    }
    public void OnSettingsClick()
    {
        SceneManager.LoadScene("Settings");
    }
    public void OnRestartClick()
    {
        isRestart = true;
        SceneManager.LoadScene("Menu");
    }
    public void OnQuitClick()
    {
        isRestart = false;
        SceneManager.LoadScene("Menu");
    }
    private void HandleEnemyDeath(CharacterBase defender)
    {
        heroCont.HealFull();
        SwitchHpPerc();
        Debug.Log($"{defender.name} повержен!");
        rewardWeapon = defender.weapon;
        weaponImg.sprite = rewardWeapon.weaponImage.sprite;
        rewardWeaponName.text = rewardWeapon.weaponName;
        if (!chooseMenu.activeInHierarchy && heroCont.TotalLevel < 3)
        {
            chooseMenu.SetActive(true);
        }

        if (monsterObj != null)
        {
            Destroy(monsterObj);
        }
        Debug.Log("Удалили монстра");
        StartCoroutine(WaitForChooseMenuToClose());
    }
    private IEnumerator WaitForChooseMenuToClose()
    {
        // ждём, пока меню активно
        while (chooseMenu.activeInHierarchy)
            yield return null;
        OnEnemyDefeated(monsterCont, heroCont);
        while (weaponChooseMenu.activeInHierarchy)
            yield return null;
        SpawnMonster();
        InitBattle();
        attack.gameObject.SetActive(true);
        // когда меню закрылось — запускаем обработку
    }

    private IEnumerator HeroTurn()
    {
        //SetWeaponAndStats();
        if (player.currentHp <= 0 || monster.currentHp <= 0)
        {
            Debug.Log("Бой уже закончен!");
            yield break;
        }

        if (attacker == player && monster.currentHp > 0 && player.currentHp > 0)
        {
            // --- Урон от яда ---
            if (defender.isPoisoned)
            {
                defender.TakeDamage();
                SwitchHpPerc();
                yield return new WaitForSeconds(2f);

                // Проверка смерти от яда
                if (defender.currentHp <= 0)
                {
                    HandleEnemyDeath(defender);
                    yield break;
                }
            }

            // --- Обычная атака ---
            attacker.Attack();
            attacker.DoAttack(defender, attacker);
            SwitchHpPerc();
            yield return new WaitForSeconds(2f);

            // Проверка смерти от атаки
            if (defender.currentHp <= 0)
            {
                HandleEnemyDeath(defender);
                yield break;
            }

            // Смена ролей
            SwapRoles();

            // --- Автоход монстра ---
            StartCoroutine(MonsterTurn());
        }
        else
        {
            StartCoroutine(MonsterTurn());
        }
    }

    private IEnumerator MonsterTurn()
    {
        if (attacker == monster && monster.currentHp > 0 && player.currentHp > 0)
        {
            attacker.Attack();
            attacker.DoAttack(defender, attacker);
            SwitchHpPerc();
            yield return new WaitForSeconds(2f);
            // Проверка конца боя
            if (defender.currentHp <= 0)
            {
                Debug.Log($"{defender.name} повержен! Вы проиграли!");
                RunOnLoose();
                yield break;
            }

            // Смена ролей обратно к игроку
            SwapRoles();
            StartCoroutine(HeroTurn());
        }
    }

    public void OnEnemyDefeated(CharacterBase enemy, CharacterBase player)
    {
        weaponChooseMenu.SetActive(true);
    }
    public void OnWeaponChange()
    {
        WeaponSwap(player, rewardWeapon);
        weaponChooseMenu.SetActive(false);
    }
    private void WeaponSwap(CharacterBase player, Weapon newWeapon)
    {
        Weapon oldWeapon = player.weapon;
        player.weapon = newWeapon;
        Destroy(WeaponPrefab);
        WeaponPrefab = Instantiate(heroCont.GetWeapon(), canv.transform);
        weaponName.text = heroCont.getWeaponName();
    }
    public void OnCancelWeaponClick()
    {
        weaponChooseMenu.SetActive(false);
    }
    public void RunOnLoose()
    {
        if (panel.activeInHierarchy)
        {
            panel.SetActive(false);
        }
        if (!looseMenu.activeInHierarchy)
        {
            looseMenu.SetActive(true);
        }

    }
    void SwapRoles()
    {
        CharacterBase temp = attacker;
        attacker = defender;
        defender = temp;
    }
    void SwitchHpPerc()
    {
        HpMonster.text = monsterCont.currentHp.ToString();
        HpHero.text = heroCont.currentHp.ToString();
        targetHeroFill = (float)player.currentHp / player.GetMaxHp();
        Debug.Log(player.currentHp + " " + player.GetMaxHp() + " = PLAYERCur + Max");
        StartCoroutine(SmoothHpChange(HpHeroImage, targetHeroFill));
        targetMonsterFill = (float)monster.currentHp / monster.GetMaxHp();
        Debug.Log(monster.currentHp + " " + monster.GetMaxHp() + " = MONSTERCur + Max");
        StartCoroutine(SmoothHpChange(HpMonsterImage, targetMonsterFill));
    }
    IEnumerator SmoothHpChange(Image bar, float target)
    {
        while (Mathf.Abs(bar.fillAmount - target) > 0.001f)
        {
            bar.fillAmount = Mathf.MoveTowards(bar.fillAmount, target, 1f * Time.deltaTime);
            yield return null;
        }
        bar.fillAmount = target;
    }
    private Coroutine logRoutine;

    public void ShowLog(string message)
    {
        // если уже идёт корутина — останавливаем её
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }

        // запускаем новую
        logRoutine = StartCoroutine(ShowLogRoutine(message));
    }
    private IEnumerator ShowLogRoutine(string message)
    {
        logs.text = message;
        Color c = logs.color;
        c.a = 1f;
        logs.color = c;

        // держим 2 секунды
        yield return new WaitForSeconds(2f);

        // плавное исчезновение
        float fadeDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            logs.color = c;
            yield return null;
        }

        c.a = 0f;
        logs.color = c;

        logRoutine = null; // освобождаем ссылку
    }

}
