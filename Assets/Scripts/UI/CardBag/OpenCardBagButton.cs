using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenCardBagButton : Interactable
{
    private bool _open = false;
    public Sprite openSprite;
    public Sprite closeSprite;
    public Image image;

    public override void TriggerOnClick()
    {
        if (_open)
        {
            CardBagManager.Instance.Close();
            image.sprite = openSprite;
            _open = false;
        }
        else
        {
            CardBagManager.Instance.Open();
            image.sprite = closeSprite;
            _open = true;
        }
    }

    public bool IsOpen()
    {
        return _open;
    }
}
