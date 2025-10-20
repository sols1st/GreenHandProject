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
            Debug.LogError("������δ�ҵ� DialogueManager��");
            enabled = false;
        }
    }

    // ��DialogueManager���ã�ͬ��������ʾ״̬
    public void SetTypingState(bool typing)
    {
        isTyping = typing;
    }

    public override void TriggerOnClick()
    {
        // ѡ����弤��ʱ������Ӧ�Ի�����
        if (dialogueManager.Panel_Options.activeSelf)
            return;

        if (dialogueManager.currentIndex >= dialogueManager.dialogueList.Count)
            return;

        DialogueData currentData = dialogueManager.dialogueList[dialogueManager.currentIndex];
        if (isTyping)
            dialogueManager.SkipTyping(currentData); // ��������
        else
        {
            dialogueManager.currentIndex++;
            dialogueManager.ShowCurrentDialogue(); // ��һ���Ի�
        }
    }
}