using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CollectionSystem : MonoBehaviour
{
    [SerializeField] private LayerMask spawnlayer;
    private List<CollectionPoint> collectionPoints;
    private Deck[][] decks;
    private List<CardSpawner> spawners;

    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private Vector3 rotationOffset = new Vector3(180, 0, 0);
    [SerializeField] private Transform VanishingPoint;

    void Awake()
    {
        EventController.StartListening(GameEvent.EVENT_LEVEL_SPAWNED, OnLevelSpawned);
    }

    void OnDestroy()
    {
        EventController.StopListening(GameEvent.EVENT_LEVEL_SPAWNED, OnLevelSpawned);        
    }

    private void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            StartCoroutine(OnPointerUp());
        }
    }

    public IEnumerator OnPointerUp()
    {
        Vector2 position = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(position);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, spawnlayer))
        {
            CardSpawner cardSpawner = hitInfo.collider.gameObject.GetComponent<CardSpawner>();

            if (cardSpawner != null)
            {
                if(cardSpawner.Card!= null)
                {
                    CollectionPoint vacantPoint = GetVacantCollectionPoint();

                    Card cardToMove = cardSpawner.Card;

                    cardSpawner.Card = null;
                    vacantPoint.Card = cardToMove;

                    cardToMove.transform.DOMove(vacantPoint.transform.position, moveDuration);
                    cardToMove.transform.DORotate(cardToMove.transform.eulerAngles + rotationOffset, moveDuration);

                    yield return new WaitForSeconds(moveDuration + 0.1f);

                    cardToMove.transform.SetParent(vacantPoint.transform);

                    if(LevelSpawner.Instance.CurrentLevelConfig.IsSpawnIndependent)
                    {
                        cardSpawner.SpawnNew();
                    }
                    else
                    {
                        if(areAllSpawnersEmpty())
                        {
                            foreach (CardSpawner spawner in spawners)
                            {
                                spawner.SpawnNew();                                
                            }
                        }
                    }

                    StartCoroutine(GrabSimilarFromDecks());
                }
            }
        }
    }

    private IEnumerator GrabSimilarFromDecks()
    {
        List<int> vacantIndices = new List<int>();

        for(int i = collectionPoints.Count - 1; i >= 0; i--)
        {
            if(collectionPoints[i].Card == null)
            {
                vacantIndices.Add(i);
            }
        }

        for(int c = collectionPoints.Count - 1; c >= 0; c--)
        {
            if(collectionPoints[c].Card != null)
            {
                CardTypes cardTypeToCollect = collectionPoints[c].Card.CardType;

                for(int i = 0; i < decks.GetLength(0); i++)
                {
                    for(int j = 0; j < decks[i].GetLength(0); j++)
                    {
                        Deck deck = decks[i][j];

                        for(int k = deck.Cards.Count - 1; k >= 0; k--)
                        {
                            Card card = deck.Cards[k];

                            if(card.CardType == cardTypeToCollect)
                            {
                                yield return new WaitForSeconds(0.1f);

                                if(vacantIndices.Count > 0)
                                {
                                    int lowestIndex = vacantIndices[vacantIndices.Count - 1];

                                    collectionPoints[lowestIndex].Card = card;

                                    deck.Cards.Remove(card);

                                    card.transform.SetParent(collectionPoints[lowestIndex].transform);

                                    card.transform.DOLocalJump(Vector3.zero, 5,1, moveDuration);
                                    Vector3 rotation = card.transform.eulerAngles;
                                    rotation += rotationOffset;

                                    card.transform.DORotate(rotation,moveDuration);

                                    yield return new WaitForSeconds(moveDuration + 0.1f);

                                    vacantIndices.Remove(lowestIndex);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        yield return StartCoroutine(SortCollectionPoints());

        StartCoroutine(RemoveMatchedCards());
    }

    private IEnumerator SortCollectionPoints()
    {
        List<Card> availableCards = new List<Card>();

        foreach (CollectionPoint cp in collectionPoints)
        {
            if(cp.Card!= null)
            {
                availableCards.Add(cp.Card);
            }
        }

        if(availableCards.Count == 0)
        {
            yield return null;
        }

        availableCards.Sort((a, b) => b.CardType.CompareTo(a.CardType));

        for(int i = 0; i < collectionPoints.Count; i++)
        {
            if(i < availableCards.Count)
            {
                collectionPoints[i].Card = availableCards[i];

                collectionPoints[i].Card.transform.SetParent(collectionPoints[i].transform);

                collectionPoints[i].Card.transform.DOLocalMove(Vector3.zero, moveDuration);
            }
            else
            {
                collectionPoints[i].Card = null;
            }
        }

        yield return new WaitForSeconds(moveDuration + 0.2f);
    }

    private IEnumerator RemoveMatchedCards()
    {
        bool isMatchFound = false;

        Dictionary<CardTypes, List<CollectionCard>> matchedCards = new Dictionary<CardTypes, List<CollectionCard>>();

        for(int i = 0; i < collectionPoints.Count; i++)
        {
            if(collectionPoints[i].Card!= null)
            {
                Card card = collectionPoints[i].Card;

                if(!matchedCards.ContainsKey(card.CardType))
                {
                    matchedCards.Add(card.CardType, new List<CollectionCard>());
                }

                CollectionCard collectionCard = new CollectionCard();

                collectionCard.Card = card;
                collectionCard.Point = collectionPoints[i];

                matchedCards[card.CardType].Add(collectionCard);
            }
        }

        foreach(KeyValuePair<CardTypes, List<CollectionCard>> pair in matchedCards)
        {
            if(pair.Value.Count >= 3)
            {
                isMatchFound = true;

                int deletedCount = 0;

                List<Card> cardsToRemove = new List<Card>();

                foreach(CollectionCard Ccard in pair.Value)
                {
                    Card card = Ccard.Card;

                    cardsToRemove.Add(card);
                    deletedCount++;
                    Ccard.Point.Card = null;

                    card.transform.DOMove(VanishingPoint.position + Vector3.up * card.CardHeight * deletedCount * 2, moveDuration);
                    card.transform.DORotate(card.transform.eulerAngles + rotationOffset, moveDuration);

                    yield return new WaitForSeconds(moveDuration/3 + 0.1f);

                    if(deletedCount == 3)
                    {
                        break;
                    }
                }

                yield return new WaitForSeconds(moveDuration + 0.1f);

                for(int i = cardsToRemove.Count - 1; i >= 0; i--)
                {
                    Destroy(cardsToRemove[i].gameObject);
                }
            }

            if(isMatchFound)
            {
                break;
            }
        }

        yield return StartCoroutine(SortCollectionPoints());

        if(isMatchFound)
        {
            StartCoroutine(GrabSimilarFromDecks());
        }
    }

    private CollectionPoint GetVacantCollectionPoint()
    {
        foreach (CollectionPoint point in collectionPoints)
        {
            if (point.Card == null)
            {
                return point;
            }
        }

        return null;
    }

    private bool areAllSpawnersEmpty()
    {
        foreach(CardSpawner cardSpawner in spawners)
        {
            if(cardSpawner.Card!= null)
            {
                return false;
            }
        }

        return true;
    }

    private void OnLevelSpawned(object arg)
    {
        collectionPoints = LevelSpawner.Instance.collectionPoints;
        spawners = LevelSpawner.Instance.spawnPoints;
        decks = LevelSpawner.Instance.decks;
    }

    class CollectionCard
    {
        public Card Card;
        public CollectionPoint Point;
    } 
}