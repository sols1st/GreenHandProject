using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Line : Interactable
{
    public bool isCorrectLine = false;
    public Image image;
    public override void TriggerOnClick()
    {
        if (isCorrectLine)
        {
            // todo UI 变化
            DeductionManager.Instance.CutLine();
        }
        else
        {
            DeductionManager.Instance.ShowNotice(new []{"这里好像没有问题，看看别的连线吧"});
        }
    }
}
