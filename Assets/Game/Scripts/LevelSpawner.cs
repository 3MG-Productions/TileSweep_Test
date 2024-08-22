using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThreeMG;
using ThreeMG.Helper.GridSystem;
using ThreeMG.Helper.ObjectPooling;
using UnityEngine;

public class LevelSpawner : Singleton<LevelSpawner>
{
    [InlineEditor]
    [SerializeField] private LevelConfig currentLevelConfig;
    [SerializeField] private string tag_cards;

    private void Start()
    {
        SpawnLevel();
    }

    [Button]
    public void SpawnLevel()
    {
        SpawnLevel(currentLevelConfig);
    }

    private void SpawnLevel(LevelConfig levelConfig)
    {
        List<Card> cardsCollection = new List<Card>();

        for (int i = 0; i < levelConfig.Cards.Count; i++)
        {
            CardTypes cardType = (CardTypes)i;
            Material material = levelConfig.Cards[i].Color;

            for (int j = 0; j < levelConfig.SpawnCount; j++)
            {
                GameObject cardGO = ObjectPooler.Instance.SpawnFromPool(tag_cards, Vector3.zero, Quaternion.identity);
                Card card = cardGO.GetComponent<Card>();
                card.Init(cardType, material);

                cardsCollection.Add(card);
            }
        }

        Deck[][] decks = CreateDecks();

        // shuffle cards and distribute them to decks
        cardsCollection = Utilities.Shuffle(cardsCollection);

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
                        cardsToAdd.Add(cardsCollection[0]);
                        cardsCollection.RemoveAt(0);
                    }
                }

                decks[i][j].Init(cardsToAdd);
            }
        }

        foreach (Card card in cardsCollection)
        {
            card.gameObject.SetActive(false);            
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