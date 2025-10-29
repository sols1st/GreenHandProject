using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("过渡效果")]
    public Image transitionImage;
    public Camera transitionCamera;
    public float fadeDuration = 0.3f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTransitionSystem();
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 初始化方法
    private void InitializeTransitionSystem()
    {
        if (transitionCamera != null)
        {
            transitionCamera.enabled = false;
        }

        StartCoroutine(InitializeCanvas());
    }

    private IEnumerator InitializeCanvas()
    {
        yield return null;

        if (transitionImage != null)
        {
            Color color = transitionImage.color;
            color.a = 0f;
            transitionImage.color = color;
            transitionImage.gameObject.SetActive(true);
        }
    }

    private void InitializeGame()
    {
        Debug.Log("SceneLoader: 游戏初始化，加载主菜单");
        StartCoroutine(LoadMainMenuCoroutine());
    }
    #endregion

    #region 核心场景管理方法
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

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = true;
        loadOp.priority = 1;
        while (!loadOp.isDone)
        {
            yield return null;
        }
    }
    #endregion

    #region 过渡效果方法
    private IEnumerator FadeIn()
    {
        if (transitionImage == null) yield break;

        if (!transitionImage.gameObject.activeSelf)
        {
            transitionImage.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            Color color = transitionImage.color;
            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            transitionImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Color finalColor = transitionImage.color;
        finalColor.a = 1f;
        transitionImage.color = finalColor;
    }

    private IEnumerator FadeOut()
    {
        if (transitionImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            Color color = transitionImage.color;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            transitionImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Color finalColor = transitionImage.color;
        finalColor.a = 0f;
        transitionImage.color = finalColor;
    }

    private void SetTransitionCamera(bool enabled)
    {
        if (transitionCamera != null)
        {
            transitionCamera.enabled = enabled;
        }
    }
    #endregion

    #region 场景切换方法 - 保持原有顺序
    private IEnumerator LoadMainMenuCoroutine()
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(mainMenuSceneIndex, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        Scene mainMenuScene = SceneManager.GetSceneByBuildIndex(mainMenuSceneIndex);
        if (mainMenuScene.IsValid())
        {
            SceneManager.SetActiveScene(mainMenuScene);
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
        SetTransitionCamera(true);
        yield return StartCoroutine(FadeIn());

        // 保持原有顺序：先卸载后加载
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(mainMenuSceneIndex);
        while (!unloadOp.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(LoadSceneAsync(chapterScene1Index));

        Scene newScene = SceneManager.GetSceneByBuildIndex(chapterScene1Index);
        if (newScene.IsValid() && newScene.isLoaded)
        {
            SceneManager.SetActiveScene(newScene);

            float waitTime = 0f;
            float maxWaitTime = 1.5f;
            while ((DialogueManager.Instance == null || DialogueManager.Instance.currentSceneUI == null) && waitTime < maxWaitTime)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }

            if (DialogueManager.Instance != null && DialogueManager.Instance.currentSceneUI != null)
            {
                DialogueManager.Instance.SwitchToMain1Dialogue();
            }
        }

        yield return StartCoroutine(FadeOut());
        SetTransitionCamera(false);
    }

    /// <summary>
    /// 进入下一章场景
    /// </summary>
    public void LoadNextChapterScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        int currentSceneIndex = currentScene.buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex <= chapterScene3Index)
        {
            StartCoroutine(LoadNextSceneCoroutine(currentSceneIndex, nextSceneIndex));
        }
    }

    private IEnumerator LoadNextSceneCoroutine(int currentSceneIndex, int nextSceneIndex)
    {
        SetTransitionCamera(true);
        yield return StartCoroutine(FadeIn());

        // 保持原有顺序：先加载后卸载
        yield return StartCoroutine(LoadSceneAsync(nextSceneIndex));

        Scene newScene = SceneManager.GetSceneByBuildIndex(nextSceneIndex);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);

            float waitTime = 0f;
            float maxWaitTime = 1.5f;
            while ((DialogueManager.Instance == null || DialogueManager.Instance.currentSceneUI == null) && waitTime < maxWaitTime)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }

            if (DialogueManager.Instance != null && DialogueManager.Instance.currentSceneUI != null)
            {
                switch (nextSceneIndex)
                {
                    case 2: DialogueManager.Instance.SwitchToMain1Dialogue(); break;
                    case 3: DialogueManager.Instance.SwitchToMain2Dialogue(); break;
                    case 4: DialogueManager.Instance.SwitchToMain3Dialogue(); break;
                }
            }
        }

        yield return StartCoroutine(UnloadSceneIfLoaded(currentSceneIndex));
        yield return StartCoroutine(FadeOut());
        SetTransitionCamera(false);
    }

    /// <summary>
    /// 返回到主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenuCoroutine());
    }

    private IEnumerator ReturnToMainMenuCoroutine()
    {
        SetTransitionCamera(true);
        yield return StartCoroutine(FadeIn());

        // 保持原有顺序
        Scene currentScene = SceneManager.GetActiveScene();
        yield return StartCoroutine(UnloadSceneIfLoaded(currentScene.buildIndex));

        yield return StartCoroutine(LoadSceneAsync(mainMenuSceneIndex));

        Scene mainMenuScene = SceneManager.GetSceneByBuildIndex(mainMenuSceneIndex);
        if (mainMenuScene.IsValid())
        {
            SceneManager.SetActiveScene(mainMenuScene);
        }

        yield return StartCoroutine(FadeOut());
        SetTransitionCamera(false);
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
        SetTransitionCamera(true);
        yield return StartCoroutine(FadeIn());

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentScene);
        while (!unloadOp.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(LoadSceneAsync(currentScene.buildIndex));

        Scene newScene = SceneManager.GetSceneByBuildIndex(currentScene.buildIndex);
        SceneManager.SetActiveScene(newScene);

        yield return StartCoroutine(FadeOut());
        SetTransitionCamera(false);
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 处理游戏结局
    /// </summary>
    public void HandleEnding()
    {
        Debug.Log("SceneLoader: 处理游戏结局，返回主菜单");
        ReturnToMainMenu();
    }
    #endregion
}