
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardItem : Interactable
{
    private bool _isSelected = false;
    public GameObject background;
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
        if (_cardData.isGot)
        {
            // 已获取的卡牌
            var imagePath = "Card/CardImages/" + _cardData.id;
            cardImage.sprite = Resources.Load<Sprite>(imagePath);
            detailButton.SetActive(true);
        }
        else
        {
            // 未获取的卡牌
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
