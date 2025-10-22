using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;
using System.Net;

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
    public string Card;
}

public class DialogueManager : MonoBehaviour
{
    // 单例实例
    public static DialogueManager Instance;

    [Header("【对话CSV文件】")]
    public TextAsset mainDialogueCSV; // 主线对话CSV
    public TextAsset inquiryDialogueCSV; // 探索问询CSV
    public TextAsset endingDialogueCSV; // 结局对话CSV

    [Header("【配置项】")]
    public float typeSpeed = 0.05f; // 逐字显示速度
    public string portraitResPath = "Portraits/"; // 重要角色立绘资源路径
    public string avatarResPath = "Avatars/";   // 次要角色头像资源路径
    public string backgroundResPath = "Background/"; // 背景资源路径

    internal SceneDialogueUI currentSceneUI; // 当前场景的UI控制器

    public List<DialogueData> dialogueList = new List<DialogueData>(); // 对话数据列表
    public int currentIndex = 0;  // 当前对话索引
    public Coroutine typingCoroutine;  // 逐字显示协程
    public bool isTyping = false;   // 是否正在逐字显示

    public event Action<bool> OnTypingStateChanged;

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
    /// 注册当前场景的UI组件
    /// 由SceneDialogueUI在场景加载时调用
    /// </summary>
    public void RegisterSceneUI(SceneDialogueUI sceneUI)
    {
        currentSceneUI = sceneUI;
        // 如果有等待中的对话，继续显示
        if (dialogueList.Count > 0 && currentIndex < dialogueList.Count)
        {
            // 延迟一帧执行，确保所有组件已初始化
            StartCoroutine(DelayedShowDialogue());
        }
    }

