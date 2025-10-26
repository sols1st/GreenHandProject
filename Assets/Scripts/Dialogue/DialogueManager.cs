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

    private OptionData currentOption;//当前选项数据
    private List<OptionData> activeOptions = new List<OptionData>();//激活的选项

    public event Action<string> OnCardGained;//获得卡牌事件

    private void Awake()
    {
        // 单例模式
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

    /// <summary>
    /// 注册当前场景的UI组件
    /// 由SceneDialogueUI在场景加载时调用
    /// </summary>
    public void RegisterSceneUI(SceneDialogueUI sceneUI)
    {
        currentSceneUI = sceneUI;
        // 如果有等待中的对话，立即显示
        if (dialogueList.Count > 0 && currentIndex < dialogueList.Count)
        {
            ShowCurrentDialogue();
        }
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

        // 逐行读取对话数据
        while (reader.Peek() != -1)
        {
            string dataLine = reader.ReadLine().Trim();
            if (string.IsNullOrEmpty(dataLine) || dataLine.StartsWith("//"))
                continue; // 跳过空行和注释行

            string[] dataParts = dataLine.Split(',');

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
        reader.Close();
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
    /// 在推理结局时调用
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
            case "U001": // 旁白
                ShowNarrationPanel();
                ShowDialogueText(currentSceneUI.Text_Narration, currentData.Text);
                break;

            case "U002": // 主要角色：主面板+立绘+名称
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
                HideAllPanels();
                HideOptions();
                //触发结局大字报显示
                if (CardManager.Instance != null)
                {
                    CardManager.Instance.ShowEnding();
                }
                else
                {
                    ContinueAfterBigPoster();
                }
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
    //显示旁白
    private void ShowNarrationPanel()
    {
        if (!HasValidUI()) return;
        currentSceneUI.DialoguePanel_Narration.SetActive(true);
    }

    // 显示主面板（控制立绘、名称显隐）
    private void ShowMainPanel(bool showPortrait, bool showName)
    {
        if (!HasValidUI()) return;

        if (currentSceneUI.DialoguePanel_Main != null)
        {
            currentSceneUI.DialoguePanel_Main.SetActive(true);
            currentSceneUI.Image_MainPortrait?.gameObject.SetActive(true);
            currentSceneUI.Text_MainName?.gameObject.SetActive(true);
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
            currentSceneUI.Image_MinorAvatar?.gameObject.SetActive(true);
            currentSceneUI.Text_MinorName?.gameObject.SetActive(true);
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
        currentSceneUI.DialoguePanel_Narration?.SetActive(false);
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
        if (optionParts.Length >= 1 && currentSceneUI.Button_Option1 != null)
        {
            SetupOptionButton(currentSceneUI.Button_Option1.gameObject, optionParts[0], 0);
        }
        else
        {
            currentSceneUI.Button_Option1?.gameObject.SetActive(false);
        }
        // 处理选项2
        if (optionParts.Length >= 2 && currentSceneUI.Button_Option2 != null)
        {
            SetupOptionButton(currentSceneUI.Button_Option2.gameObject, optionParts[1], 1);
        }
        else
        {
            currentSceneUI.Button_Option2?.gameObject.SetActive(false);
        }
    }

    // 设置选项按钮
    private void SetupOptionButton(GameObject buttonObject, string optionStr, int optionIndex)
    {
        buttonObject.SetActive(true);
        
        string[] optData = optionStr.Split(':');

        if (optData.Length < 2)
        {
            Debug.LogError($"选项格式错误：{optionStr}");
            buttonObject.SetActive(false);
            return;
        }

        // 创建选项数据
        OptionData option = new OptionData
        {
            OptionText = optData[0],
            TargetProcess = optData[1],
            RequireCard = optData.Length > 2 ? optData[2] : "",
            GainedCard = optData.Length > 3 ? optData[3] : ""
        };
        // 设置按钮文本
        TMP_Text buttonText = optionIndex == 0 ? currentSceneUI.Text_Option1 : currentSceneUI.Text_Option2;
        if (buttonText != null)
            buttonText.text = option.OptionText;
        // 设置交互组件
        OptionButtonInteract interact = buttonObject.GetComponent<OptionButtonInteract>();
        if (interact == null)
            interact = buttonObject.AddComponent<OptionButtonInteract>();
        interact.SetOptionData(option);

    }

    // 处理选项（不需要卡牌）
    public void ProcessOption(OptionData option)
    {
        HideOptions();

        // 处理获得卡牌
        if (!string.IsNullOrEmpty(option.GainedCard))
        {
            GainCard(option.GainedCard);
        }

        // 跳转到目标进程
        OnOptionClicked(option.TargetProcess);
    }

    // 打开卡牌库选项（需要卡牌）
    public void OpenCardSelection(OptionData option)
    {
        HideOptions();
        currentOption = option;

        // 打开卡牌库选择界面
        //CardBagManager.Instance.Open("Dialogue");
    }

    // 处理卡牌选择结果
    public void OnCardSelected(string selectedCardId)
    {
        if (currentOption == null)return;

        // 检查卡牌是否正确
        if (selectedCardId == currentOption.RequireCard)
        {
            HideOptions();

            // 处理获得卡牌
            if (!string.IsNullOrEmpty(currentOption.GainedCard))
            {
                GainCard(currentOption.GainedCard);
            }

            // 跳转到目标进程
            OnOptionClicked(currentOption.TargetProcess);
        }
        else
        {
            // 卡牌错误，隐藏面板结束对话
            HideAllPanels();
            HideOptions();
        }

        currentOption = null; // 重置当前选项
    }

    // 获得卡牌处理
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
            Debug.LogError("CardManager实例未找到，无法获得卡牌");
        }
    }

    // 隐藏选项
    private void HideOptions()
    {
        if (!HasValidUI()) return;
        currentSceneUI.Panel_Options?.SetActive(false);
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

        if (currentSceneUI.DialoguePanel_Narration.activeSelf)
            currentSceneUI.Text_Narration.text = currentData.Text;
        else if (currentSceneUI.DialoguePanel_Main.activeSelf)
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
