using UnityEngine;

public class YesButton : Interactable
{
    public GameObject nextButton;
    public GameObject background;
    public GameObject window;
    public GameObject endChoiceCanvas;
    public override void TriggerOnClick()
    {
        // todo 进入推理
        nextButton.SetActive(false);
        background.SetActive(false);
        CursorManager.Instance.DisableCustomCursor(); // 测试使用
        DeductionManager.Instance.ShowDeductionCanvas();
        window.SetActive(false);
        endChoiceCanvas.SetActive(false);
    }
}
