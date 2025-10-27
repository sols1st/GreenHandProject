using UnityEngine;

public class Knife : Interactable
{
    public GameObject[] lines; 
    public CenterLine centerLine;
    public override void TriggerOnClick()
    {
        isInteractable = false;
        foreach (var line in lines)
        {
            line.SetActive(true);
        }
        centerLine.ShiningLine();
    }

    public void CutLine()
    {
        foreach (var interactable in lines)
        {
            interactable.gameObject.SetActive(false);
        }
        centerLine.SetCutLine();
        gameObject.SetActive(false);
    }
}
