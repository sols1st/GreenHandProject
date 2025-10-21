using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景加载器 - 负责管理场景切换逻辑
/// 使用单例模式，跨场景不销毁
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // 单例实例，可以通过SceneLoader.Instance访问
    public static SceneLoader Instance;

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁
        }
        else
        {
            Destroy(gameObject); // 如果已有实例，销毁重复的对象
        }
    }

    /// <summary>
    /// 从主菜单开始游戏
    /// 在主菜单的开始按钮中调用
    /// </summary>
    public void StartGame()
    {
        Debug.Log("SceneLoader: 开始游戏，加载第一个场景");
        UnityEngine.SceneManagement.SceneManager.LoadScene("DemoScene");
    }

    /// <summary>
    /// 返回到主菜单
    /// 在游戏结束时调用
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("SceneLoader: 返回主菜单");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// 处理游戏结局
    /// 在对话结束时调用
    /// </summary>
    public void HandleEnding()
    {
        Debug.Log("SceneLoader: 处理游戏结局，返回主菜单");
        ReturnToMainMenu();
    }
}
