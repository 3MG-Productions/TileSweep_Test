using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class CardSpawnConfig
{
    [InlineEditor(InlineEditorModes.SmallPreview)]
    public Material Color;
    public int Count;
}