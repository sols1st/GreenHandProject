using UnityEngine;

public class SceneController : MonoBehaviour
{
    public int chapter;
    private void OnEnable()
    {
        // todo 章节信息更新
        CardManager.Instance.UpdateChapter(chapter);
        DeductionManager.Instance.UpdateChapter(chapter);
    }
}