    private IEnumerator DelayedShowDialogue()
    {
        yield return null; // 等待一帧
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 注销当前场景的UI组件
    /// 由SceneDialogueUI在场景销毁时调用
    /// </summary>
    public void UnregisterSceneUI()
    {
        if (currentSceneUI != null)
        {
            currentSceneUI = null;
        }
        // 停止所有协程，避免使用已销毁的UI组件
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    /// <summary>
    /// 检查当前是否有有效的UI引用
    /// </summary>
    private bool HasValidUI()
    {
        if (currentSceneUI == null)
        {
            Debug.LogWarning("DialogueManager: 当前没有有效的UI引用，请确保SceneDialogueUI已注册");
            return false;
        }
        return true;
    }

    // 加载CSV + 显示首条对话
    void Start()
    {
        if (Instance != this) return;

        // 加载主线对话CSV
        if (mainDialogueCSV != null)
        {
            LoadDialogueData(mainDialogueCSV);
        }
        else
        {
            Debug.LogError("主线对话CSV文件未分配");
        }
    }

    /// <summary>
    /// 加载CSV数据
    /// </summary>
    /// <param name="csvFile">要加载的CSV文件</param>
    private void LoadCsvData(TextAsset csvFile)
    {
        dialogueList.Clear();
        StringReader reader = new StringReader(csvFile.text);

        // 跳过表头
        string headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
        {
            Debug.LogError("CSV表头为空，格式错误！");
            return;
        }

        // 逐行读取对话数据
        while (reader.Peek() != -1)
        {
            string dataLine = reader.ReadLine().Trim();
            if (string.IsNullOrEmpty(dataLine) || dataLine.StartsWith("//"))
                continue; // 跳过空行和注释行

            string[] dataParts = dataLine.Split(',');
            if (dataParts.Length != 9)
            {
                Debug.LogWarning($"CSV行格式错误：行内容「{dataLine}」，字段数{dataParts.Length}（需9个）");
                continue;
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
                Next = dataParts[7].Trim(),
                Card = dataParts[8].Trim()
            };
            dialogueList.Add(newData);
        }
        reader.Close();
        Debug.Log($"CSV加载成功，共{dialogueList.Count}条对话数据");
    }

    /// <summary>
    /// 加载指定的CSV文件数据
    /// 供外部调用切换不同对话文件
    /// </summary>
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

    /// <summary>
    /// 切换到主线对话
    /// 在游戏开始时调用
    /// </summary>
    public void SwitchToMainDialogue()
    {
        LoadDialogueData(mainDialogueCSV);
    }

    /// <summary>
    /// 切换到探索问询对话
    /// 在探索阶段调用
    /// </summary>
    public void SwitchToInquiryDialogue()
    {
        LoadDialogueData(inquiryDialogueCSV);
    }

    /// <summary>
    /// 切换到结局对话
    /// 在游戏结局时调用
    /// </summary>
    public void SwitchToEndingDialogue()
    {
        LoadDialogueData(endingDialogueCSV);
    }


    // 显示当前对话
    public void ShowCurrentDialogue()
    {
        if (!HasValidUI()) return;

        // 对话结束判断 - 检查索引越界或Next为"Ending"
        if (currentIndex >= dialogueList.Count || dialogueList[currentIndex].Next == "Ending")
        { 
            Debug.Log("对话流程结束或到达结局！");
            HideAllPanels();
            HideOptions();

            // 调用场景加载器的结局处理方法
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.HandleEnding();
            }
            else
            {
                Debug.LogError("SceneLoader实例未找到，无法处理结局");
                // 备用方案：直接返回主菜单
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            return;
        }

        DialogueData currentData = dialogueList[currentIndex];
        string currentUiType = currentData.UI;

        // 重置状态：隐藏所有面板、选项，停止逐字协程
        HideAllPanels();
        HideOptions();
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;

        // 根据UI类型（U001-U004）切换面板
        switch (currentUiType)
        {
            case "U001": // 旁白：仅主面板，无角色立绘/名称
                ShowMainPanel(showPortrait: false, showName: false);
                ShowDialogueText(currentSceneUI.Text_MainDialogue, currentData.Text);
                break;

            case "U002": // 重要角色：主面板+立绘+名称
                ShowMainPanel(showPortrait: true, showName: true);
                currentSceneUI.Text_MainName.text = currentData.Character_Name;
                LoadSpriteToImage(currentSceneUI.Image_MainPortrait, portraitResPath + currentData.Character);
                ShowDialogueText(currentSceneUI.Text_MainDialogue, currentData.Text);
                break;

            case "U003": // 次要角色：次要面板+头像+名称
                ShowMinorPanel(showAvatar: true, showName: true);
                currentSceneUI.Text_MinorName.text = currentData.Character_Name;
                LoadSpriteToImage(currentSceneUI.Image_MinorAvatar, avatarResPath + currentData.Character);
                ShowDialogueText(currentSceneUI.Text_MinorDialogue, currentData.Text);
                break;

            case "U004": // 带选项：当前角色面板+选项
                if (IsImportantCharacter(currentData.Character))
                {
                    ShowMainPanel(showPortrait: true, showName: true);
                    currentSceneUI.Text_MainName.text = currentData.Character_Name;
                    LoadSpriteToImage(currentSceneUI.Image_MainPortrait, portraitResPath + currentData.Character);
                    ShowDialogueText(currentSceneUI.Text_MainDialogue, currentData.Text);
                }
                else
                {
                    ShowMinorPanel(showAvatar: true, showName: true);
                    currentSceneUI.Text_MinorName.text = currentData.Character_Name;
                    LoadSpriteToImage(currentSceneUI.Image_MinorAvatar, avatarResPath + currentData.Character);
                    ShowDialogueText(currentSceneUI.Text_MinorDialogue, currentData.Text);
                }
                ShowOptions(currentData.Choice);
                break;

            case "U005"://大字报触发节点
                //隐藏对话面板
                HideAllPanels();
                HideOptions();
                //触发大字报显示事件
                TriggerBigPosterEvent();
                break;

            default:
                Debug.LogError($"未知UI类型「{currentUiType}」，请检查CSV的UI字段！");
                break;
        }

        // 切换背景
        ChangeBackground(currentData.Scene);
    }


    // UI控制方法
    #region UI控制
    // 显示主面板（控制立绘、名称显隐）
    private void ShowMainPanel(bool showPortrait, bool showName)
    {
        if (!HasValidUI()) return;

        if (currentSceneUI.DialoguePanel_Main != null)
        {
            currentSceneUI.DialoguePanel_Main.SetActive(true);
            currentSceneUI.Image_MainPortrait?.gameObject.SetActive(showPortrait);
            currentSceneUI.Panel_MainName?.SetActive(showName);
            currentSceneUI.Text_MainName?.gameObject.SetActive(showName);
        }
        else
        {
            Debug.LogError("主对话面板（DialoguePanel_Main）未赋值！");
        }
    }

    // 显示次要面板（控制头像、名称显隐）
    private void ShowMinorPanel(bool showAvatar, bool showName)
    {
        if (!HasValidUI()) return;

        if (currentSceneUI.DialoguePanel_Minor != null)
        {
            currentSceneUI.DialoguePanel_Minor.SetActive(true);
            currentSceneUI.Image_MinorAvatar?.gameObject.SetActive(showAvatar);
            currentSceneUI.Panel_MinorName?.SetActive(showName);
            currentSceneUI.Text_MinorName?.gameObject.SetActive(showName);
        }
        else
        {
            Debug.LogError("次要对话面板（DialoguePanel_Minor）未赋值！");
        }
    }

    // 隐藏所有对话面板
    private void HideAllPanels()
    {
        if (!HasValidUI()) return;

        currentSceneUI.DialoguePanel_Main?.SetActive(false);
        currentSceneUI.DialoguePanel_Minor?.SetActive(false);
    }

    // 显示选项
    private void ShowOptions(string choiceStr)
    {
        if (!HasValidUI()) return;
        if (string.IsNullOrEmpty(choiceStr) || currentSceneUI.Panel_Options == null)
        {
            HideOptions();
            return;
        }

        currentSceneUI.Panel_Options.SetActive(true);
        string[] optionParts = choiceStr.Split(';');

        // 处理选项1
        if (optionParts.Length >= 1 && currentSceneUI.Button_Option1 != null && currentSceneUI.Text_Option1 != null)
        {
            string[] opt1 = optionParts[0].Split(':');
            if (opt1.Length == 2)
            {
                currentSceneUI.Text_Option1.text = opt1[0];
                string targetProcess = opt1[1];
                OptionButtonInteract interact1 = currentSceneUI.Button_Option1.GetComponent<OptionButtonInteract>();
                if (interact1 != null)
                {
                    interact1.SetTargetProcess(targetProcess);
                }
                currentSceneUI.Button_Option1.onClick.RemoveAllListeners();
                currentSceneUI.Button_Option1.gameObject.SetActive(true);
            }
            else
            {
                currentSceneUI.Button_Option1.gameObject.SetActive(false);
                Debug.LogWarning($"选项1格式错误：{optionParts[0]}（需「文本:进程」格式）");
            }
        }

        // 处理选项2
        if (optionParts.Length >= 2 && currentSceneUI.Button_Option2 != null && currentSceneUI.Text_Option2 != null)
        {
            string[] opt2 = optionParts[1].Split(':');
            if (opt2.Length == 2)
            {
                currentSceneUI.Text_Option2.text = opt2[0];
                string targetProcess = opt2[1];
                OptionButtonInteract interact2 = currentSceneUI.Button_Option2.GetComponent<OptionButtonInteract>();
                if (interact2 != null)
                {
                    interact2.SetTargetProcess(targetProcess);
                }
                currentSceneUI.Button_Option2.onClick.RemoveAllListeners();
                currentSceneUI.Button_Option2.gameObject.SetActive(true);
            }
            else
            {
                currentSceneUI.Button_Option2.gameObject.SetActive(false);
                Debug.LogWarning($"选项2格式错误：{optionParts[1]}（需「文本:进程」格式）");
            }
        }
    }

    // 隐藏选项
    private void HideOptions()
    {
        if (!HasValidUI()) return;
        currentSceneUI.Panel_Options?.SetActive(false);
        currentSceneUI.Button_Option1?.onClick.RemoveAllListeners();
        currentSceneUI.Button_Option2?.onClick.RemoveAllListeners();
    }
    #endregion

    // 选项按钮点击：跳转到目标进程
    public void OnOptionClicked(string targetProcess)
    {
        if (string.IsNullOrEmpty(targetProcess))
        {
            Debug.LogError("选项目标进程为空！");
            return;
        }

        HideOptions();

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

    // 文本控制：逐字显示
    #region 文本控制
    // 启动逐字显示
    private void ShowDialogueText(TMP_Text targetText, string textContent)
    {
        if (targetText == null)
        {
            Debug.LogError("目标文本组件（TMP_Text）未赋值！");
            return;
        }

        targetText.text = "";
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypingCoroutine(targetText, textContent));
    }

    // 逐字显示协程
    private IEnumerator TypingCoroutine(TMP_Text targetText, string textContent)
    {
        isTyping = true;
        OnTypingStateChanged?.Invoke(true); //开始逐字
        targetText.richText = true;
        int currentPos = 0;

        while (currentPos < textContent.Length)
        {
            if (currentPos + 3 < textContent.Length && textContent.Substring(currentPos, 4) == "<br>")
            {
                targetText.text += "<br>";
                currentPos += 4;
            }
            else
            {
                targetText.text += textContent[currentPos];
                currentPos += 1;
            }
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        OnTypingStateChanged?.Invoke(false);  //结束逐字
    }

    // 跳过逐字显示（直接显示完整文本）
    public void SkipTyping(DialogueData currentData)
    {
        if (!isTyping || typingCoroutine == null) return;

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
        isTyping = false;
        OnTypingStateChanged?.Invoke(false);

        if (currentSceneUI.DialoguePanel_Main.activeSelf)
            currentSceneUI.Text_MainDialogue.text = currentData.Text;
        else if (currentSceneUI.DialoguePanel_Minor.activeSelf)
            currentSceneUI.Text_MinorDialogue.text = currentData.Text;
    }
    #endregion

    // 判断是否为重要角色
    private bool IsImportantCharacter(string charId)
    {
        List<string> importantChars = new List<string> { "C001", "C002", "C003", "C004" };
        return importantChars.Contains(charId);
    }

    // 加载Sprite到Image
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
    /// 切换背景 - 只对S005和S001显示背景，其他情况清空
    /// </summary>
    private void ChangeBackground(string sceneId)
    {
        if (!HasValidUI()) return;

        // 只处理S005和S001，其他情况清空背景
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
                Debug.LogWarning($"未找到背景Sprite：{backgroundPath}，请检查Resources路径和文件名！");
                currentSceneUI.SetBackground(null); // 清空背景
            }
        }
        else
        {
            // 非S005和S001的场景，清空背景
            currentSceneUI.SetBackground(null);
        }
    }

    /// <summary>
    /// 触发大字报显示事件
    /// U005自动调用
    /// </summary>
    private void TriggerBigPosterEvent()
    {
        Debug.Log("触发大字报显示");
        if (DialogueEventSystem.Instance != null)
        {
            // 触发大字报显示事件，通知其他系统显示大字报
            DialogueEventSystem.Instance.OnShowBigPoster?.Invoke();
        }
        else
        {
            Debug.LogError("DialogueEventSystem实例未找到，无法触发大字报事件！");
            // 自动继续到下一句对话
            ContinueAfterBigPoster();
        }
            
    }

    /// <summary>
    /// 大字报关闭后继续对话流程
    /// 这个方法需要由大字报系统在关闭时调用
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
            HideAllPanels();
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
}
