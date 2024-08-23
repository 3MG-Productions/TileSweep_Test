using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> Cards;

    public void Init(List<Card> cards)
    {
        Cards = cards;

        Vector3 position = Vector3.zero;

        foreach (Card card in cards)
        {
            card.gameObject.SetActive(true);

            card.transform.SetParent(transform);

            card.transform.localPosition = position;
            position.z += 0.2f;
            position.y += card.CardHeight;
        }
    }
}