using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHp;
    public int strength;
    public int agility;
    public int endurance;
    public int weaponDamage;
    [TextArea] public string specialAbility; 
    public Weapon reward; 
    public GameObject prefab; 
}
