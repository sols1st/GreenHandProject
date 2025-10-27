using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseCardButton : Interactable
{
    public override void TriggerOnClick()
    {
        CardBagManager.Instance.UseCard();
    }
}
