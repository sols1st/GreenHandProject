using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    private bool isTransitioning = false;

    [Header("场景配置")]
    public int mainMenuSceneIndex = 1;
    public int chapterScene1Index = 2;
    public int chapterScene2Index = 3;
    public int chapterScene3Index = 4;
    public int chapterScene4Index = 5;

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

    private void InitializeTransitionSystem()
    {
        if (transitionCamera != null)
        {
            transitionCamera.enabled = false;
        }

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
        SceneManager.LoadScene(mainMenuSceneIndex, LoadSceneMode.Additive);
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
            yield return null;

        Scene newScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }
    }

    private IEnumerator UnloadSceneAsync(int sceneIndex)
    {
        if (!SceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
            yield break;

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneIndex);
        while (!unloadOp.isDone)
            yield return null;
    }

    private IEnumerator FadeIn()
    {
        if (transitionImage == null) yield break;

        transitionImage.gameObject.SetActive(true);
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
        transitionImage.gameObject.SetActive(false);
    }

    private void SetTransitionCamera(bool enabled)
    {
        if (transitionCamera != null)
        {
            transitionCamera.enabled = enabled;
        }
    }

    public void StartGame()
    {
        if (isTransitioning) return;
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        isTransitioning = true;
        SetTransitionCamera(true);
        yield return FadeIn();

        yield return UnloadSceneAsync(mainMenuSceneIndex);
        yield return LoadSceneAsync(chapterScene1Index);

        ForceHideAllUI();
        yield return null;
        yield return null;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SwitchToMain1Dialogue();
        }

        yield return FadeOut();
        SetTransitionCamera(false);
        isTransitioning = false;
    }

    public void LoadNextChapterScene()
    {
        if (isTransitioning) return;

        Scene currentScene = SceneManager.GetActiveScene();
        int nextSceneIndex = currentScene.buildIndex + 1;

        if (nextSceneIndex <= chapterScene3Index)
        {
            StartCoroutine(LoadNextSceneCoroutine(currentScene.buildIndex, nextSceneIndex));
        }
    }

    private IEnumerator LoadNextSceneCoroutine(int currentSceneIndex, int nextSceneIndex)
    {
        isTransitioning = true;
        SetTransitionCamera(true);
        yield return FadeIn();

        yield return UnloadSceneAsync(currentSceneIndex);
        yield return LoadSceneAsync(nextSceneIndex);

        ForceHideAllUI();
        yield return null;
        yield return null;

        if (DialogueManager.Instance != null)
        {
            switch (nextSceneIndex)
            {
                case 2:
                    DialogueManager.Instance.SwitchToMain1Dialogue();
                    break;
                case 3:
                    DialogueManager.Instance.SwitchToMain2Dialogue();
                    break;
                case 4:
                    DialogueManager.Instance.SwitchToMain3Dialogue();
                    break;
            }
        }

        yield return FadeOut();
        SetTransitionCamera(false);
        isTransitioning = false;
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenuCoroutine());
    }

    private IEnumerator ReturnToMainMenuCoroutine()
    {
        SetTransitionCamera(true);
        yield return FadeIn();

        Scene currentScene = SceneManager.GetActiveScene();
        yield return UnloadSceneAsync(currentScene.buildIndex);
        yield return LoadSceneAsync(mainMenuSceneIndex);

        yield return FadeOut();
        SetTransitionCamera(false);
    }

    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        StartCoroutine(ReloadCurrentSceneCoroutine(currentScene));
    }

    private IEnumerator ReloadCurrentSceneCoroutine(Scene currentScene)
    {
        SetTransitionCamera(true);
        yield return FadeIn();

        yield return UnloadSceneAsync(currentScene.buildIndex);
        yield return LoadSceneAsync(currentScene.buildIndex);

        yield return FadeOut();
        SetTransitionCamera(false);
    }

    public void HandleEnding()
    {
        ReturnToMainMenu();
    }

    private void ForceHideAllUI()
    {
        SceneDialogueUI[] sceneUIs = FindObjectsOfType<SceneDialogueUI>();
        foreach (var sceneUI in sceneUIs)
        {
            if (sceneUI.DialoguePanel_Narration != null)
                sceneUI.DialoguePanel_Narration.SetActive(false);
            if (sceneUI.DialoguePanel_Main != null)
                sceneUI.DialoguePanel_Main.SetActive(false);
            if (sceneUI.DialoguePanel_Minor != null)
                sceneUI.DialoguePanel_Minor.SetActive(false);
            if (sceneUI.Panel_Options != null)
                sceneUI.Panel_Options.SetActive(false);
        }
    }
}