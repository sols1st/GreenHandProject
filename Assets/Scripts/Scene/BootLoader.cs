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
    [Tooltip("游戏启动后加载的第一个场景")]
    public int firstSceneIndex = 1;

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
        SceneManager.LoadScene(firstSceneIndex, LoadSceneMode.Additive);
        SceneManager.sceneLoaded += OnFirstSceneLoaded;
    }

    /// <summary>
    /// 第一个场景加载完成后的回调
    /// </summary>
    private void OnFirstSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"BootLoader: 场景索引 '{firstSceneIndex}' 加载完成。");
        SceneManager.sceneLoaded -= OnFirstSceneLoaded;
        SceneManager.SetActiveScene(scene);

        // 通知SceneLoader启动完成
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnBootComplete();
        }
    }
}
