using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig", order = 0)]
public class LevelConfig : ScriptableObject
{
    public Vector2Int GridSize;
    public Vector2Int StackSize;
    public int CollectionTilesCount;
    public int MatchCount;
    public int SpawnCount;
    public bool IsSpawnIndependent;
    public int CardsMultiplier;
    public List<CardSpawnConfig> Cards;
}