using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    private string cardName;
    public override void TriggerOnClick()
    {
        cardName = this.gameObject.name;
        Debug.Log($"卡片 {cardName} 被点击！");
        string[] cardsToCheck = { cardName };
        if (!CardManager.Instance.HasCards(cardsToCheck))
        {
            Debug.Log($"卡片 {cardName} 被获取！");
            CardManager.Instance.GetNewCard(cardName);
        }
        else
        {
            Debug.Log($"卡片 {cardName} 已被获取！");
        }
    }
}