using MoreMountains.Feedbacks;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public Card Card;
    public MMF_Player Feedback_newSpawn;

    public void SpawnNew()
    {
        if(Card == null)
        {
            //TODO: feedback here
            Card = LevelSpawner.Instance.PopCardFromRemaining();

            if(Card != null)
            {
                Card.gameObject.SetActive(true);

                Card.transform.SetParent(transform, false);

                Card.transform.localPosition = Vector3.zero;
                
                Card.PlayFeedback_spawn();
                PlayFeedback_Spawn();
            }
        }
    }

    private void PlayFeedback_Spawn()
    {
        if(Feedback_newSpawn!= null)
        {
            Feedback_newSpawn.PlayFeedbacks();
        }
    }
}