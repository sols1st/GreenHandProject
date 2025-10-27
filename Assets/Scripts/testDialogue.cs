using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testDialogue : Interactable
{
    public override void TriggerOnClick()
    {
        CardBagManager.Instance.Open("Dialogue");
    }
}
