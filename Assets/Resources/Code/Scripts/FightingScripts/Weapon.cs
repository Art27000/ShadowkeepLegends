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
}
