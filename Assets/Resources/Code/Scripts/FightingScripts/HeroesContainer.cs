using UnityEngine;

[CreateAssetMenu(fileName = "HeroesContainer", menuName = "Heroes/Heroes Container")]
public class HeroesContainer : ScriptableObject
{
    public PlayerData[] heroes;
}
