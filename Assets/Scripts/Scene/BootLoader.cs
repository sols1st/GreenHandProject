using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏启动加载器 - 放在Persistent场景中
/// 管理游戏启动流程和场景加载顺序
/// </summary>

public class BootLoader : MonoBehaviour
{
    [Tooltip("游戏启动后加载的第一个场景名称")]
    public string firstSceneName = "MainMenu";

    void Start()
    {
        ValidateEssentialSingletons();
        LoadFirstScene();
    }

    /// <summary>
    /// 验证所有必要的单例组件是否已正确初始化
    /// </summary>
    private void ValidateEssentialSingletons()
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogError("BootLoader: SceneLoader单例未初始化！");
        }
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("BootLoader: DialogueManager单例未初始化！");
        }
        if (DialogueEventSystem.Instance == null)
        {
            Debug.LogError("BootLoader: DialogueEventSystem单例未初始化！");
        }
        // 在此处添加其他必要单例的验证
    }

    /// <summary>
    /// 检查场景是否已经加载
    /// </summary>
    private bool IsSceneAlreadyLoaded(string sceneName)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (loadedScene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 加载第一个游戏场景
    /// </summary>
    private void LoadFirstScene()
    {
        if (string.IsNullOrEmpty(firstSceneName))
        {
            Debug.LogError("BootLoader: 第一个场景名称未设置！");
            return;
        }
        // 检查是否已经加载了目标场景（热重载等情况）
        if (IsSceneAlreadyLoaded(firstSceneName))
        {
            Debug.Log($"BootLoader: 场景 '{firstSceneName}' 已经加载，无需重复加载。");
            return;
        }
        // 注册场景加载完成回调
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnFirstSceneLoaded;

        // 使用叠加模式加载场景，保持Persistent场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(firstSceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// 第一个场景加载完成后的回调
    /// </summary>
    private void OnFirstSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == firstSceneName)
        {
            Debug.Log($"BootLoader: 场景 '{firstSceneName}' 加载完成。");
            // 注销回调，避免重复调用
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnFirstSceneLoaded;
        }
        // 设置新加载的场景为活动场景
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
        // 通知SceneLoader启动完成
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnBootComplete();
        }
    }
}
