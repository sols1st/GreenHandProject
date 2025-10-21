using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YesButton : Interactable
{
    public override void TriggerOnClick()
    {
        var cur = transform;
        while (cur != null)
        {
            if (cur.GetComponent<Canvas>() != null)
            {
                cur.gameObject.SetActive(false);
                break;
            }
            cur = cur.parent;
        }
        CardManager.Instance.HideSceneCanvas();
        CardManager.Instance.ShowDeductionCanvas();
    }
}
