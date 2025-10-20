using UnityEngine;

public class DialoguePanelInteract : Interactable
{
    private DialogueManager dialogueManager;
    private bool isTyping;

    void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("场景中未找到 DialogueManager！");
            enabled = false;
        }
    }

    // 由DialogueManager调用，同步逐字显示状态
    public void SetTypingState(bool typing)
    {
        isTyping = typing;
    }

    public override void TriggerOnClick()
    {
        // 选项面板激活时，不响应对话框点击
        if (dialogueManager.Panel_Options.activeSelf)
            return;

        if (dialogueManager.currentIndex >= dialogueManager.dialogueList.Count)
            return;

        DialogueData currentData = dialogueManager.dialogueList[dialogueManager.currentIndex];
        if (isTyping)
            dialogueManager.SkipTyping(currentData); // 跳过逐字
        else
        {
            dialogueManager.currentIndex++;
            dialogueManager.ShowCurrentDialogue(); // 下一条对话
        }
    }
}