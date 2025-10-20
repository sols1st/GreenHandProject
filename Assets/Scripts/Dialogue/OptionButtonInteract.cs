using UnityEngine;

public class OptionButtonInteract : Interactable
{
    private string targetProcess;
    private DialogueManager dialogueManager;

    void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("场景中未找到 DialogueManager！");
            enabled = false;
        }
    }

    public void SetTargetProcess(string process)
    {
        targetProcess = process;
    }

    public override void TriggerOnClick()
    {
        dialogueManager.OnOptionClicked(targetProcess); // 触发选项跳转
    }
}