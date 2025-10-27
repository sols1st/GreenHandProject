using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextChapter : Interactable
{
    private int _nextChapter;
    public GameObject sceneCanvas; // 测试使用
    public override void TriggerOnClick()
    {
        // todo 进入下一章
        AudioManager.Instance.PlayBackgroundMusic("");
        DeductionManager.Instance.UpdateChapter(_nextChapter);
        gameObject.SetActive(false);
        DeductionManager.Instance.CloseDeductionCanvas();
        sceneCanvas.SetActive(true); // 测试使用
    }

    public void UpdateChapter(int currentChapter)
    {
        _nextChapter = currentChapter + 1;
    }
}
