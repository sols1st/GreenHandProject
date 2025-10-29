using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;
using System.Net;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueData 
{
    public string Process;
    public string Scene;
    public string Character;
    public string Character_Name;
    public string Text;
    public string UI;
    public string Choice;
    public string Next;
}

public class OptionData
{
    public string OptionText;
    public string TargetProcess;
    public string RequireCard;
    public string GainedCard;

    public bool RequiresCard => !string.IsNullOrEmpty(RequireCard);
    public bool GrantsCard => !string.IsNullOrEmpty(GainedCard);
}

public class DialogueManager : MonoBehaviour
{
    // 单例实例
    public static DialogueManager Instance;

    [Header("【对话CSV文件】")]
    public TextAsset main1DialogueCSV; // 第一章主线对话
    public TextAsset main2DialogueCSV; // 第二章主线对话  
    public TextAsset main3DialogueCSV; // 第三章主线对话
    public TextAsset inquiry1DialogueCSV; // 第一章问询对话
    public TextAsset inquiry2DialogueCSV; // 第二章问询对话
    public TextAsset inquiry3DialogueCSV; // 第三章问询对话
    public TextAsset endingDialogueCSV; // 结局对话CSV
    public TextAsset reasoningDialogueCSV; // 推理界面对话CSV

    [Header("【配置项】")]
    public float typeSpeed = 0.05f; // 逐字显示速度
    public string portraitResPath = "Portraits/"; // 重要角色立绘资源路径
    public string avatarResPath = "Avatars/";   // 次要角色头像资源路径
    public string backgroundResPath = "Background/"; // 背景资源路径

    [Header("【游戏结束界面】")]
    public GameObject gameEndCanvasPrefab;
    private GameObject gameEndCanvasInstance; // 游戏结束Canvas实例

    internal SceneDialogueUI currentSceneUI; // 当前场景的UI控制器

    public List<DialogueData> dialogueList = new List<DialogueData>(); // 对话数据列表
    public int currentIndex = 0;  // 当前对话索引

    public event Action<bool> OnTypingStateChanged;

    private OptionData currentOption;//当前选项数据
    private List<OptionData> activeOptions = new List<OptionData>();//激活的选项

