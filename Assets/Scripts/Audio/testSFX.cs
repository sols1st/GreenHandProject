using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testSFX : Interactable
{
    public string sfxName;
    public override void TriggerOnClick()
    {
        AudioManager.Instance.PlaySoundEffect(sfxName);
    }
}
