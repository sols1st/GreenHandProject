using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScene : Interactable
{
    public GameObject background;
    public GameObject window;
    public override void TriggerOnClick()
    {
        background.SetActive(true);
        window.SetActive(true);
    }
}
