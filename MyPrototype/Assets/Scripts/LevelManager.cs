using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GameManager gameManager;
    public bool isShowCardCompleted;

    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Card cardPrefab;

    private Sprite[] _cardSprites;
    private List<Card> _cards;
    private List<Card> _clearedCards;
    private int[] _shuffledCardsIndex;
    private Card _firstSelectedCard;
    private Card _secondSelectedCard;

    private Vector2Int _gridSize;
    private int _score = 10; // user will get 10 point when match correct pair  
    private int _comboMultiplier = 0;//It will be increase when user match correct pair in sequence. If user give 2 correct pair in sequence then score will be like 2(combo)*10(basic score).


    private List<CardData> savedCardData = null;

    //Actions
    public Action<Vector2Int, List<Card>> OnLevelCreated;
    public Action<Vector2Int, List<Card>> OnCardFlipped;
    public Action<Vector2Int> OnLevelFinished;

    private void Awake()
    {
        Instance = this;
        _cardSprites = Resources.LoadAll<Sprite>("AllCards"); // Get all sprites from Resources/AllCards folder.

    }

    // Start is called before the first frame update
    void Start()
    {
        isShowCardCompleted = false;
    }

    public void StartLevel(Vector2Int gridSize)
    {
        _gridSize = gridSize;
        SpawnCards(_gridSize); // rows, columns
        StartCoroutine(ShowAllCardsOnStart(_gridSize));
    }

    private void SpawnCards(Vector2Int gridSize)
    {
        var rows = gridSize.x;
        var columns = gridSize.y;
        _shuffledCardsIndex = new int[rows * columns];
        for (var i = 0; i < _shuffledCardsIndex.Length; i++)
        {
            _shuffledCardsIndex[i] = i / 2; // Setup pair of cards.
        }

        //Get level from file otherwise create new one
        if (SaveManager.Instance.LoadData(gridSize, out var savedData))
        {
            savedCardData = savedData.cards;
            _shuffledCardsIndex = savedCardData.Select(card => card.cardId).ToArray();
        }
        else
        {
            ShuffleCards();
        }

        // update grid layout
        gridLayoutGroup.constraintCount = rows;

        _cards = new List<Card>(rows * columns);
        _clearedCards = new List<Card>(_cards.Capacity);

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                var cell = i * columns + j;
                var card = Instantiate(cardPrefab, gridLayoutGroup.transform);
                var cardId = _shuffledCardsIndex[cell];
                var frontImage = _cardSprites[cardId];
                var isFlipped = savedCardData != null && savedCardData[cell].isFlipped;
                var isMatched = savedCardData != null && savedCardData[cell].isMatched;
                card.Setup(cardId, frontImage, isFlipped);
                card.OnFlipped += CardFlipped;
                _cards.Add(card);

                if (isMatched)
                {
                    _clearedCards.Add(card);
                }
            }
        }

        // Update First selected in case player selected from last session
        FindFirstSelected();

        OnLevelCreated?.Invoke(gridSize, _cards);
    }

    public void CardFlipped(Card selectedCard)
    {
        if (selectedCard.IsFlipped)
        {
            return;
        }

        SoundManager.Instance.CardflipSfx();
        selectedCard.ShowFront();
        OnCardFlipped?.Invoke(_gridSize, _cards);

        if (_firstSelectedCard == null)
        {
            _firstSelectedCard = selectedCard;
        }
        else if (_secondSelectedCard == null)
        {
            _secondSelectedCard = selectedCard;
            StartCoroutine(CheckCardMatch(_firstSelectedCard, _secondSelectedCard));
        }
    }

    IEnumerator CheckCardMatch(Card firstSelection, Card secondSelection) // Check is card match or not.
    {
        _firstSelectedCard = null;
        _secondSelectedCard = null;
        if (firstSelection.CardId == secondSelection.CardId)
        {
            Debug.Log("Card Match");
            _comboMultiplier++;

            gameManager.ShowScoreCombo(_comboMultiplier);
            gameManager.ScoreAdd(_score * _comboMultiplier);
            SoundManager.Instance.CardMatchSfx();

            _clearedCards.Add(firstSelection);
            _clearedCards.Add(secondSelection);

            CheckForLevelComplete();
        }
        else
        {
            _comboMultiplier = 0;

            gameManager.ShowScoreCombo(_comboMultiplier);
            yield return new WaitForSeconds(0.5f);
            SoundManager.Instance.CardNotMatchSfx();

            Debug.Log("Card Not Match");
            firstSelection.ShowBack();
            secondSelection.ShowBack();

            OnCardFlipped?.Invoke(_gridSize, _cards);
        }
    }

    private void CheckForLevelComplete()
    {
        if (_clearedCards.Count == _cards.Count)
        {
            SoundManager.Instance.GameCompleteSfx();
            _clearedCards.Clear();
            _cards.Clear();
            gameManager.LevelCompleted();
            OnLevelFinished?.Invoke(_gridSize);
        }
    }

    private void ShuffleCards() // Randomly setup cards.
    {
        for (var i = 0; i < _shuffledCardsIndex.Length; i++)
        {
            var randomIndex = Random.Range(0, _shuffledCardsIndex.Length);
            (_shuffledCardsIndex[i], _shuffledCardsIndex[randomIndex]) = (_shuffledCardsIndex[randomIndex], _shuffledCardsIndex[i]);
        }
    }

    IEnumerator ShowAllCardsOnStart(Vector2Int gridSize) // Show all cards front side at the starting of the game
    {
        var rows = gridSize.x;
        var columns = gridSize.y;
        isShowCardCompleted = false;

        yield return new WaitForSeconds(0.2f);

        foreach (Card card in _cards)
        {
            card.ShowFront(true);
        }

        yield return new WaitForSeconds(2f);

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                var cell = i * columns + j;
                if (savedCardData != null)
                {
                    var getSavedCardData = savedCardData[cell];
                    if (getSavedCardData.isMatched) continue;
                    if (getSavedCardData.isFlipped == false)
                    {
                        //show back
                        _cards[cell].ShowBack(true);
                    }
                }
                else
                {
                    _cards[cell].ShowBack(true);
                }

            }
        }
        isShowCardCompleted = true;
    }

    private void FindFirstSelected()
    {
        foreach (var card in _cards)
        {
            if (card.IsFlipped && !_clearedCards.Contains(card))
            {
                _firstSelectedCard = card;
                break;
            }
        }
    }
}