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
    [SerializeField] private float cardVanishDelay = 0.8f;
    [SerializeField] private bool onlyCollectMatches = false;

    public bool IsMatchingInProgress { get; private set; }

    void Awake()
    {
        IsMatchingInProgress = false;
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
        if(IsMatchingInProgress)
        {
            yield return null;
        }

        Vector2 position = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(position);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, spawnlayer))
        {
            CardSpawner cardSpawner = hitInfo.collider.gameObject.GetComponent<CardSpawner>();

            if (cardSpawner != null)
            {
                if(cardSpawner.Card!= null)
                {
                    IsMatchingInProgress = true;

                    CollectionPoint vacantPoint = GetVacantCollectionPoint();

                    Card cardToMove = cardSpawner.Card;

                    cardSpawner.Card = null;
                    vacantPoint.AddCard(cardToMove);

                    // cardToMove.transform.DOMove(vacantPoint.transform.position, moveDuration);
                    // cardToMove.transform.DORotate(cardToMove.transform.eulerAngles + rotationOffset, moveDuration);

                    yield return new WaitForSeconds( 0.1f);

                    cardToMove.transform.SetParent(vacantPoint.transform);

                    if(LevelSpawner.Instance.CurrentLevelConfig.IsSpawnIndependent)
                    {
                        cardSpawner.SpawnNew();
                    }
                    else
                    {
                        // clubbed spawning moved to RemoveMatchedCards()
                    }

                    StartCoroutine(GrabSimilarFromDecks());
                }
            }
        }
    }

    private IEnumerator GrabSimilarFromDecks()
    {
        List<int> vacantIndices = new List<int>();

        Dictionary<CardTypes, List<Card>> similarCards = new Dictionary<CardTypes, List<Card>>();

        for(int i = collectionPoints.Count - 1; i >= 0; i--)
        {
            if(collectionPoints[i].Card == null)
            {
                vacantIndices.Add(i);
            }
            else
            {
                if(!similarCards.ContainsKey(collectionPoints[i].Card.CardType))
                {
                    similarCards.Add(collectionPoints[i].Card.CardType, new List<Card>());
                }

                similarCards[collectionPoints[i].Card.CardType].Add(collectionPoints[i].Card);
            }
        }

        List<Card> cardsAnimationOrder = new List<Card>();

        for(int c = collectionPoints.Count - 1; c >= 0; c--)
        {
            if(collectionPoints[c].Card != null)
            {
                CardTypes cardTypeToCollect = collectionPoints[c].Card.CardType;

                if(onlyCollectMatches && similarCards.ContainsKey(cardTypeToCollect) && similarCards[cardTypeToCollect].Count >= 3)
                {
                    continue;
                }

                for(int i = 0; i < decks.GetLength(0); i++)
                {
                    for(int j = 0; j < decks[i].GetLength(0); j++)
                    {
                        Deck deck = decks[i][j];

                        for(int k = deck.Cards.Count - 1; k >= 0; k--)
                        {
                            Card card = deck.Cards[k];

                            if(card.CardType == cardTypeToCollect )
                            {
                                if(onlyCollectMatches && similarCards.ContainsKey(card.CardType) && similarCards[card.CardType].Count >= 3)
                                {
                                    continue;
                                }
                                // yield return new WaitForSeconds(0.1f);

                                if(vacantIndices.Count > 0)
                                {
                                    int lowestIndex = vacantIndices[vacantIndices.Count - 1];

                                    collectionPoints[lowestIndex].AddCard(card);

                                    deck.Cards.Remove(card);

                                    card.transform.SetParent(collectionPoints[lowestIndex].transform);

                                    cardsAnimationOrder.Add(card);

                                    // card.transform.DOLocalJump(Vector3.zero, 5,1, moveDuration);
                                    Vector3 rotation = card.transform.eulerAngles;
                                    rotation += rotationOffset;

                                    // card.transform.DORotate(rotation,moveDuration);

                                    // yield return new WaitForSeconds(moveDuration + 0.1f);

                                    vacantIndices.Remove(lowestIndex);

                                    similarCards[cardTypeToCollect].Add(card);
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

        yield return StartCoroutine(SortCollectionPoints(cardsAnimationOrder));

        StartCoroutine(RemoveMatchedCards());
    }

    private IEnumerator SortCollectionPoints(List<Card> cardsAnimationOrder = null)
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
                collectionPoints[i].AddCard(availableCards[i]);

                collectionPoints[i].Card.transform.SetParent(collectionPoints[i].transform);

                if(cardsAnimationOrder == null || (cardsAnimationOrder != null && !cardsAnimationOrder.Contains(availableCards[i])))
                {
                    collectionPoints[i].Card.transform.DOLocalMove(Vector3.zero, moveDuration);
                }
            }
            else
            {
                collectionPoints[i].AddCard(null);
            }
        }

        bool isAnythingMoving = false;

        foreach(CollectionPoint cp in collectionPoints)
        {
            if(cp.Card!= null)
            {
                if(cp.Card.transform.localPosition.magnitude > 0.01f)
                {
                    isAnythingMoving = true;
                    break;
                }
            }
        }

        if(isAnythingMoving)
            yield return new WaitForSeconds(moveDuration + 0.1f);

        if(cardsAnimationOrder != null)
        foreach (Card card in cardsAnimationOrder)
        {
            card.transform.DOLocalMove(Vector3.zero, moveDuration);

            yield return new WaitForSeconds(moveDuration + 0.1f);
        }
    }

    private IEnumerator RemoveMatchedCards()
    {
        bool isMatchFound = false;
        int matchCount = LevelSpawner.Instance.CurrentLevelConfig.MatchCount;

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
            if(pair.Value.Count >= matchCount)
            {
                isMatchFound = true;

                int deletedCount = 0;

                List<Card> cardsToRemove = new List<Card>();

                foreach(CollectionCard Ccard in pair.Value)
                {
                    Card card = Ccard.Card;

                    cardsToRemove.Add(card);
                    deletedCount++;
                    Ccard.Point.AddCard(null);

                    card.PlayExitAnimation(deletedCount);

                    if(deletedCount == matchCount)
                    {
                        break;
                    }
                }

                // Sequence sequence = DOTween.Sequence();

                Vector3 pos = cardsToRemove[0].transform.position;
                pos.y = VanishingPoint.position.y;

                // for (int i = 0; i < matchCount; i++)
                // {
                //     sequence.Append(cardsToRemove[i].transform.DOMove(pos, moveDuration));
                //     pos.y += cardsToRemove[i].GetComponent<Card>().CardHeight;
                // }

                // sequence.Play();

                yield return new WaitForSeconds(cardVanishDelay);

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
        else
        {
            IsMatchingInProgress = false;

            if(areAllSpawnersEmpty())
            {
                foreach (CardSpawner spawner in spawners)
                {
                    spawner.SpawnNew();                                
                }
            }
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