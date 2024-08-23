using UnityEngine;

public class Card : MonoBehaviour
{
    public CardTypes CardType;
    public MeshRenderer meshRenderer;
    public float CardHeight;
    public Animator animator;
    public string exitAnimationTag;

    public void Init(CardTypes cardType, Material material)
    {
        CardType = cardType;
        UpdateMaterial(material);
    }

    private void UpdateMaterial(Material material)
    {
        meshRenderer.material = material;
    }

    /// <summary>
    /// positions between 1,2 and 3
    /// </summary>
    /// <param name="pos"></param> <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    public void PlayExitAnimation(int pos)
    {
        animator.enabled = true;
        animator.Play(exitAnimationTag+pos);
    }
}