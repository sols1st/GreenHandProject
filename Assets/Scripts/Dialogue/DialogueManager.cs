using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;

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
    // UI组件引用
    [Header("【主面板】重要角色/旁白共用")]
    public GameObject DialoguePanel_Main; // 主对话面板
    public Image Image_MainPortrait;  // 重要角色立绘
    public GameObject Panel_MainName;  // 重要角色名称面板
    public TMP_Text Text_MainName;   // 重要角色名称文本
    public TMP_Text Text_MainDialogue;  // 主对话文本

    [Header("【次要面板】次要角色专用")]
    public GameObject DialoguePanel_Minor; // 次要对话面板
    public Image Image_MinorAvatar;  // 次要角色头像
    public GameObject Panel_MinorName;   // 次要角色名称面板
    public TMP_Text Text_MinorName;  // 次要角色名称文本
    public TMP_Text Text_MinorDialogue; // 次要对话文本

    [Header("【选项面板】带选项时显示")]
    public GameObject Panel_Options;
    public Button Button_Option1;
    public TMP_Text Text_Option1;
    public Button Button_Option2;
    public TMP_Text Text_Option2;

    [Header("【背景】显示背景精灵图")]
    public GameObject BackgroundGameObject; // 用于显示背景的游戏对象

    [Header("【配置项】")]
    public TextAsset dialogueCsv;   // 对话CSV文件
    public float typeSpeed = 0.05f;  // 逐字显示速度
    public string portraitResPath = "Portraits/"; // 重要角色立绘资源路径
    public string avatarResPath = "Avatars/";   // 次要角色头像资源路径
    public string backgroundResPath = "Background/"; // 背景资源路径

    public List<DialogueData> dialogueList = new List<DialogueData>(); // 对话数据列表
    public int currentIndex = 0;  // 当前对话索引
    public Coroutine typingCoroutine;  // 逐字显示协程
    public bool isTyping = false;   // 是否正在逐字显示


    // 加载CSV + 显示首条对话
    void Start()
    {
        // 初始隐藏所有面板和选项
        HideAllPanels();
        HideOptions();

        // 加载CSV数据
        if (dialogueCsv != null)
        {
            LoadCsvData();
            if (dialogueList.Count > 0)
            {
                ShowCurrentDialogue(); // 显示第一条对话
            }
            else
            {
                Debug.LogError("CSV数据为空，请检查文件内容！");
            }
        }
        else
        {
            Debug.LogError("未赋值对话CSV文件，请在Inspector中拖入！");
        }
    }


    // 加载CSV数据
    private void LoadCsvData()
    {
        dialogueList.Clear();
        StringReader reader = new StringReader(dialogueCsv.text);

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


    // 显示当前对话
    public void ShowCurrentDialogue()
    {
        // 对话结束判断
        if (currentIndex >= dialogueList.Count)
        {
            Debug.Log("对话流程结束！");
            HideAllPanels();
            HideOptions();
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
                ShowDialogueText(Text_MainDialogue, currentData.Text);
                break;

            case "U002": // 重要角色：主面板+立绘+名称
                ShowMainPanel(showPortrait: true, showName: true);
                Text_MainName.text = currentData.Character_Name;
                LoadSpriteToImage(Image_MainPortrait, portraitResPath + currentData.Character);
                ShowDialogueText(Text_MainDialogue, currentData.Text);
                break;

            case "U003": // 次要角色：次要面板+头像+名称
                ShowMinorPanel(showAvatar: true, showName: true);
                Text_MinorName.text = currentData.Character_Name;
                LoadSpriteToImage(Image_MinorAvatar, avatarResPath + currentData.Character);
                ShowDialogueText(Text_MinorDialogue, currentData.Text);
                break;

            case "U004": // 带选项：当前角色面板+选项
                if (IsImportantCharacter(currentData.Character))
                {
                    ShowMainPanel(showPortrait: true, showName: true);
                    Text_MainName.text = currentData.Character_Name;
                    LoadSpriteToImage(Image_MainPortrait, portraitResPath + currentData.Character);
                    ShowDialogueText(Text_MainDialogue, currentData.Text);
                }
                else
                {
                    ShowMinorPanel(showAvatar: true, showName: true);
                    Text_MinorName.text = currentData.Character_Name;
                    LoadSpriteToImage(Image_MinorAvatar, avatarResPath + currentData.Character);
                    ShowDialogueText(Text_MinorDialogue, currentData.Text);
                }
                ShowOptions(currentData.Choice);
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
        if (DialoguePanel_Main != null)
        {
            DialoguePanel_Main.SetActive(true);
            Image_MainPortrait?.gameObject.SetActive(showPortrait);
            Panel_MainName?.SetActive(showName);
            Text_MainName?.gameObject.SetActive(showName);
        }
        else
        {
            Debug.LogError("主对话面板（DialoguePanel_Main）未赋值！");
        }
    }

    // 显示次要面板（控制头像、名称显隐）
    private void ShowMinorPanel(bool showAvatar, bool showName)
    {
        if (DialoguePanel_Minor != null)
        {
            DialoguePanel_Minor.SetActive(true);
            Image_MinorAvatar?.gameObject.SetActive(showAvatar);
            Panel_MinorName?.SetActive(showName);
            Text_MinorName?.gameObject.SetActive(showName);
        }
        else
        {
            Debug.LogError("次要对话面板（DialoguePanel_Minor）未赋值！");
        }
    }

    // 隐藏所有对话面板
    private void HideAllPanels()
    {
        DialoguePanel_Main?.SetActive(false);
        DialoguePanel_Minor?.SetActive(false);
    }

    // 显示选项
    private void ShowOptions(string choiceStr)
    {
        if (string.IsNullOrEmpty(choiceStr) || Panel_Options == null)
        {
            HideOptions();
            return;
        }

        Panel_Options.SetActive(true);
        string[] optionParts = choiceStr.Split(';');

        // 处理选项1
        if (optionParts.Length >= 1 && Button_Option1 != null && Text_Option1 != null)
        {
            string[] opt1 = optionParts[0].Split(':');
            if (opt1.Length == 2)
            {
                Text_Option1.text = opt1[0];
                string targetProcess = opt1[1];
                OptionButtonInteract interact1 = Button_Option1.GetComponent<OptionButtonInteract>();
                if (interact1 != null)
                {
                    interact1.SetTargetProcess(targetProcess);
                }
                Button_Option1.onClick.RemoveAllListeners();
                Button_Option1.gameObject.SetActive(true);
            }
            else
            {
                Button_Option1.gameObject.SetActive(false);
                Debug.LogWarning($"选项1格式错误：{optionParts[0]}（需「文本:进程」格式）");
            }
        }

        // 处理选项2
        if (optionParts.Length >= 2 && Button_Option2 != null && Text_Option2 != null)
        {
            string[] opt2 = optionParts[1].Split(':');
            if (opt2.Length == 2)
            {
                Text_Option2.text = opt2[0];
                string targetProcess = opt2[1];
                OptionButtonInteract interact2 = Button_Option2.GetComponent<OptionButtonInteract>();
                if (interact2 != null)
                {
                    interact2.SetTargetProcess(targetProcess);
                }
                Button_Option2.onClick.RemoveAllListeners();
                Button_Option2.gameObject.SetActive(true);
            }
            else
            {
                Button_Option2.gameObject.SetActive(false);
                Debug.LogWarning($"选项2格式错误：{optionParts[1]}（需「文本:进程」格式）");
            }
        }
    }

    // 隐藏选项
    private void HideOptions()
    {
        Panel_Options?.SetActive(false);
        Button_Option1?.onClick.RemoveAllListeners();
        Button_Option2?.onClick.RemoveAllListeners();
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

    //public void UpdateTypingState(bool typing)
    //{
    //    isTyping = typing;
    //    // 找到对话框面板的交互脚本，更新状态
    //    DialoguePanelInteract mainInteract = DialoguePanel_Main.GetComponent<DialoguePanelInteract>();
    //    DialoguePanelInteract minorInteract = DialoguePanel_Minor.GetComponent<DialoguePanelInteract>();
    //    mainInteract?.SetTypingState(typing);
    //    minorInteract?.SetTypingState(typing);
    //}

    public event Action<bool> OnTypingStateChanged;

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

        if (DialoguePanel_Main.activeSelf)
            Text_MainDialogue.text = currentData.Text;
        else if (DialoguePanel_Minor.activeSelf)
            Text_MinorDialogue.text = currentData.Text;
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

    // 切换背景
    private void ChangeBackground(string sceneId)
    {
        if (BackgroundGameObject == null)
        {
            Debug.LogError("背景游戏对象（BackgroundGameObject）未赋值！");
            return;
        }

        SpriteRenderer spriteRenderer = BackgroundGameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("背景游戏对象上没有SpriteRenderer组件！");
            return;
        }

        string backgroundPath = backgroundResPath + sceneId;
        Sprite backgroundSprite = Resources.Load<Sprite>(backgroundPath);
        if (backgroundSprite != null)
        {
            spriteRenderer.sprite = backgroundSprite;
            BackgroundGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"未找到背景Sprite：{backgroundPath}，请检查Resources路径和文件名！");
            BackgroundGameObject.SetActive(false);
        }
    }
}