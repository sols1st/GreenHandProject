using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEscapeManager : MonoBehaviour
{
    public static GlobalEscapeManager Instance;

    [Header("游戏结束界面预制体")]
    public GameObject gameEndCanvasPrefab;

    private GameObject gameEndCanvasInstance;
    private bool isGameEndCanvasShowing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 全局ESC键检测 - 在任何时候按下ESC都显示游戏结束界面
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameEndCanvasShowing)
        {
            ShowGameEndCanvas();
        }
    }

    /// <summary>
    /// 显示游戏结束界面
    /// </summary>
    public void ShowGameEndCanvas()
    { 
        if (gameEndCanvasPrefab != null)
        {
            if (isGameEndCanvasShowing)
                return;
            gameEndCanvasInstance = Instantiate(gameEndCanvasPrefab);
            isGameEndCanvasShowing = true;
            Debug.Log("ESC键触发显示游戏结束界面");
        }
        else
        {
            Debug.LogError("gameEndCanvasPrefab 未设置");
        }
    }

    /// <summary>
    /// 游戏结束界面关闭回调（由GameEndCanvasController调用）
    /// </summary>
    public void OnGameEndCanvasClosed()
    {
        isGameEndCanvasShowing = false;
        if (gameEndCanvasInstance != null)
        {
            Destroy(gameEndCanvasInstance);
            gameEndCanvasInstance = null;
        }
    }

    /// <summary>
    /// 直接退出游戏
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("退出应用程序");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
