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
    }

    /// <summary>
    /// 退出应用程序
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("点击游戏结束界面，退出应用程序");

        // 通知GlobalEscapeManager界面已关闭
        if (GlobalEscapeManager.Instance != null)
        {
            GlobalEscapeManager.Instance.OnGameEndCanvasClosed();
        }

        // 执行退出
        GlobalEscapeManager.Instance.ExitGame();
    }
}
