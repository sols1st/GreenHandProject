using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchSceneCursor : MonoBehaviour
{
    [SerializeField] string bgmName = null;
    private void OnEnable()
    {
        AudioManager.Instance.PlayBackgroundMusic(bgmName);
        CursorManager.Instance.EnableCustomCursor();    
    }

    private void OnDisable()
    {
        AudioManager.Instance.PlayBackgroundMusic(null);
        CursorManager.Instance.DisableCustomCursor();    
    }

}
