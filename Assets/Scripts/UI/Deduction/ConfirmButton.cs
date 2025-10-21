using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfirmButton : Interactable
{
    public TMP_Dropdown selectedFirst;
    public TMP_Dropdown selectedSecond;
    public TMP_Text notice;
    private Coroutine hideNoticeCoroutine;
    private Coroutine _hideCanvasCoroutine;
    public override void TriggerOnClick()
    {
        // 停止之前的协程（如果有）
        if (hideNoticeCoroutine != null)
        {
            StopCoroutine(hideNoticeCoroutine);
        }

        // 隐藏notice
        notice.gameObject.SetActive(false);
        var selectedFirstText = selectedFirst.captionText.text;
        var selectedSecondText = selectedSecond.captionText.text;
        // 检查selectedFirst的captionText是否为空
        if (selectedFirstText == "" || selectedSecondText == "")
        {
            notice.text = "Please Select Card";
            notice.gameObject.SetActive(true);
            // 启动新的协程
            hideNoticeCoroutine = StartCoroutine(HideNoticeAfterDelay());
            return;
        }
        // todo ending
        var isCorrect = CardManager.Instance.IsEnding(selectedFirstText, selectedSecondText);
        if (!isCorrect)
        {
            notice.text = "Cards are incorrect";
            notice.gameObject.SetActive(true);
            // 启动新的协程
            hideNoticeCoroutine = StartCoroutine(HideNoticeAfterDelay());
            return;
        }
        CardManager.Instance.ShowEnding();

        StartCoroutine(HideCurrentCanvas());
    }

    // 3秒后隐藏notice的协程
    private IEnumerator HideNoticeAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        notice.gameObject.SetActive(false);
        hideNoticeCoroutine = null; // 清空引用
    }

    private IEnumerator HideCurrentCanvas()
    {
        // 等待一段时间再隐藏 Canvas，确保 Ending Canvas 有时间显示
        yield return new WaitForSeconds(1f);

        var cur = transform;
        while (cur != null)
        {
            if (cur.GetComponent<Canvas>() != null)
            {
                cur.gameObject.SetActive(false);
                break;
            }
            cur = cur.parent;
        }
    }
}
