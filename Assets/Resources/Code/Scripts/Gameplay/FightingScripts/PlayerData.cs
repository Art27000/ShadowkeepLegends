using UnityEngine;

[CreateAssetMenu(fileName = "New Hero", menuName = "Heroes/Hero Data")]
public class PlayerData : ScriptableObject
{
    public string playerClass;
    public int HP_Per_Lvl;
    public Weapon weapon;
    [TextArea] public string Bonus_lvl1;
    [TextArea] public string Bonus_lvl2;
    [TextArea] public string Bonus_lvl3;
    public GameObject prefab;
    [Tooltip("Смещение относительно точки спавна на сцене: компенсирует пивот спрайта, чтобы ноги стояли на земле.")]
    public Vector3 spawnOffset;
}
