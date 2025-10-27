using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDetailButton : Interactable
{
    public override void TriggerOnClick()
    {
        var cardItem = FindParentCardItem(transform);

        if (cardItem == null) return;
        var cardID = cardItem.GetCardID();
        CardManager.Instance.ShowCardDetail(cardID);
    }
    
    private CardItem FindParentCardItem(Transform startTransform)
    {
        var current = startTransform;
        while (current != null)
        {
            var cardItem = current.GetComponent<CardItem>();
            if (cardItem != null)
            {
                return cardItem;
            }
            current = current.parent;
        }
        return null;
    }
}
