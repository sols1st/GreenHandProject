using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public string[] keyCards;
    
    
    public void CheckIsActive()
    {
        if (!gameObject.activeSelf && CardManager.Instance.HasCards(keyCards))
        {
            gameObject.SetActive(true);
        }
    }
}
