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
    // UI�������
    [Header("������塿��Ҫ��ɫ/�԰׹���")]
    public GameObject DialoguePanel_Main; // ���Ի����
    public Image Image_MainPortrait;  // ��Ҫ��ɫ����
    public GameObject Panel_MainName;  // ��Ҫ��ɫ�������
    public TMP_Text Text_MainName;   // ��Ҫ��ɫ�����ı�
    public TMP_Text Text_MainDialogue;  // ���Ի��ı�

    [Header("����Ҫ��塿��Ҫ��ɫר��")]
    public GameObject DialoguePanel_Minor; // ��Ҫ�Ի����
    public Image Image_MinorAvatar;  // ��Ҫ��ɫͷ��
    public GameObject Panel_MinorName;   // ��Ҫ��ɫ�������
    public TMP_Text Text_MinorName;  // ��Ҫ��ɫ�����ı�
    public TMP_Text Text_MinorDialogue; // ��Ҫ�Ի��ı�

    [Header("��ѡ����塿��ѡ��ʱ��ʾ")]
    public GameObject Panel_Options;
    public Button Button_Option1;
    public TMP_Text Text_Option1;
    public Button Button_Option2;
    public TMP_Text Text_Option2;

    [Header("����������ʾ��������ͼ")]
    public GameObject BackgroundGameObject; // ������ʾ��������Ϸ����

    [Header("�������")]
    public TextAsset dialogueCsv;   // �Ի�CSV�ļ�
    public float typeSpeed = 0.05f;  // ������ʾ�ٶ�
    public string portraitResPath = "Portraits/"; // ��Ҫ��ɫ������Դ·��
    public string avatarResPath = "Avatars/";   // ��Ҫ��ɫͷ����Դ·��
    public string backgroundResPath = "Background/"; // ������Դ·��

    public List<DialogueData> dialogueList = new List<DialogueData>(); // �Ի������б�
    public int currentIndex = 0;  // ��ǰ�Ի�����
    public Coroutine typingCoroutine;  // ������ʾЭ��
    public bool isTyping = false;   // �Ƿ�����������ʾ


    // ����CSV + ��ʾ�����Ի�
    void Start()
    {
        // ��ʼ������������ѡ��
        HideAllPanels();
        HideOptions();

        // ����CSV����
        if (dialogueCsv != null)
        {
            LoadCsvData();
            if (dialogueList.Count > 0)
            {
                ShowCurrentDialogue(); // ��ʾ��һ���Ի�
            }
            else
            {
                Debug.LogError("CSV����Ϊ�գ������ļ����ݣ�");
            }
        }
        else
        {
            Debug.LogError("δ��ֵ�Ի�CSV�ļ�������Inspector�����룡");
        }
    }


    // ����CSV����
    private void LoadCsvData()
    {
        dialogueList.Clear();
        StringReader reader = new StringReader(dialogueCsv.text);

        // ������ͷ
        string headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
        {
            Debug.LogError("CSV��ͷΪ�գ���ʽ����");
            return;
        }

        // ���ж�ȡ�Ի�����
        while (reader.Peek() != -1)
        {
            string dataLine = reader.ReadLine().Trim();
            if (string.IsNullOrEmpty(dataLine) || dataLine.StartsWith("//"))
                continue; // �������к�ע����

            string[] dataParts = dataLine.Split(',');
            if (dataParts.Length != 9)
            {
                Debug.LogWarning($"CSV�и�ʽ���������ݡ�{dataLine}�����ֶ���{dataParts.Length}����9����");
                continue;
            }

            // �������洢�Ի�����
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
        Debug.Log($"CSV���سɹ�����{dialogueList.Count}���Ի�����");
    }


    // ��ʾ��ǰ�Ի�
    public void ShowCurrentDialogue()
    {
        // �Ի������ж�
        if (currentIndex >= dialogueList.Count)
        {
            Debug.Log("�Ի����̽�����");
            HideAllPanels();
            HideOptions();
            return;
        }

        DialogueData currentData = dialogueList[currentIndex];
        string currentUiType = currentData.UI;

        // ����״̬������������塢ѡ�ֹͣ����Э��
        HideAllPanels();
        HideOptions();
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;

        // ����UI���ͣ�U001-U004���л����
        switch (currentUiType)
        {
            case "U001": // �԰ף�������壬�޽�ɫ����/����
                ShowMainPanel(showPortrait: false, showName: false);
                ShowDialogueText(Text_MainDialogue, currentData.Text);
                break;

            case "U002": // ��Ҫ��ɫ�������+����+����
                ShowMainPanel(showPortrait: true, showName: true);
                Text_MainName.text = currentData.Character_Name;
                LoadSpriteToImage(Image_MainPortrait, portraitResPath + currentData.Character);
                ShowDialogueText(Text_MainDialogue, currentData.Text);
                break;

            case "U003": // ��Ҫ��ɫ����Ҫ���+ͷ��+����
                ShowMinorPanel(showAvatar: true, showName: true);
                Text_MinorName.text = currentData.Character_Name;
                LoadSpriteToImage(Image_MinorAvatar, avatarResPath + currentData.Character);
                ShowDialogueText(Text_MinorDialogue, currentData.Text);
                break;

            case "U004": // ��ѡ���ǰ��ɫ���+ѡ��
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
                Debug.LogError($"δ֪UI���͡�{currentUiType}��������CSV��UI�ֶΣ�");
                break;
        }

        // �л�����
        ChangeBackground(currentData.Scene);
    }


    // UI���Ʒ���
    #region UI����
    // ��ʾ����壨�������桢����������
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
            Debug.LogError("���Ի���壨DialoguePanel_Main��δ��ֵ��");
        }
    }

    // ��ʾ��Ҫ��壨����ͷ������������
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
            Debug.LogError("��Ҫ�Ի���壨DialoguePanel_Minor��δ��ֵ��");
        }
    }

    // �������жԻ����
    private void HideAllPanels()
    {
        DialoguePanel_Main?.SetActive(false);
        DialoguePanel_Minor?.SetActive(false);
    }

    // ��ʾѡ��
    private void ShowOptions(string choiceStr)
    {
        if (string.IsNullOrEmpty(choiceStr) || Panel_Options == null)
        {
            HideOptions();
            return;
        }

        Panel_Options.SetActive(true);
        string[] optionParts = choiceStr.Split(';');

        // ����ѡ��1
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
                Debug.LogWarning($"ѡ��1��ʽ����{optionParts[0]}���衸�ı�:���̡���ʽ��");
            }
        }

        // ����ѡ��2
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
                Debug.LogWarning($"ѡ��2��ʽ����{optionParts[1]}���衸�ı�:���̡���ʽ��");
            }
        }
    }

    // ����ѡ��
    private void HideOptions()
    {
        Panel_Options?.SetActive(false);
        Button_Option1?.onClick.RemoveAllListeners();
        Button_Option2?.onClick.RemoveAllListeners();
    }
    #endregion

    // ѡ�ť�������ת��Ŀ�����
    public void OnOptionClicked(string targetProcess)
    {
        if (string.IsNullOrEmpty(targetProcess))
        {
            Debug.LogError("ѡ��Ŀ�����Ϊ�գ�");
            return;
        }

        HideOptions();

        // ����Ŀ����̵�����
        for (int i = 0; i < dialogueList.Count; i++)
        {
            if (dialogueList[i].Process == targetProcess)
            {
                currentIndex = i;
                ShowCurrentDialogue();
                return;
            }
        }

        Debug.LogError($"δ�ҵ�Ŀ����̡�{targetProcess}��������CSV��Choice�ֶΣ�");
        currentIndex++;
        ShowCurrentDialogue();
    }

    // �ı����ƣ�������ʾ
    #region �ı�����
    // ����������ʾ
    private void ShowDialogueText(TMP_Text targetText, string textContent)
    {
        if (targetText == null)
        {
            Debug.LogError("Ŀ���ı������TMP_Text��δ��ֵ��");
            return;
        }

        targetText.text = "";
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypingCoroutine(targetText, textContent));
    }

    //public void UpdateTypingState(bool typing)
    //{
    //    isTyping = typing;
    //    // �ҵ��Ի������Ľ����ű�������״̬
    //    DialoguePanelInteract mainInteract = DialoguePanel_Main.GetComponent<DialoguePanelInteract>();
    //    DialoguePanelInteract minorInteract = DialoguePanel_Minor.GetComponent<DialoguePanelInteract>();
    //    mainInteract?.SetTypingState(typing);
    //    minorInteract?.SetTypingState(typing);
    //}

    public event Action<bool> OnTypingStateChanged;

    // ������ʾЭ��
    private IEnumerator TypingCoroutine(TMP_Text targetText, string textContent)
    {
        isTyping = true;
        OnTypingStateChanged?.Invoke(true); //��ʼ����
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
        OnTypingStateChanged?.Invoke(false);  //��������
    }

    // ����������ʾ��ֱ����ʾ�����ı���
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

    // �ж��Ƿ�Ϊ��Ҫ��ɫ
    private bool IsImportantCharacter(string charId)
    {
        List<string> importantChars = new List<string> { "C001", "C002", "C003", "C004" };
        return importantChars.Contains(charId);
    }

    // ����Sprite��Image
    private void LoadSpriteToImage(Image targetImage, string spritePath)
    {
        if (targetImage == null)
        {
            Debug.LogError("Ŀ��Image���δ��ֵ��");
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
            Debug.LogWarning($"δ�ҵ�Sprite��{spritePath}������Resources·�����ļ�����");
            targetImage.gameObject.SetActive(false);
        }
    }

    // �л�����
    private void ChangeBackground(string sceneId)
    {
        if (BackgroundGameObject == null)
        {
            Debug.LogError("������Ϸ����BackgroundGameObject��δ��ֵ��");
            return;
        }

        SpriteRenderer spriteRenderer = BackgroundGameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("������Ϸ������û��SpriteRenderer�����");
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
            Debug.LogWarning($"δ�ҵ�����Sprite��{backgroundPath}������Resources·�����ļ�����");
            BackgroundGameObject.SetActive(false);
        }
    }
}