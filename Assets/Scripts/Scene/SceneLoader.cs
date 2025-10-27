using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景加载器 - 负责管理场景切换逻辑
/// 使用单例模式，跨场景不销毁
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // 单例实例，可以通过SceneLoader.Instance访问
    public static SceneLoader Instance;

    [Header("场景配置")]
    public int mainMenuSceneIndex = 1;
    public int demoSceneIndex = 2;

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
            return;
        }
    }

    /// <summary>
    /// BootLoader完成启动后调用
    /// </summary>
    public void OnBootComplete()
    {
        Debug.Log("SceneLoader: 启动完成，加载主菜单场景");
    }

    /// <summary>
    /// 从主菜单开始游戏
    /// 在主菜单的开始按钮中调用
    /// </summary>
    public void StartGame()
    {
        Debug.Log("SceneLoader: 开始游戏，加载第一个场景");
        // 卸载主菜单场景，加载DemoScene
        SceneManager.LoadScene(demoSceneIndex, LoadSceneMode.Single);
    }

    /// <summary>
    /// 返回到主菜单
    /// 在游戏结束时调用
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("SceneLoader: 返回主菜单");
        // 获取当前活动场景
        //Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(mainMenuSceneIndex, LoadSceneMode.Single);

    }

    /// <summary>
    /// 处理游戏结局
    /// 在对话结束时调用
    /// </summary>
    public void HandleEnding()
    {
        Debug.Log("SceneLoader: 处理游戏结局，返回主菜单");
        // 暂时直接返回主菜单
        ReturnToMainMenu();
    }

    /// <summary>
    /// 带过渡的场景加载协程
    /// </summary>
    private System.Collections.IEnumerator LoadSceneWithTransition(string unloadScene, string loadScene)
    {
        // 这里可以添加过渡动画逻辑
        Debug.Log($"SceneLoader: 卸载场景 '{unloadScene}' 并加载场景 '{loadScene}'");

        // 卸载当前场景
        AsyncOperation unloadOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(unloadScene);
        while (!unloadOp.isDone)
        {
            yield return null;
        }

        // 加载新场景
        AsyncOperation loadOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadScene, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // 设置新加载的场景为活动场景
        Scene newScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(loadScene);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(newScene);

        // 这里可以结束过渡动画逻辑
        Debug.Log($"SceneLoader: 场景 '{loadScene}' 加载完成");
    }

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Debug.Log($"SceneLoader: 重新加载当前场景 '{currentScene.name}'");
        StartCoroutine(LoadSceneWithTransition(currentScene.name, currentScene.name));
    }
}
