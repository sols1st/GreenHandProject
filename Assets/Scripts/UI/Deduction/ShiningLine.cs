using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShiningLine : MonoBehaviour
{
    public Image image;
    public void Shining()
    {
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

        // 闪烁两次后将物体设置为不可见
        gameObject.SetActive(false);
    }
}
