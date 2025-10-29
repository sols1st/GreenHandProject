using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAssigned: Interactable
{
    [SerializeField] private string npcName;
    public override void TriggerOnClick()
    {
        Debug.Log($"点击 {npcName}");
        DialogueManager.Instance.StartNPCDialogue(npcName);
    }
}
