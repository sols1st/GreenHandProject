using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            CardManager.Instance.GetNewCard(cardName);
            isHover = false; 
        }
        else
        {
            Debug.Log($"卡片 {cardName} 已被获取！");
        }
    }
}