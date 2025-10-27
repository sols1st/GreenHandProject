using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    [SerializeField] private string cardName;
    public override void TriggerOnClick()
    {
        string[] cardsToCheck = { cardName };
        Debug.Log($"1 {cardName}");
        if (!CardManager.Instance.HasCards(cardsToCheck))
        {
            Debug.Log($"2 {cardName}");
            CardManager.Instance.GetNewCard(cardName);
        }
        else
        {
            Debug.Log($"3 {cardName}");
            Debug.Log($"卡片 {cardName} 已被获取！");
        }
    }
}