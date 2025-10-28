using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueUIManager
{
    private SceneDialogueUI sceneUI;

    public DialogueUIManager(SceneDialogueUI sceneUI)
    {
        this.sceneUI = sceneUI;
    }

    #region 面板控制方法
    public void ShowNarrationPanel()
    {
        if (!HasValidUI()) return;
        sceneUI.DialoguePanel_Narration?.SetActive(true);
    }

    public void ShowMainPanel(bool showPortrait, bool showName)
    {
        if (!HasValidUI()) return;

        if (sceneUI.DialoguePanel_Main != null)
        {
            sceneUI.DialoguePanel_Main.SetActive(true);
            sceneUI.Image_MainPortrait?.gameObject.SetActive(showPortrait);
            sceneUI.Text_MainName?.gameObject.SetActive(showName);
        }
    }

    public void ShowMinorPanel(bool showAvatar, bool showName)
    {
        if (!HasValidUI()) return;

        if (sceneUI.DialoguePanel_Minor != null)
        {
            sceneUI.DialoguePanel_Minor.SetActive(true);
            sceneUI.Image_MinorAvatar?.gameObject.SetActive(showAvatar);
            sceneUI.Text_MinorName?.gameObject.SetActive(showName);
        }
    }

    public void HideAllPanels()
    {
        if (!HasValidUI()) return;
        sceneUI.DialoguePanel_Narration?.SetActive(false);
        sceneUI.DialoguePanel_Main?.SetActive(false);
        sceneUI.DialoguePanel_Minor?.SetActive(false);
    }
    #endregion

    #region 选项控制方法
    public void ShowOptions(string choiceStr)
    {
        if (!HasValidUI()) return;
        if (string.IsNullOrEmpty(choiceStr) || sceneUI.Panel_Options == null)
        {
            HideOptions();
            return;
        }
        sceneUI.Panel_Options.SetActive(true);

        string[] optionParts = choiceStr.Split(';');

        bool hasVisibleOptions = false;// 是否有可见选项
        for (int i = 0; i < Mathf.Min(optionParts.Length, 2); i++)
        {
            string[] optData = optionParts[i].Split(':');
            if (optData.Length >= 2)
            {
                OptionData option = new OptionData
                {
                    OptionText = optData[0],
                    TargetProcess = optData[1],
                    RequireCard = optData.Length > 2 ? optData[2] : "",
                    GainedCard = optData.Length > 3 ? optData[3] : ""
                };

                // 如果选项不需要卡牌，或者需要卡牌且玩家有，就说明有可显示的选项
                if (!option.RequiresCard ||
                    (CardManager.Instance != null && !string.IsNullOrEmpty(option.RequireCard) &&
                     CardManager.Instance.HasCards(new string[] { option.RequireCard })))
                {
                    hasVisibleOptions = true;
                    break;
                }
            }
        }

        if (!hasVisibleOptions)
            {
                Debug.Log("没有可用的选项，隐藏选项面板");
                HideOptions();
                return;
            }

        sceneUI.Panel_Options.SetActive(true);

        // 处理选项1
        if (optionParts.Length >= 1 && sceneUI.Button_Option1 != null)
        {
            SetupOptionButton(sceneUI.Button_Option1.gameObject, optionParts[0], 0);
        }
        else
        {
            sceneUI.Button_Option1?.gameObject.SetActive(false);
        }
        // 处理选项2
        if (optionParts.Length >= 2 && sceneUI.Button_Option2 != null)
        {
            SetupOptionButton(sceneUI.Button_Option2.gameObject, optionParts[1], 1);
        }
        else
        {
            sceneUI.Button_Option2?.gameObject.SetActive(false);
        }
    }

    private void SetupOptionButton(GameObject buttonObject, string optionStr, int optionIndex)
    {
        string[] optData = optionStr.Split(':');

        if (optData.Length < 2)
        {
            Debug.LogError($"选项格式错误：{optionStr}");
            buttonObject.SetActive(false);
            return;
        }

        // 创建选项数据
        OptionData option = new OptionData
        {
            OptionText = optData[0],
            TargetProcess = optData[1],
            RequireCard = optData.Length > 2 ? optData[2] : "",
            GainedCard = optData.Length > 3 ? optData[3] : ""
        };

        //是否需要卡牌且玩家是否拥有
        if (option.RequiresCard)
        {
            if (CardManager.Instance != null && !string.IsNullOrEmpty(option.RequireCard) &&
        CardManager.Instance.HasCards(new string[] { option.RequireCard }))
            {
                buttonObject.SetActive(true);
            }
            else
            {
                buttonObject.SetActive(false);
                return;
            }
        }

        // 设置按钮文本
        TMP_Text buttonText = optionIndex == 0 ? sceneUI.Text_Option1 : sceneUI.Text_Option2;
        if (buttonText != null)
            buttonText.text = option.OptionText;

        // 设置交互组件
        OptionButtonInteract interact = buttonObject.GetComponent<OptionButtonInteract>();
        if (interact == null)
            interact = buttonObject.AddComponent<OptionButtonInteract>();
        interact.SetOptionData(option);
    }

    public void HideOptions()
    {
        if (!HasValidUI()) return;
        sceneUI.Panel_Options?.SetActive(false);
    }
    #endregion

    private bool HasValidUI()
    {
        return sceneUI != null;
    }
}
