using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    [InlineEditor]
    [SerializeField] private LevelConfig[] levelConfigs;
}