    public event Action<string> OnCardGained;//获得卡牌事件
    private DialogueUIManager uiManager;
    private TextDisplayController textDisplayController;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            textDisplayController = new TextDisplayController(this);
        }
        else
        {
            Destroy(gameObject);
        }
        textDisplayController.OnTypingStateChanged += (typing) =>
        {
            OnTypingStateChanged?.Invoke(typing);
        };
    }

    /// <summary>注册当前场景的UI组件</summary>
    public void RegisterSceneUI(SceneDialogueUI sceneUI)
    {
        currentSceneUI = sceneUI;
        uiManager = new DialogueUIManager(sceneUI);
    }

    /// <summary>注销当前场景的UI组件</summary>
    public void UnregisterSceneUI()
    {
        if (currentSceneUI != null)
        {
            currentSceneUI = null;
            uiManager = null;
        }
        textDisplayController.StopCurrentTyping();
    }

    /// <summary>检查当前是否有有效的UI引用</summary>
    private bool HasValidUI()
    {
        if (currentSceneUI == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>加载CSV + 显示首条对话</summary>
    void Start()
    {
        if (Instance != this) return;
        Debug.Log("DialogueManager初始化完成，等待场景控制器触发对话");
    }

    /// <summary>加载CSV数据</summary>
    /// <param name="csvFile">要加载的CSV文件</param>
    private void LoadCsvData(TextAsset csvFile)
    {
        dialogueList.Clear();
        string csvText = csvFile.text;

        // 处理UTF-8 BOM字符
        if (csvText.Length > 0 && csvText[0] == '\uFEFF')
        {
            csvText = csvText.Substring(1);
        }
        StringReader reader = new StringReader(csvFile.text);

        // 跳过表头
        string headerLine = reader.ReadLine();

        // 逐行读取对话数据
        while (reader.Peek() != -1)
        {
            string dataLine = reader.ReadLine().Trim();
            if (string.IsNullOrEmpty(dataLine) || dataLine.StartsWith("//"))
                continue; // 跳过空行和注释行

            string[] dataParts = dataLine.Split(',');

            if (dataParts.Length < 8)
            {
                string[] newDataParts = new string[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i < dataParts.Length)
                        newDataParts[i] = dataParts[i].Trim();
                    else
                        newDataParts[i] = "";
                }
                dataParts = newDataParts;
            }

            // 解析并存储对话数据
            DialogueData newData = new DialogueData
            {
                Process = dataParts[0].Trim(),
                Scene = dataParts[1].Trim(),
                Character = dataParts[2].Trim(),
                Character_Name = dataParts[3].Trim(),
                Text = dataParts[4].Trim(),
                UI = dataParts[5].Trim(),
                Choice = dataParts[6].Trim(),
                Next = dataParts[7].Trim()
            };
            dialogueList.Add(newData);
        }
        //reader.Close();
    }

    /// <summary>加载指定的CSV文件数据</summary>
    /// <param name="csvFile">要加载的CSV文件</param>
    public void LoadDialogueData(TextAsset csvFile)
    {
        if (csvFile == null)
        {
            Debug.LogError("传入的CSV文件为空，无法加载对话数据！");
            return;
        }

        currentIndex = 0;
        LoadCsvData(csvFile);
        ShowCurrentDialogue();
    }

    /// <summary>切换到结局对话</summary>
    public void SwitchToEndingDialogue()
    {
        LoadDialogueData(endingDialogueCSV);
    }

    /// <summary>切换到推理界面对话</summary>
    public void SwitchToReasoningDialogue()
    {
        LoadDialogueData(reasoningDialogueCSV);
    }

    /// <summary>切换到第一个探索场景对话</summary>
    public void SwitchToMain1Dialogue()
    {
        LoadDialogueData(main1DialogueCSV);
    }

    /// <summary>切换到第二个探索场景对话</summary>
    public void SwitchToMain2Dialogue()
    {
        LoadDialogueData(main2DialogueCSV);
    }

    /// <summary>切换到第三个探索场景对话</summary>
    public void SwitchToMain3Dialogue()
    {
        LoadDialogueData(main3DialogueCSV);
    }

    /// <summary>切换到第一章问询对话</summary>
    public void SwitchToInquiry1Dialogue()
    {
        LoadDialogueData(inquiry1DialogueCSV);
    }

    /// <summary>切换到第二章问询对话</summary>
    public void SwitchToInquiry2Dialogue()
    {
        LoadDialogueData(inquiry2DialogueCSV);
    }

    /// <summary>切换到第三章问询对话</summary>
    public void SwitchToInquiry3Dialogue()
    {
        LoadDialogueData(inquiry3DialogueCSV);
    }

    /// <summary>
    /// 显示当前对话
    /// </summary>
    public void ShowCurrentDialogue()
    {
        if (!HasValidUI()) return;

        if (currentIndex >= dialogueList.Count)
        {
            Debug.Log("对话列表已结束！");
            uiManager.HideAllPanels();
            uiManager.HideOptions();
            if (CursorManager.Instance != null)
                CursorManager.Instance.EnableCustomCursor();
            return;
        }
        DialogueData currentData = dialogueList[currentIndex];

        // 显示对话时禁用自定义光标，恢复系统光标
        if (CursorManager.Instance != null)
            CursorManager.Instance.DisableCustomCursor();

        if (currentData.Next == "Ending")
        {
            Debug.Log("对话流程到达结局！");
        }
        string currentUiType = currentData.UI;

        // 重置状态：隐藏所有面板、选项，停止逐字协程
        uiManager.HideAllPanels();
        uiManager.HideOptions();
        textDisplayController.StopCurrentTyping();

        // 根据UI类型（U001-U004）切换面板
        switch (currentUiType)
        {
            case "U001": // 旁白
                uiManager.ShowNarrationPanel();
                textDisplayController.StartTypingEffect(currentSceneUI.Text_Narration, currentData.Text, typeSpeed);
                break;

            case "U002": // 主要角色：主面板+立绘+名称
                uiManager.ShowMainPanel(showPortrait: true, showName: true);
                currentSceneUI.Text_MainName.text = currentData.Character_Name;
                if (!string.IsNullOrEmpty(currentData.Character))
                {
                    string portraitPath = portraitResPath + currentData.Character;
                    Sprite portraitSprite = Resources.Load<Sprite>(portraitPath);

                    if (portraitSprite != null)
                    {
                        currentSceneUI.Image_MainPortrait.sprite = portraitSprite;
                        currentSceneUI.Image_MainPortrait.gameObject.SetActive(true);
                    }
                    else
                    {
                        // 没有立绘资源，隐藏立绘
                        currentSceneUI.Image_MainPortrait.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // Character字段为空，隐藏立绘
                    currentSceneUI.Image_MainPortrait.gameObject.SetActive(false);
                }
                textDisplayController.StartTypingEffect(currentSceneUI.Text_MainDialogue, currentData.Text, typeSpeed);
                break;

            case "U003": // 次要角色：次要面板+头像+名称
                uiManager.ShowMinorPanel(showAvatar: true, showName: true);
                currentSceneUI.Text_MinorName.text = currentData.Character_Name;
                if (!string.IsNullOrEmpty(currentData.Character))
                {
                    string avatarPath = avatarResPath + currentData.Character;
                    Sprite avatarSprite = Resources.Load<Sprite>(avatarPath);

                    if (avatarSprite != null)
                    {
                        currentSceneUI.Image_MinorAvatar.sprite = avatarSprite;
                        currentSceneUI.Image_MinorAvatar.gameObject.SetActive(true);
                    }
                    else
                    {
                        // 没有头像资源，隐藏头像
                        currentSceneUI.Image_MinorAvatar.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // Character字段为空，隐藏头像
                    currentSceneUI.Image_MinorAvatar.gameObject.SetActive(false);
                }
                textDisplayController.StartTypingEffect(currentSceneUI.Text_MinorDialogue, currentData.Text, typeSpeed);
                break;

            case "U004": // 带选项：当前面板+选项
                         // 确保在显示选项前正确设置对话框
                if (string.IsNullOrEmpty(currentData.Character) && string.IsNullOrEmpty(currentData.Character_Name))
                {
                    uiManager.ShowNarrationPanel();
                    textDisplayController.StartTypingEffect(currentSceneUI.Text_Narration, currentData.Text, typeSpeed);
                }
                else if (!string.IsNullOrEmpty(currentData.Character) && IsImportantCharacter(currentData.Character))
                {
                    uiManager.ShowMainPanel(showPortrait: true, showName: true);
                    currentSceneUI.Text_MainName.text = currentData.Character_Name;
                    LoadSpriteToImage(currentSceneUI.Image_MainPortrait, portraitResPath + currentData.Character);
                    textDisplayController.StartTypingEffect(currentSceneUI.Text_MainDialogue, currentData.Text, typeSpeed);
                }
                else
                {
                    uiManager.ShowMinorPanel(showAvatar: true, showName: true);
                    currentSceneUI.Text_MinorName.text = currentData.Character_Name;
                    LoadSpriteToImage(currentSceneUI.Image_MinorAvatar, avatarResPath + currentData.Character);
                    textDisplayController.StartTypingEffect(currentSceneUI.Text_MinorDialogue, currentData.Text, typeSpeed);
                }

                // 显示选项（在文本显示完成后）
                if (!string.IsNullOrEmpty(currentData.Choice))
                {
                    uiManager.ShowOptions(currentData.Choice);
                }
                break;

            case "U005"://大字报触发节点
                uiManager.HideAllPanels();
                uiManager.HideOptions();
                break;

            default:
                Debug.LogError($"未知UI类型「{currentUiType}」，请检查CSV的UI字段！");
                break;
        }

        // 切换背景
        ChangeBackground(currentData.Scene);
    }

    /// <summary>
    /// 跳过逐字显示
    /// </summary>
    public void SkipTyping(DialogueData currentData)
    {
        textDisplayController.SkipTyping();
    }

    public void OnOptionClicked(string targetProcess)
    {
        if (string.IsNullOrEmpty(targetProcess))
        {
            Debug.LogError("选项目标进程为空！");
            return;
        }

        uiManager.HideOptions();

        // 查找目标进程的索引
        for (int i = 0; i < dialogueList.Count; i++)
        {
            if (dialogueList[i].Process == targetProcess)
            {
                currentIndex = i;
                ShowCurrentDialogue();
                return;
            }
        }

        Debug.LogError($"未找到目标进程「{targetProcess}」，请检查CSV的Choice字段！");
        currentIndex++;
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 处理选项（不需要卡牌）
    /// </summary>
    public void ProcessOption(OptionData option)
    {
        uiManager?.HideOptions();

        // 处理获得卡牌
        if (!string.IsNullOrEmpty(option.GainedCard))
        {
            GainCard(option.GainedCard);
        }

        // 跳转到目标进程
        OnOptionClicked(option.TargetProcess);

        currentOption = null;
    }

    /// <summary>
    /// 获得卡牌
    /// </summary>
    /// <param name="cardId"></param>
    private void GainCard(string cardId)
    {
        if (CardManager.Instance != null)
        {
            CardManager.Instance.GetNewCard(cardId);
            Debug.Log($"获得新卡牌: {cardId}");
            OnCardGained?.Invoke(cardId);
        }
        else
        {
            Debug.LogError("CardManager实例未找到，无法获得卡牌！");
        }
    }

    /// <summary>
    /// 处理打开卡牌库选项（需要卡牌）
    /// </summary>
    public void OpenCardSelection(OptionData option)
    {
        uiManager?.HideOptions();
        currentOption = option;

        // 打开卡牌库选择界面
        CardBagManager.Instance.Open("Dialogue");
    }

    /// <summary>
    /// 卡牌选择完成后的回调
    /// </summary>
    public void OnCardSelected(string cardId)
    {
        if (currentOption != null)
        {
            // 检查选择的卡牌是否符合要求
            if (currentOption.RequiresCard && !string.IsNullOrEmpty(currentOption.RequireCard))
            {
                if (cardId == currentOption.RequireCard)
                {
                    ProcessOption(currentOption);
                }
                else
                {
                    uiManager?.HideAllPanels();
                    uiManager?.HideOptions();
                    currentOption = null;
                }
            }
            else
            {
                ProcessOption(currentOption);
            }
        }
    }


    /// <summary>
    /// 判断是否为重要角色
    /// </summary>
    /// <param name="charId"></param>
    /// <returns></returns>
    private bool IsImportantCharacter(string charId)
    {
        List<string> importantChars = new List<string> { "C001", "C002", "C003", "C004" };
        return importantChars.Contains(charId);
    }

    /// <summary>
    /// 启动指定NPC的问询对话（通过GameObject名称）
    /// </summary>
    /// <param name="npcObjectName">NPC的GameObject名称，对应CSV中的Character字段</param>
    public void StartNPCDialogue(string npcId)
    {
        Debug.Log($"开始NPC对话，NPC ID: {npcId}");
        // 根据当前场景自动选择对应的问询对话文件
        string currentScene = GetCurrentSceneId();

        switch (currentScene)
        {
            case "S002": // 第一章场景
                SwitchToInquiry1Dialogue();
                break;
            case "S003": // 第二章场景
                SwitchToInquiry2Dialogue();
                break;
            case "S004": // 第三章场景
                SwitchToInquiry3Dialogue();
                break;
        }
        int npcDialogueIndex = FindNPCFirstDialogue(npcId);

        if (npcDialogueIndex != -1)
        {
            currentIndex = npcDialogueIndex;
            ShowCurrentDialogue();
        }
        else
        {
            Debug.LogWarning($"未找到NPC「{npcId}」的问询对话！请检查：");
        }
    }

    /// <summary>
    /// 获取当前场景ID
    /// </summary>
    private string GetCurrentSceneId()
    {
        Scene currentScene = SceneManager.GetActiveScene();// 优先从活动场景判断
        switch (currentScene.buildIndex)
        {
            case 2: return "S002"; // 第一章
            case 3: return "S003"; // 第二章
            case 4: return "S004"; // 第三章
            default:
                // 备用方案：从对话数据判断
                if (dialogueList.Count > 0 && currentIndex < dialogueList.Count)
                {
                    return dialogueList[currentIndex].Scene;
                }
                return "Unknown";
        }
    }

    /// <summary>
    /// 查找NPC的第一个对话进程
    /// </summary>
    private int FindNPCFirstDialogue(string npcId)
    {
        for (int i = 0; i < dialogueList.Count; i++)
        {
            if (dialogueList[i].Character == npcId)
            {
               return i;
            }
         }
        return -1; // 未找到
    }
    private bool hasShownReasoningIntro = false;//是否已显示推理介绍

    /// <summary>
    /// 启动推理界面对话（自动判断是否是第一次）
    /// </summary>
    public void StartReasoningDialogue()
    {
        if (!hasShownReasoningIntro)
        {
            SwitchToReasoningDialogue(); // 使用专门的推理对话文件
            int reasoningStartIndex = FindFirstProcessOfScene("S007");

            if (reasoningStartIndex != -1)
            {
                currentIndex = reasoningStartIndex;
                ShowCurrentDialogue();
                hasShownReasoningIntro = true;//标记已显示过推理介绍
            }
            else
            {
                Debug.LogError("未找到S007场景的对话数据！");
            }
            return;
        }
    }

    /// <summary>
    /// 查找指定场景的第一个进程
    /// </summary>
    private int FindFirstProcessOfScene(string sceneId)
    {
        for (int i = 0; i < dialogueList.Count; i++)
        {
            if (dialogueList[i].Scene == sceneId)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 加载Sprite到Image
    /// </summary>
    /// <param name="targetImage"></param>
    /// <param name="spritePath"></param>
    private void LoadSpriteToImage(Image targetImage, string spritePath)
    {
        if (targetImage == null)
        {
            Debug.LogError("目标Image组件未赋值！");
            return;
        }

        Sprite targetSprite = Resources.Load<Sprite>(spritePath);
        if (targetSprite != null)
        {
            targetImage.sprite = targetSprite;
            targetImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"未找到Sprite：{spritePath}，请检查Resources路径和文件名！");
            targetImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 处理对话背景
    /// </summary>
    private void ChangeBackground(string sceneId)
    {
        if (!HasValidUI()) return;

        if (sceneId == "S005" || sceneId == "S001")
        {
            string backgroundPath = backgroundResPath + sceneId;
            Sprite backgroundSprite = Resources.Load<Sprite>(backgroundPath);

            if (backgroundSprite != null)
            {
                currentSceneUI.SetBackground(backgroundSprite);
            }
            else
            {
                Debug.LogWarning($"未找到背景Sprite：{backgroundPath}");
                currentSceneUI.SetBackground(null); // 清空背景
            }
        }
        else
        {
            currentSceneUI.SetBackground(null);
        }
    }

    /// <summary>
    /// 处理对话继续
    /// </summary>
    public void ContinueDialogue()
    {
        if (textDisplayController.IsTyping)
        {
            textDisplayController.SkipTyping();
            return;
        }
        DialogueData currentData = dialogueList[currentIndex];

        // 检查是否有选项 - 如果有选项，不自动继续，等待玩家选择
        if (!string.IsNullOrEmpty(currentData.Choice))
        {
            if (currentSceneUI.Panel_Options != null && currentSceneUI.Panel_Options.activeSelf)
            {
                return;
            }
            else
            {
                uiManager.HideAllPanels();
                uiManager.HideOptions();
                return;
            }
        }

        // 只有当选项字段和Next字段均为空时才结束对话
        if (string.IsNullOrEmpty(currentData.Choice) && string.IsNullOrEmpty(currentData.Next))
        {
            uiManager.HideAllPanels();
            uiManager.HideOptions();
            // 对话完全结束时恢复自定义光标
            if (CursorManager.Instance != null)
                CursorManager.Instance.EnableCustomCursor();
            return;
        }

        if (currentData.Next == "Ending")
        {
            Debug.Log($"触发结局处理: {currentData.Process}");
            StartCoroutine(DelayedEnding());
            return;
        }

        // 解析Next字段，跳转到指定进程
        if (!string.IsNullOrEmpty(currentData.Next))
        {
            // 查找目标进程的索引
            for (int i = 0; i < dialogueList.Count; i++)
            {
                if (dialogueList[i].Process == currentData.Next)
                {
                    currentIndex = i;
                    ShowCurrentDialogue();
                    return;
                }
            }

            // 如果找不到目标进程，使用递增索引作为后备
            Debug.LogWarning($"未找到目标进程「{currentData.Next}」，使用索引递增");
            currentIndex++;
        }
        else
        {
            currentIndex++;
        }
        // 检查索引是否越界
        if (currentIndex >= dialogueList.Count)
        {
            Debug.Log("对话列表已结束！");
            uiManager.HideAllPanels();
            uiManager.HideOptions();
            return;
        }

        DialogueData nextData = dialogueList[currentIndex];
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 延迟处理结局
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedEnding()
    {
        uiManager.HideAllPanels();
        uiManager.HideOptions();
        yield return new WaitForSeconds(2.0f);
        //SceneLoader.Instance.HandleEnding();
        ShowGameEndCanvas();
    }

    /// <summary>
    /// 显示游戏结束界面
    /// </summary>
    private void ShowGameEndCanvas()
    {
        if (gameEndCanvasPrefab != null)
        {
            // 直接创建并显示实例
            gameEndCanvasInstance = Instantiate(gameEndCanvasPrefab);
        }
        else
        {
            ExitGame();
        }
    }

    private void ExitGame()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 大字报关闭后继续对话流程
    /// </summary>
    public void ContinueAfterBigPoster()
    {
        Debug.Log("大字报关闭，继续对话流程");
        currentIndex++;

        // 检查是否还有对话
        if (currentIndex < dialogueList.Count)
        {
            ShowCurrentDialogue();
        }
        else
        {
            uiManager.HideAllPanels();
            // 调用场景加载器的结局处理方法
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.HandleEnding();
            }
            else
            {
                Debug.LogError("SceneLoader实例未找到，无法处理结局");
            }
        }
    }

    private void Update()
    {
        if (!HasValidUI()) return;

        HandleShortcuts();
    }

    /// <summary>
    /// 处理快捷键输入
    /// </summary>
    private void HandleShortcuts()
    {
        // 空格键切换对话（同鼠标左键点击）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerDialogueAdvance();
        }

        // F1键直接跳转到当前对话的最后一个进程（测试用）
        if (Input.GetKeyDown(KeyCode.F1))
        {
            JumpToLastProcess();
        }

        // F3键切换结局对话（测试用）
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestEndingDialogue();
        }
    }

    /// <summary>
    /// 触发对话前进（同鼠标左键点击）
    /// </summary>
    private void TriggerDialogueAdvance()
    {
        // 检查选项面板是否激活，激活时不响应空格键
        if (currentSceneUI.Panel_Options != null && currentSceneUI.Panel_Options.activeSelf)
            return;

        if (currentIndex >= dialogueList.Count)
            return;

        DialogueData currentData = dialogueList[currentIndex];
        if (textDisplayController.IsTyping)
        {
            textDisplayController.SkipTyping();
        }
        else
        {
            ContinueDialogue();
        }
    }

    /// <summary>
    /// 跳转到最后一个进程（测试用）
    /// </summary>
    private void JumpToLastProcess()
    {
        if (dialogueList.Count == 0) return;

        Debug.Log($"跳转到最后一个进程，原索引: {currentIndex} -> 新索引: {dialogueList.Count - 1}");

        currentIndex = dialogueList.Count - 1;
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 测试结局对话（测试用）
    /// </summary>
    private void TestEndingDialogue()
    {
        if (endingDialogueCSV == null)
        {
            Debug.LogWarning("结局对话CSV文件未分配！");
            return;
        }

        Debug.Log("切换到结局对话（测试模式）");
        SwitchToEndingDialogue();
    }
}
