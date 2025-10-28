using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextChapter : Interactable
{
    // private int _nextChapter = 1;
    public override void TriggerOnClick()
    {
        // todo 进入下一章
        AudioManager.Instance.PlayBackgroundMusic("");
        // _nextChapter++;
        // DeductionManager.Instance.UpdateChapter(_nextChapter);
        gameObject.SetActive(false);
        DeductionManager.Instance.CloseDeductionCanvas();
        
    }
    
}
