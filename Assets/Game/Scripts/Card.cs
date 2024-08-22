using UnityEngine;

public class Card : MonoBehaviour
{
    public CardTypes CardType;
    public MeshRenderer meshRenderer;
    public float CardHeight;

    public void Init(CardTypes cardType, Material material)
    {
        CardType = cardType;
        UpdateMaterial(material);
    }

    private void UpdateMaterial(Material material)
    {
        meshRenderer.material = material;
    }
}