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
            Debug.LogError("������δ�ҵ� DialogueManager��");
            enabled = false;
        }
    }

    public void SetTargetProcess(string process)
    {
        targetProcess = process;
    }

    public override void TriggerOnClick()
    {
        dialogueManager.OnOptionClicked(targetProcess); // ����ѡ����ת
    }
}