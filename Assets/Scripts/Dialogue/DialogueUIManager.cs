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
        buttonObject.SetActive(true);

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
