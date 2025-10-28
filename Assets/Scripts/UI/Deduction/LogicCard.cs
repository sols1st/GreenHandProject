using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogicCard : Interactable
{
    private Card _card;
    public Image image;
    // public TMP_Text text;
    public Sprite defaultSprite;
    public override void TriggerOnClick()
    {
        if (_card != null)
        {
            CardManager.Instance.ShowCardDetail(_card.id);
        }
    }

    public void SetCard(Card card)
    {
        _card = card;
        if (card != null)
        {
            // todo 加载图片
            // var imagePath = "CardTestFiles/Images" + card.image;
            var imagePath = "Card/CardImages/" + card.id;
            image.sprite = Resources.Load<Sprite>(imagePath);
            // text.text = card.name;
        }
        else
        {
            image.sprite = defaultSprite;
            // text.text = "";
        }
    }

    public string GetCardID()
    {
        return _card == null ? "" : _card.id;
    }
}
