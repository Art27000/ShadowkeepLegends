using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/NewWeapon")]
public class Weapon : ScriptableObject
{
    public Button weaponObj;
    public string weaponName;
    public int damage;
    public string damageType;
    public Image weaponImage;

    public int getDamage()
    {
        return damage;
    }

    // Тип урона как enum. Строка damageType в ассетах остаётся прежней, миграция не нужна.
    public DamageType Type =>
        System.Enum.TryParse(damageType, true, out DamageType parsed) ? parsed : DamageType.None;
}
