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
        // todo 进入推理
        CursorManager.Instance.DisableCustomCursor(); // 测试使用
        DeductionManager.Instance.ShowDeductionCanvas();
    }
}
