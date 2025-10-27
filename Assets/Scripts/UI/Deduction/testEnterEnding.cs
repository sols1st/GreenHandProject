using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testEnterEnding : Interactable
{
    public string ending;

    public override void TriggerOnClick()
    {
       DeductionManager.Instance.EnterEnding(ending);
    }
}
