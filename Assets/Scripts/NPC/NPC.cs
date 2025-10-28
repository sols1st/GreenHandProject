using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    private string npcName;
    public override void TriggerOnClick()
    {
        Debug.Log($"人物 {npcName} 被点击！");
        npcName = gameObject.name;
        DialogueManager.Instance.StartNPCDialogue(npcName);
    }
}
