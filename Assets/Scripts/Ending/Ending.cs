using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ending : Interactable
{
    public TMP_Text text;
    private string _endingText = "";

    private void OnEnable()
    {
        _endingText = "";
        text.text = "";
    }
    
    public override void TriggerOnClick()
    {
        var currentParagraph = DeductionManager.Instance.GetNextParagraph();
        if (currentParagraph == null)
        {
            gameObject.SetActive(false);
            // DialogueManager.Instance.ContinueAfterBigPoster();
        }
        else
        {
            _endingText += currentParagraph + "\n\n";
            text.text = _endingText;
        }
    }
}
