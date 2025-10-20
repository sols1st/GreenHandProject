using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ending : Interactable
{
    public TMP_Text text;
    private string _endingText;
    private int currentParagraphIndex = 0;
    private List<string> paragraphs;

    public void SetText(string endingText)
    {
        _endingText = endingText;
        currentParagraphIndex = -1; // 设置为 -1 表示不显示任何内容
        paragraphs = SplitTextIntoParagraphs(endingText);
        text.text = ""; // 清空显示
    }

    private List<string> SplitTextIntoParagraphs(string text)
    {
        var paragraphs = new List<string>();
        var lines = text.Split('\n');
        var currentParagraph = "";

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (!string.IsNullOrWhiteSpace(currentParagraph))
                {
                    paragraphs.Add(currentParagraph.Trim());
                    currentParagraph = "";
                }
            }
            else
            {
                if (currentParagraph.Length > 0)
                {
                    currentParagraph += "\n";
                }
                currentParagraph += line;
            }
        }

        if (!string.IsNullOrWhiteSpace(currentParagraph))
        {
            paragraphs.Add(currentParagraph.Trim());
        }

        return paragraphs;
    }

    private void UpdateDisplay()
    {
        if (paragraphs == null || paragraphs.Count == 0)
        {
            text.text = "";
            return;
        }

        // 显示到当前段落的所有内容
        var displayText = "";
        for (int i = 0; i <= currentParagraphIndex && i < paragraphs.Count; i++)
        {
            if (displayText.Length > 0)
            {
                displayText += "\n\n";
            }
            displayText += paragraphs[i];
        }
        text.text = displayText;
    }

    public override void TriggerOnClick()
    {
        if (paragraphs == null || paragraphs.Count == 0)
        {
            return;
        }

        // 显示下一段落
        if (currentParagraphIndex < paragraphs.Count - 1)
        {
            currentParagraphIndex++;
            UpdateDisplay();
        }
        else
        {
            // todo 进入最终对话
        }
    }
}
