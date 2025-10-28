using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCardItem : Interactable
{
    private Card _card = null;
    public Image image;
    public TMP_Text text;
    public Sprite defaultSprite;
    public Sprite lockSprite;

    private void Awake()
    {
        image.sprite = lockSprite;
    }

    public override void TriggerOnClick()
    {
        if (_card != null)
        {
            _card = null;
            image.sprite = defaultSprite;
            text.text = "";
        }
        DeductionManager.Instance.UpdateSelector(gameObject);
        // CardBagManager.Instance.Open(name);
    }

    public void SetCard(Card card)
    {
        _card = card;
        // todo 加载图片
        // var imagePath = "CardTestFiles/Images" + card.image;
        var imagePath = "Card/CardImages/" + card.id;
        image.sprite = Resources.Load<Sprite>(imagePath);
        // text.text = _card.name;
    }

    public void Unlock()
    {
        if (image.sprite == lockSprite)
        {
            image.sprite = defaultSprite;
        }
    }
}
