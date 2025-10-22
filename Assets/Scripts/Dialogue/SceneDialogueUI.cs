using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 场景对话UI控制器 - 每个场景一个实例
/// 管理本场景的对话UI组件，并在场景加载时注册到DialogueManager
/// </summary>
public class SceneDialogueUI : MonoBehaviour
{
    [Header("【主面板】重要角色/旁白共用")]
    public GameObject DialoguePanel_Main;
    public Image Image_MainPortrait;
    public GameObject Panel_MainName;
    public TMP_Text Text_MainName;
    public TMP_Text Text_MainDialogue;

    [Header("【次要面板】次要角色专用")]
    public GameObject DialoguePanel_Minor;
    public Image Image_MinorAvatar;
    public GameObject Panel_MinorName;
    public TMP_Text Text_MinorName;
    public TMP_Text Text_MinorDialogue;

    [Header("【选项面板】")]
    public GameObject Panel_Options;
    public Button Button_Option1;
    public TMP_Text Text_Option1;
    public Button Button_Option2;
    public TMP_Text Text_Option2;

    [Header("【背景】显示对话背景")]
    public Image BackgroundImage;

    void Start()
    {
        RegisterUIWithDialogueManager();
    }

    private void OnDestroy()
    {
        UnregisterUIFromDialogueManager();
    }

    /// <summary>
    /// 将本场景的UI组件注册到DialogueManager
    /// </summary>
    private void RegisterUIWithDialogueManager()
    {
        DialogueManager.Instance.RegisterSceneUI(this);
    }

    /// <summary>
    /// 从DialogueManager注销本场景的UI组件
    /// </summary>
    private void UnregisterUIFromDialogueManager()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.UnregisterSceneUI();
    }

    /// <summary>
    /// 设置背景图片
    /// </summary>
    public void SetBackground(Sprite backgroundSprite)
    {
        if (backgroundSprite != null)
        {
            BackgroundImage.sprite = backgroundSprite;
            BackgroundImage.gameObject.SetActive(true); // 确保图片可见
        }
        else
        {
            BackgroundImage.sprite = null;
            BackgroundImage.gameObject.SetActive(false); // 隐藏背景
        }
    }
}
