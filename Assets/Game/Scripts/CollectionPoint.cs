using MoreMountains.Feedbacks;
using UnityEngine;

public class CollectionPoint : MonoBehaviour
{
    public Card Card { get; private set; } = null;
    public MMF_Player Feedback_cardAdded;

    public void AddCard(Card card)
    {
        Card = card;

        if(card!= null && Feedback_cardAdded!= null)
        {
            Feedback_cardAdded.PlayFeedbacks();
        }
    }
}