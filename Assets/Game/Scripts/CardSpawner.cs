using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public Card Card;

    public void SpawnNew()
    {
        if(Card == null)
        {
        //TODO: feedback here

        Card = LevelSpawner.Instance.PopCardFromRemaining();

        Card.gameObject.SetActive(true);

        Card.transform.SetParent(transform, false);

        Card.transform.localPosition = Vector3.zero;

        }
    }
}