
public class TestEnterButton : Interactable
{
    public int chapter;
    public override void TriggerOnClick()
    {
        DeductionManager.Instance.UpdateChapter(chapter);
        DeductionManager.Instance.ShowDeductionCanvas();
        gameObject.SetActive(false);
    }
}
