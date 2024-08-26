using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> Cards;
    public float DelayBetweenCards = 0.5f;
    public float DelayBetweenDeck = 0.5f;
    public MMF_Player Feedback_spawn;

    public void Init(List<Card> cards)
    {
        Cards = cards;

        Vector3 position = Vector3.zero;

        foreach (Card card in cards)
        {
            card.gameObject.SetActive(true);

            card.transform.SetParent(transform);

            card.transform.localPosition = position;
            position.y += card.CardHeight;
        }

        StartCoroutine(PlayFeedback_spawn());
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
    }
}