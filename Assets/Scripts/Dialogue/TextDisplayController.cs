using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TextDisplayController
{
    private TMP_Text currentTextComponent;
    private string fullText;
    private Coroutine typingCoroutine;
    private MonoBehaviour coroutineRunner;

    public bool IsTyping { get; private set; }
    public event Action<bool> OnTypingStateChanged;

    public TextDisplayController(MonoBehaviour runner)
    {
        coroutineRunner = runner;
    }

    /// <summary>
    /// 启动逐字显示
    /// </summary>
    public void StartTypingEffect(TMP_Text targetText, string textContent, float typeSpeed)
    {
        if (targetText == null)
        {
            Debug.LogError("目标文本组件（TMP_Text）未赋值！");
            return;
        }

        // 停止之前的协程
        StopCurrentTyping();

        currentTextComponent = targetText;
        fullText = textContent;
        currentTextComponent.text = "";

        typingCoroutine = coroutineRunner.StartCoroutine(
            TypingCoroutine(targetText, textContent, typeSpeed)
        );
    }

    /// <summary>
    /// 逐字显示协程
    /// </summary>
    private IEnumerator TypingCoroutine(TMP_Text targetText, string textContent, float typeSpeed)
    {
        IsTyping = true;
        OnTypingStateChanged?.Invoke(true);
        targetText.richText = true;
        int currentPos = 0;

        while (currentPos < textContent.Length)
        {
            if (currentPos + 3 < textContent.Length && textContent.Substring(currentPos, 4) == "<br>")
            {
                targetText.text += "<br>";
                currentPos += 4;
            }
            else
            {
                targetText.text += textContent[currentPos];
                currentPos += 1;
            }
            yield return new WaitForSeconds(typeSpeed);
        }

        IsTyping = false;
        OnTypingStateChanged?.Invoke(false);
    }

    /// <summary>
    /// 跳过逐字显示
    /// </summary>
    public void SkipTyping()
    {
        if (!IsTyping || typingCoroutine == null) return;

        coroutineRunner.StopCoroutine(typingCoroutine);
        typingCoroutine = null;
        IsTyping = false;
        OnTypingStateChanged?.Invoke(false);

        if (currentTextComponent != null)
        {
            currentTextComponent.text = fullText;
        }
    }

    /// <summary>
    /// 停止当前逐字显示
    /// </summary>
    public void StopCurrentTyping()
    {
        if (typingCoroutine != null)
        {
            coroutineRunner.StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        IsTyping = false;
        OnTypingStateChanged?.Invoke(false);
    }
}
