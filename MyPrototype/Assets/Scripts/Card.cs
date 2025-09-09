using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int CardId { get; private set; }
    public bool IsFlipped { get; private set; }

    private Animation _animation;
    private LevelManager _levelManager;
    private Sprite _cardFrontImage;
    private Image _cardImage;
    [SerializeField]
    private Sprite cardBackImage;

    //Action
    public Action<Card> OnFlipped;

    void Awake()
    {
        _animation = GetComponent<Animation>();
        _cardImage = GetComponent<Image>();
    }
    private void Start()
    {
        _levelManager = LevelManager.Instance;
    }

    public void Setup(int cardId, Sprite frontImage, bool isFlipped)
    {
        CardId = cardId;
        _cardFrontImage = frontImage;
        GetComponent<Button>().onClick.AddListener(OnClicked);
        if (isFlipped)
        {
            ShowFront();
        }
        else
        {
            ShowBack();
        }
    }

    private void OnClicked()
    {
        Debug.Log("OnClicked");
        if (_levelManager.isShowCardCompleted)
        {
            OnFlipped?.Invoke(this);
        }
    }

    public void ShowFront(bool isInit = false)
    {
        _animation.Play("Flip");
        Debug.Log("ShowFront");
        _cardImage.sprite = _cardFrontImage;
        if (!isInit)
        {
            IsFlipped = true;
        }
    }

    public void ShowBack(bool isInit = false)
    {
        _animation.Play("Flip");
        Debug.Log("ShowBack");
        _cardImage.sprite = cardBackImage;
        if (!isInit)
        {
            IsFlipped = false;
        }
    }
}