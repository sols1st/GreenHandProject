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
    public static SceneLoader Instance;

    [Header("场景配置")]
    public int mainMenuSceneIndex = 1;
    public int chapterScene1Index = 2;  // 第一章场景
    public int chapterScene2Index = 3;  // 第二章场景
    public int chapterScene3Index = 4;  // 第三章场景
    public int chapterScene4Index = 5;  // 第四章场景

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 游戏初始化
    /// </summary>
    private void InitializeGame()
    {
        Debug.Log("SceneLoader: 游戏初始化，加载主菜单");
        StartCoroutine(LoadMainMenuCoroutine());
    }

    private IEnumerator LoadMainMenuCoroutine()
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(mainMenuSceneIndex, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }
        // 设置主菜单为活动场景
        Scene mainMenuScene = SceneManager.GetSceneByBuildIndex(mainMenuSceneIndex);
        if (mainMenuScene.IsValid())
        {
            SceneManager.SetActiveScene(mainMenuScene);
        }
    }

    /// <summary>
    /// 检查场景是否已经加载
    /// </summary>
    private bool IsSceneLoaded(int sceneIndex)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex == sceneIndex && scene.isLoaded)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 卸载指定场景
    /// </summary>
    private IEnumerator UnloadSceneIfLoaded(int sceneIndex)
    {
        if (IsSceneLoaded(sceneIndex))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneIndex);
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// 从主菜单开始游戏
    /// </summary>
    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        // 卸载主菜单场景
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(mainMenuSceneIndex);
        while (!unloadOp.isDone)
        {
            yield return null;
        }
        // 加载第一章场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(chapterScene1Index, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }
        // 设置新场景为活动场景
        Scene newScene = SceneManager.GetSceneByBuildIndex(chapterScene1Index);
        if (newScene.IsValid() && newScene.isLoaded)
        {
            SceneManager.SetActiveScene(newScene);
            // 触发第一章主线对话
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.SwitchToMain1Dialogue();
            }
        }
    }

    /// <summary>
    /// 进入下一章场景
    /// 在推理界面结束后调用
    /// </summary>
    public void LoadNextChapterScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        int currentSceneIndex = currentScene.buildIndex;

        // 根据当前场景决定下一个场景
        int nextSceneIndex = currentSceneIndex + 1;

        // 检查下一个场景是否在有效范围内
        if (nextSceneIndex <= chapterScene3Index)
        {
            StartCoroutine(LoadNextSceneCoroutine(currentSceneIndex, nextSceneIndex));
        }
    }

    private IEnumerator LoadNextSceneCoroutine(int currentSceneIndex, int nextSceneIndex)
    {
        // 卸载当前场景
        yield return StartCoroutine(UnloadSceneIfLoaded(currentSceneIndex));

        // 加载下一个场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // 设置新场景为活动场景
        Scene newScene = SceneManager.GetSceneByBuildIndex(nextSceneIndex);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);

            // 根据场景索引触发对应对话
            switch (nextSceneIndex)
            {
                case 2: DialogueManager.Instance.SwitchToMain1Dialogue(); break;
                case 3: DialogueManager.Instance.SwitchToMain2Dialogue(); break;
                case 4: DialogueManager.Instance.SwitchToMain3Dialogue(); break;
            }
        }
    }

    /// <summary>
    /// 返回到主菜单
    /// 在游戏结束时调用
    /// </summary>
    public void ReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenuCoroutine());
    }

    private IEnumerator ReturnToMainMenuCoroutine()
    {
        // 卸载当前游戏场景
        Scene currentScene = SceneManager.GetActiveScene();
        yield return StartCoroutine(UnloadSceneIfLoaded(currentScene.buildIndex));

        // 加载主菜单场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(mainMenuSceneIndex, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // 设置主菜单为活动场景
        Scene mainMenuScene = SceneManager.GetSceneByBuildIndex(mainMenuSceneIndex);
        if (mainMenuScene.IsValid())
        {
            SceneManager.SetActiveScene(mainMenuScene);
        }
    }

    /// <summary>
    /// 处理游戏结局
    /// 在结局对话结束时调用
    /// </summary>
    public void HandleEnding()
    {
        Debug.Log("SceneLoader: 处理游戏结局，返回主菜单");
        ReturnToMainMenu();
    }

    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        StartCoroutine(ReloadCurrentSceneCoroutine(currentScene));
    }

    private IEnumerator ReloadCurrentSceneCoroutine(Scene currentScene)
    {
        // 卸载当前场景
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
        {
            yield return null;
        }

        // 加载当前场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(currentScene.buildIndex, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // 设置新加载的场景为活动场景
        Scene newScene = SceneManager.GetSceneByBuildIndex(currentScene.buildIndex);
        SceneManager.SetActiveScene(newScene);
    }
}
