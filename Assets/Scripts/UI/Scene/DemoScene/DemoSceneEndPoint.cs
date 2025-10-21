using UnityEngine;

public class DemoSceneEndPoint : Interactable
{
    public GameObject endChoiceWindow;
    public override void TriggerOnClick()
    {
        endChoiceWindow.SetActive(true);
    }
}
