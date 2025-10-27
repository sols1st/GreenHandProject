using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CenterLine : MonoBehaviour
{
    public Image image;
    public Sprite defaultLine;
    public Sprite shiningLine;
    public Sprite cutLine;

    private void OnEnable()
    {
        image.sprite = defaultLine;
    }

    public void SetCutLine()
    {
        image.sprite = cutLine;
    }

    public void ShiningLine()
    {
        image.sprite = shiningLine;
        StartCoroutine(FlickerImage());
    }
    
    private IEnumerator FlickerImage()
    {
        for (int i = 0; i < 2; i++)
        {
            image.enabled = false;
            yield return new WaitForSeconds(0.1f);
            image.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        image.sprite = defaultLine;
    }
}
