using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> Cards = new List<Card>();
    public float DelayBetweenCards = 0.5f;
    public float DelayBetweenDeck = 0.5f;
    public MMF_Player Feedback_spawn;
    public Card Card;

    public void Init(List<Card> cards)
    {
        if(cards != null)
        {
            Cards = cards;
        }

        Vector3 position = Vector3.zero;

        foreach (Card card in Cards)
        {
            card.gameObject.SetActive(true);

            card.transform.SetParent(transform);

            card.transform.localPosition = position;
            position.y += card.CardHeight;
        }
        SpawnNew();
        StartCoroutine(PlayFeedback_spawn());
    }

    public void AddCard(Card card)
    {
        if(Cards == null)
        {
            Cards = new List<Card>();
        }

        Cards.Add(card);
    }

    public void SpawnNew()
    {
        if(Card == null)
        {
        }
            Card = Cards.Count > 0 ? Cards[Cards.Count - 1] : null;

            if(Card != null)
            {
                Cards.Remove(Card);
            }
    }

    public IEnumerator PlayFeedback_spawn()
    {
        if(Feedback_spawn!= null)
        {
            Feedback_spawn.PlayFeedbacks();
        }

        yield return new WaitForSeconds(DelayBetweenDeck);

        foreach (Card card in Cards)
        {
            yield return new WaitForSeconds(DelayBetweenCards);

            card.PlayFeedback_spawn();
        }

        yield return new WaitForSeconds(DelayBetweenCards);

        Card.PlayFeedback_spawn();
    }
}