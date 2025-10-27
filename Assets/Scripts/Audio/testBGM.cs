using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testBGM : Interactable
{
    public string bgmName;

    public override void TriggerOnClick()
    {
        AudioManager.Instance.PlayBackgroundMusic(bgmName);
    }
}
