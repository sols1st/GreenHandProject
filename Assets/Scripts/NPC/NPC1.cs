using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC1 : Interactable
{
    public override void TriggerOnClick()
    {
        var cur = transform;
        cur.gameObject.SetActive(false);

    }
}
