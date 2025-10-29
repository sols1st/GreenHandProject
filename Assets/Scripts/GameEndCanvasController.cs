using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndCanvasController : MonoBehaviour
{
    private void Update()
    {
        // 鼠标左键点击任意位置退出
        if (Input.GetMouseButtonDown(0))
        {
            ExitGame();
        }

        // ESC键退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }
    }

    /// <summary>
    /// 退出应用程序
    /// </summary>
    private void ExitGame()
    {
        Debug.Log("点击游戏结束界面，退出应用程序");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
