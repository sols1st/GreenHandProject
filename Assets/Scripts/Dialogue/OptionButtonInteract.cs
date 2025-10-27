using UnityEngine;

public class OptionButtonInteract : Interactable
{
    private DialogueManager dialogueManager;
    private OptionData optionData;

    void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("场景中未找到 DialogueManager！");
            enabled = false;
            return;
        }
    }

    public void SetOptionData(OptionData data)
    {
        optionData = data;
    }

    public override void TriggerOnClick()
    {
        if (optionData == null || dialogueManager == null) return;
        Debug.Log($"点击选项: {optionData.OptionText}, 目标进程: {optionData.TargetProcess}");
        // 根据所需卡牌是否为空决定处理方式
        if (string.IsNullOrEmpty(optionData.RequireCard))
        {
            // 不需要卡牌：直接处理
            dialogueManager.ProcessOption(optionData);
        }
        else
        {
            // 需要卡牌：打开卡牌库
            dialogueManager.OpenCardSelection(optionData);
        }
    }
}