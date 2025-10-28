using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterFinalDeduction : MonoBehaviour
{
    private void OnEnable()
    {
        DeductionManager.Instance.UpdateChapter(4);
        DeductionManager.Instance.ShowDeductionCanvas();
    }
}
