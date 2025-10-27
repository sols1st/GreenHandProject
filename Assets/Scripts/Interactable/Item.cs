using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    [SerializeField] private string cardName;
    public override void TriggerOnClick()
    {
        string[] cardsToCheck = { cardName };
        if (!CardManager.Instance.HasCards(cardsToCheck))
        {
            CardManager.Instance.GetNewCard(cardName);
        }
        else
        {
            Debug.Log($"卡片 {cardName} 已被获取！");
        }
    }
}