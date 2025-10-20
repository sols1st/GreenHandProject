using UnityEngine;

public class DialoguePanelInteract : Interactable
{
    private DialogueManager dialogueManager;
    private bool isTyping;

    void OnEnable()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OnTypingStateChanged += OnTypingStateChanged;
        }
        else
        {
            Debug.LogError("������δ�ҵ� DialogueManager��");
            enabled = false;
        }
    }

    void OnDisable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnTypingStateChanged -= OnTypingStateChanged;
        }
    }

    private void OnTypingStateChanged(bool typing)
    {
        isTyping = typing;
    }

    public override void TriggerOnClick()
    {
        //ѡ����弤��ʱ������Ӧ�Ի�����
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