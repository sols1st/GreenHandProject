using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueItem : Interactable
{
    private string cardName;
    [SerializeField] private string npcName;
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
        DialogueManager.Instance.StartNPCDialogue(npcName);
    }
}
