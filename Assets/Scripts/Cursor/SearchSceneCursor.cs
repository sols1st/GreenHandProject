using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchSceneCursor : MonoBehaviour
{
    private void OnEnable()
    {
        CursorManager.Instance.EnableCustomCursor();    
    }

    private void OnDisable()
    {
        CursorManager.Instance.DisableCustomCursor();    
    }

}
