
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardItem : Interactable
{
    private bool _isSelected = false;
    public GameObject background;
    public TMP_Text cardName;
    public Image cardImage;
    public Sprite defaultSprite;
    public GameObject detailButton;
    private Card _cardData;
    
    public override void TriggerOnClick()
    {
        if (!_cardData.isGot || DeductionManager.Instance.IsSelected(_cardData.id)) return;
        CardBagManager.Instance.UpdateSelectedCard(this, _cardData);
    }

    public void Init(Card cardData)
    {
        _cardData = cardData;
        // todo 加载图片
        // var imagePath = "CardTestFiles/Images" + card.image;
        if (_cardData.isGot)
        {
            // 已获取的卡牌
            cardName.text = _cardData.name;
            var imagePath = "CardTestFiles/Images/CardTestImage";
            if (_cardData.type == "Item")
            {
                imagePath = "Card/Images/物件卡";
            }
            else if (_cardData.type == "Memory")
            {
                imagePath = "Card/Images/记忆卡";
            }
            else if (_cardData.type == "Power")
            {
                imagePath = "Card/Images/势力卡";
            }
            cardImage.sprite = Resources.Load<Sprite>(imagePath);
            detailButton.SetActive(true);
        }
        else
        {
            // 未获取的卡牌
            cardName.text = "";
            cardImage.sprite = defaultSprite;
            detailButton.SetActive(false);
        }
        
    }

    public void UpdateBackground()
    {
        if (_isSelected)
        {
            background.SetActive(false);
            _isSelected = false;
        }
        else
        {
            background.SetActive(true);
            _isSelected = true;
        }
    }

    public string GetCardID()
    {
        return _cardData.id;
    }
    
}
