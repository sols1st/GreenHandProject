using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public NPCName npcName;

    public void NPCClicked()
    {
        switch (npcName)
        {
            case NPCName.NPC1:
                this.gameObject.SetActive(false);
                break;
        }
    }
}
