using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidateEndingButton : Interactable
{
    public override void TriggerOnClick()
    {
        DeductionManager.Instance.ValidateEnding();
    }
}
