using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThreeMG;
using ThreeMG.Helper.GridSystem;
using ThreeMG.Helper.ObjectPooling;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSpawner : Singleton<LevelSpawner>
{
    [InlineEditor, PropertyOrder(999)]
    [SerializeField] private LevelConfig currentLevelConfig;
    public LevelConfig CurrentLevelConfig => currentLevelConfig;
    [SerializeField] private string tag_cards;

    [BoxGroup("Spawner")]
    [SerializeField] private CardSpawner cardSpawnerPrefab;
    [BoxGroup("Spawner")]
    [SerializeField] private Vector3 spawnPosition;
    [BoxGroup("Spawner")]
    [SerializeField] private float spawnOffset;
    [BoxGroup("Spawner")]
    public List<CardSpawner> spawnPoints { get; private set; }

    [BoxGroup("Collection Point")]
    [SerializeField] private CollectionPoint collectionPointPrefab;
    [BoxGroup("Collection Point")]
    [SerializeField] private Vector3 collectionPosition;
    [BoxGroup("Collection Point")]
    [SerializeField] private float collectionOffset;
    [BoxGroup("Collection Point")]
    public List<CollectionPoint> collectionPoints { get; private set; }
    public Deck[][] decks;

    private List<Card> cardsCollection;

    private void Start()
    {
        cardsCollection = new List<Card>();

        SpawnLevel();
    }

    public void SpawnLevel()
    {
        SpawnLevel(currentLevelConfig);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public Card PopCardFromRemaining(int index = 0)
    {
        Card card = null;

        if (cardsCollection.Count > 0)
        {
            Utilities.Shuffle(cardsCollection);

            card = cardsCollection[index];
            cardsCollection.Remove(card);
        }

        return card;
    }

    private void SpawnLevel(LevelConfig levelConfig)
    {
        SpawnCards(levelConfig, cardsCollection);

        cardsCollection = Utilities.Shuffle(cardsCollection);

        decks = AssignCardsToDecks(levelConfig, cardsCollection);

        SpawnSpawners(levelConfig);

        SpawnCollectionPoints(levelConfig);

        EventController.TriggerEvent(GameEvent.EVENT_LEVEL_SPAWNED, null);
    }

    private void SpawnSpawners(LevelConfig levelConfig)
    {
        Vector3 pos = spawnPosition;

        pos.x = -(levelConfig.SpawnCount * spawnOffset) / 2 + spawnOffset / 2;

        spawnPoints = new List<CardSpawner>();

        List<Card> cardsToSpawn = new List<Card>();

        for(int i = 0; i < levelConfig.SpawnCount; i++)
        {
            GameObject spawner = Instantiate(cardSpawnerPrefab.gameObject, Vector3.zero, Quaternion.identity, transform);

            spawner.name = $"Spawner_{i}";

            spawner.transform.position = pos;

            pos.x += spawnOffset;

            CardSpawner cardSpawner = spawner.GetComponent<CardSpawner>();

            spawnPoints.Add(cardSpawner);
        }

        for(int i = 0; i < cardsCollection.Count; i++)
        {
            spawnPoints[i % levelConfig.SpawnCount].QueueCard(cardsCollection[i]);
        }

        foreach (CardSpawner spawner in spawnPoints)
        {
            spawner.SpawnNew();
        }
    }

    private void SpawnCollectionPoints(LevelConfig levelConfig)
    {
        collectionPoints = new List<CollectionPoint>();

        Vector3 pos = collectionPosition;

        pos.x = -(levelConfig.CollectionTilesCount * collectionOffset) / 2 + collectionOffset / 2;

        for (int i = 0; i < levelConfig.CollectionTilesCount; i++)
        {
            GameObject collectionPoint = Instantiate(collectionPointPrefab.gameObject, Vector3.zero, Quaternion.identity, transform);

            collectionPoint.name = $"CollectionPoint_{i}";

            collectionPoint.transform.position = pos;

            pos.x += collectionOffset;

            CollectionPoint collecter = collectionPoint.GetComponent<CollectionPoint>();

            collectionPoints.Add(collecter);
        }
    }

    private Deck[][] AssignCardsToDecks(LevelConfig levelConfig, List<Card> cardsCollection)
    {
        Deck[][] decks = CreateDecks();

        for (int i = 0; i < decks.GetLength(0); i++)
        {
            for (int j = 0; j < decks[i].GetLength(0); j++)
            {
                List<Card> cardsToAdd = new List<Card>();

                int cardsCount = levelConfig.StackSize.x == -1 ? levelConfig.StackSize.y : Random.Range(levelConfig.StackSize.x, levelConfig.StackSize.y);

                for (int k = 0; k < cardsCount; k++)
                {
                    if (cardsCollection.Count > 0)
                    {
                        Card card = PopCardFromRemaining();
                        cardsToAdd.Add(card);
                    }
                }

                decks[i][j].Init(cardsToAdd);
            }
        }

        return decks;
    }

    private void SpawnCards(LevelConfig levelConfig, List<Card> cardsCollection)
    {
        for (int i = 0; i < levelConfig.Cards.Count; i++)
        {
            CardTypes cardType = (CardTypes)i;
            Material material = levelConfig.Cards[i].Color;

            for (int j = 0; j < levelConfig.CardsMultiplier; j++)
            {
                GameObject cardGO = ObjectPooler.Instance.SpawnFromPool(tag_cards, Vector3.zero, Quaternion.identity);
                Card card = cardGO.GetComponent<Card>();
                card.Init(cardType, material);

                card.name = $"Card_{cardType}({i})";

                card.gameObject.SetActive(false);

                cardsCollection.Add(card);
            }
        }
    }

    private Deck[][] CreateDecks()
    {
        GameObject[][] decksGO = GridManager.Instance.GetTiles(currentLevelConfig.GridSize);

        Deck[][] decks = new Deck[currentLevelConfig.GridSize.x][];

        for (int i = 0; i < currentLevelConfig.GridSize.x; i++)
        {
            decks[i] = new Deck[currentLevelConfig.GridSize.y];

            for (int j = 0; j < currentLevelConfig.GridSize.y; j++)
            {
                decks[i][j] = decksGO[i][j].GetComponent<Deck>();
            }
        }

        return decks;
    }
}

public enum EventId
{
    LEVEL_SPAWNED
}