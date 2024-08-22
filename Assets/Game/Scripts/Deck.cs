using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public Card[] Cards;

    public void Init(List<Card> cards)
    {
        Cards = cards.ToArray();

        Vector3 position = Vector3.zero;

        foreach (Card card in cards)
        {
            card.transform.SetParent(transform);

            card.transform.localPosition = position;

            position.y += card.CardHeight;
        }
    }
}