using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public Card Card;
    public MMF_Player Feedback_newSpawn;

    public List<Card> cardsInQueue;

    void Awake()
    {
        cardsInQueue = new List<Card>();
    }

    public void SpawnNew()
    {
        if(Card == null)
        {
            //TODO: feedback here

            if(cardsInQueue.Count > 0)
            {
                Card = cardsInQueue.Last();
                cardsInQueue.RemoveAt(cardsInQueue.Count - 1);
            }

            // if(Card != null)
            // {
            //     Card.gameObject.SetActive(true);

            //     Card.transform.SetParent(transform, false);

            //     Card.transform.localPosition = Vector3.zero;
            // }
        }
    }

    public void RearrangeCards()
    {
        Vector3 pos = Vector3.zero;

        foreach (var card in cardsInQueue)
        {
            card.transform.localPosition = pos;
            pos.y += card.CardHeight;
            pos.z += 0.2f;
        }

        if(this.Card != null)
        {
            Card.transform.localPosition = pos;
        }
    }

    public void QueueCard(Card card)
    {
        cardsInQueue.Add(card);
        card.gameObject.SetActive(true);
        card.transform.SetParent(transform);

        RearrangeCards();
    }
    private void PlayFeedback_Spawn()
    {
        if(Feedback_newSpawn!= null)
        {
            Feedback_newSpawn.PlayFeedbacks();
        }
    }
}