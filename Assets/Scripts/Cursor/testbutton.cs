using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testbutton : Interactable
{
    public bool isEnabled = false;
    public override void TriggerOnClick()
    {
        if (isEnabled)
        {
            CursorManager.Instance.DisableCustomCursor();
            isEnabled = false;
        }
        else
        {
            CursorManager.Instance.EnableCustomCursor();
            isEnabled = true;
        }
            
        
    }
}
