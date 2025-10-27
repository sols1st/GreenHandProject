using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DeductionManager : MonoBehaviour
{
    public static DeductionManager Instance { get; private set; }
    public GameObject DeductionCanvas;
    public GameObject Deduction1;
    public GameObject Deduction2;
    public GameObject Deduction3;
    public GameObject Deduction4;
    public GameObject Deduction5;
    public GameObject Deduction6;
    public GameObject LogicCard1;
    public GameObject LogicCard2;
    public GameObject LogicCard3;
    public Tab[] tabs;
    public GameObject powerTab;
    public GameObject validateEndingButton;
    public TMP_Text endingText;
    public GameObject endingCanvas;
    public GameObject openCardBagButton;
    public GameObject noticeCanvas;
    public GameObject knife;
    public GameObject next;
    
    private GameObject _selector;

    private Dictionary<string, Card> _selectedCards = new Dictionary<string, Card>()
    {
        { "Deduction1", null },
        { "Deduction2", null },
        { "Deduction3", null },
        { "Deduction4", null },
        { "Deduction5", null },
        { "Deduction6", null }
    };
    private Dictionary<string, string> _endings = new ();
    private int _chapter = 1;
    private string[] _logic1Functions = new string[6]{"A1",  "A2", "B1", "B2", "C1", "C2"};
    private string[] _logic2Functions = new string[6]{"A3",  "A4", "B3", "B4", "C3", "C4"};
    private string[] _logic3Functions = new string[6]{"A5",  "A6", "B5", "B6", "C5", "C6"};
    private bool _isCutLine = false;
    private string[] _errorLogicCardNotice = 
    {
        "你就这么相信自己？",
        "真的存在绝对真实的东西吗？",
        "真相不是很无趣吗？"
    };
    private int _errorLogicCardNoticeIndex = 0;
    private List<string> _paragraphs = new List<string>();
    private int _paragraphIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        
        // 加载文件
        LoadEndingFromFile();
    }
    
    // 加载结局数据
    private void LoadEndingFromFile()
    {
        var path = Path.Combine(Application.dataPath, "Resources/Ending/Ending.csv");

        // 清空现有数据
        _endings.Clear();
        var allLines = File.ReadAllText(path);
        var lines = ParseCsvWithLineBreaks(allLines);

        // 跳过标题行，从第二行开始处理
        for (var i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.Count >= 2)
            {
                // 第1列和第2列组合作为key
                var key = line[0].Trim();
                // 第3列作为值
                var value = line[1].Trim();

                // 添加到字典中
                _endings[key] = value;
            }
        }
    }
    
    // 解析结局数据文件
    private List<List<string>> ParseCsvWithLineBreaks(string csvContent)
    {
        var lines = new List<List<string>>();
        var currentLine = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < csvContent.Length; i++)
        {
            char c = csvContent[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < csvContent.Length && csvContent[i + 1] == '"')
                {
                    // 处理双引号转义
                    currentField.Append('"');
                    i++; // 跳过下一个引号
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // 字段结束
                currentLine.Add(currentField.ToString());
                currentField.Clear();
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                // 行结束
                if (c == '\r' && i + 1 < csvContent.Length && csvContent[i + 1] == '\n')
                {
                    i++; // 处理 Windows 换行符 \r\n
                }

                currentLine.Add(currentField.ToString());
                currentField.Clear();

                if (currentLine.Count > 0)
                {
                    lines.Add(currentLine);
                    currentLine = new List<string>();
                }
            }
            else
            {
                currentField.Append(c);
            }
        }

        // 添加最后一个字段
        if (currentField.Length > 0 || currentLine.Count > 0)
        {
            currentLine.Add(currentField.ToString());
            if (currentLine.Count > 0)
            {
                lines.Add(currentLine);
            }
        }

        return lines;
    }
    
    // 更新章节信息
    public void UpdateChapter(int chapter)
    {
        _chapter = chapter;
    }
    
    // 进入推理画布
    public void ShowDeductionCanvas()
    {
        // todo 对话 
        // DialogueManager.Instance.StartReasoningDialogue();  
        AudioManager.Instance.PlayBackgroundMusic("B001");
        if (openCardBagButton.GetComponent<OpenCardBagButton>().IsOpen())
        {
            openCardBagButton.GetComponent<OpenCardBagButton>().TriggerOnClick();
        }
        openCardBagButton.SetActive(false);
        powerTab.SetActive(false);
        DeductionCanvas.SetActive(true);
        switch (_chapter)
        {
            // 第1章探索结束后进入
            case 1:
                SetChapter1();
                break;
            case 2:
                SetChapter2();
                break;
            case 3:
                SetChapter3();
                break;
            case 4:
                SetChapter4();
                break;
        }
        
    }
    
    // 退出推理画布
    public void CloseDeductionCanvas()
    {
        powerTab.SetActive(true);
        DisableAllInteractable();
        DeductionCanvas.SetActive(false);
        openCardBagButton.SetActive(true);
    }

    // 更新选中的卡槽
    public void UpdateSelector(GameObject selector)
    {
        _selector = selector;
        _selectedCards[_selector.name] = null;
        string[] functions = null;
        Debug.Log(selector.name);
        switch (selector.name)
        {
            case "Deduction1":
            case "Deduction2":
                functions = _logic1Functions;
                LogicCard1.GetComponent<LogicCard>().SetCard(null);
                break;
            case "Deduction3":
            case "Deduction4":
                functions = _logic2Functions;
                LogicCard2.GetComponent<LogicCard>().SetCard(null);
                break;
            case "Deduction5":
            case "Deduction6":
                functions = _logic3Functions;
                LogicCard3.GetComponent<LogicCard>().SetCard(null);
                break;
        }
        UpdateTabsFunctions(functions);
        CardBagManager.Instance.Open(name, true, functions);
    }

    public void UpdateTabsFunctions(string[] functions)
    {
        foreach (var tab in tabs)
        {
            tab.UpdateFunctions(functions);
        }
    }
    
    // 判断卡牌是否已经选中
    public bool IsSelected(string cardID)
    {
        foreach (var card in _selectedCards.Values)
        {
            if (card != null && cardID == card.id)
            {
                return true;
            }
        }
        return false;
    }

    // 使用卡牌
    public void UseCard(Card card)
    {
        _selectedCards[_selector.name] = card;
        _selector.GetComponent<SelectCardItem>().SetCard(card);
        Card logicCard = null;
        // 逻辑卡UI更新
        switch (_selector.name)
        {
            case "Deduction1" or "Deduction2":
                var card1 = _selectedCards["Deduction1"];
                var card2 = _selectedCards["Deduction2"];
                if (card1 != null && card2 != null)
                {
                    var functions = new List<string>();
                    functions.AddRange(card1.function);
                    functions.AddRange(card2.function);
                    if (functions.Contains("A1") && functions.Contains("A2"))
                    {
                        // AI
                        logicCard = CardManager.Instance.GetCardInfo("CA012");
                    }else if (functions.Contains("B1") && functions.Contains("B2"))
                    {
                        // BI
                        logicCard = CardManager.Instance.GetCardInfo("CA013");
                    }else if (functions.Contains("C1") && functions.Contains("C2"))
                    {
                        // CI
                        logicCard = CardManager.Instance.GetCardInfo("CA014");
                    }
                }
                // 第一章生成逻辑卡牌后
                if (_chapter == 1 && logicCard != null)
                {
                    next.GetComponent<NextChapter>().UpdateChapter(_chapter);
                    next.SetActive(true);
                    Deduction1.GetComponent<SelectCardItem>().isInteractable = false;
                    Deduction2.GetComponent<SelectCardItem>().isInteractable = false;
                }
                LogicCard1.GetComponent<LogicCard>().SetCard(logicCard);
                break;
            case "Deduction3" or "Deduction4":
                var card3 = _selectedCards["Deduction3"];
                var card4 = _selectedCards["Deduction4"];
                
                if (card3 != null && card4 != null)
                {
                    var functions = new List<string>();
                    functions.AddRange(card3.function);
                    functions.AddRange(card4.function);
                    
                    if (functions.Contains("A3") && functions.Contains("A4"))
                    {
                        // AII
                        logicCard = CardManager.Instance.GetCardInfo("CA022");
                    }else if (functions.Contains("B3") && functions.Contains("B4"))
                    {
                        // BII
                        logicCard = CardManager.Instance.GetCardInfo("CA023");
                    }else if (functions.Contains("C1") && functions.Contains("C2"))
                    {
                        // CII
                        logicCard = CardManager.Instance.GetCardInfo("CA024");
                    }
                }
                // 第二章生成逻辑卡牌后
                if (_chapter == 2 && logicCard != null)
                {
                    next.GetComponent<NextChapter>().UpdateChapter(_chapter);
                    next.SetActive(true);
                    Deduction3.GetComponent<SelectCardItem>().isInteractable = false;
                    Deduction4.GetComponent<SelectCardItem>().isInteractable = false;
                }
                LogicCard2.GetComponent<LogicCard>().SetCard(logicCard);
                break;
            case "Deduction5" or "Deduction6":
                var card5 = _selectedCards["Deduction5"];
                var card6 = _selectedCards["Deduction6"];
                if (card5 != null && card6 != null)
                {
                    var functions = new List<string>();
                    functions.AddRange(card5.function);
                    functions.AddRange(card6.function);
                    if (functions.Contains("A5") && functions.Contains("A6"))
                    {
                        // AIII
                        logicCard = CardManager.Instance.GetCardInfo("CA036");
                    }else if (functions.Contains("B5") && functions.Contains("B6"))
                    {
                        // BIII
                        logicCard = CardManager.Instance.GetCardInfo("CA037");
                    }else if (functions.Contains("C5") && functions.Contains("C6"))
                    {
                        // CIII
                        logicCard = CardManager.Instance.GetCardInfo("CA038");
                    }
                }
                if (_chapter == 3 && logicCard != null)
                {
                    next.GetComponent<NextChapter>().UpdateChapter(_chapter);
                    next.SetActive(true);
                    Deduction5.GetComponent<SelectCardItem>().isInteractable = false;
                    Deduction6.GetComponent<SelectCardItem>().isInteractable = false;
                }
                LogicCard3.GetComponent<LogicCard>().SetCard(logicCard);
                break;
        }
        // 合成逻辑卡音效
        if (logicCard != null)
        {
            AudioManager.Instance.PlaySoundEffect("V003");
        }
        _selector = null;
        UpdateTabsFunctions(null);
    }
    
    // chapter1 对应UI初始化
    private void SetChapter1()
    {
        DisableAllInteractable();
        // 可交互
        Deduction1.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction2.GetComponent<SelectCardItem>().isInteractable = true;
        LogicCard1.GetComponent<LogicCard>().isInteractable = true;
    }
    
    // chapter2 对应UI初始化
    private void SetChapter2()
    {
        DisableAllInteractable();
        // 可交互
        Deduction3.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction4.GetComponent<SelectCardItem>().isInteractable = true;
        LogicCard2.GetComponent<LogicCard>().isInteractable = true;
    }
    
    // chapter3 对应UI初始化
    private void SetChapter3()
    {
        DisableAllInteractable();
        // 可交互
        Deduction5.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction6.GetComponent<SelectCardItem>().isInteractable = true;
        LogicCard3.GetComponent<LogicCard>().isInteractable = true;
    }
    
    // chapter4 对应UI初始化
    private void SetChapter4()
    {
        EnableAllInteractable();
    }
    
    // 剪线
    public void CutLine()
    {
        _isCutLine = true;
        knife.GetComponent<Knife>().CutLine();
    }
    
    
    // 验证是否能够进入结局（验证按钮调用）
    public void ValidateEnding()
    {
        var card1 = LogicCard1.GetComponent<LogicCard>().GetCardID();
        var card2 = LogicCard2.GetComponent<LogicCard>().GetCardID();
        var card3 = LogicCard3.GetComponent<LogicCard>().GetCardID();
        if (card1 == "" || card2 == "" || card3 == "")
        {
            // todo 提示文案更改
            ShowNotice(new []{"推理尚未结束"});
            return;
        }
        if (card1 == "CA012" && card2 == "CA022" && card3 == "CA036")
        {
            // todo 结局A
            if (!_isCutLine)
            {
                // todo 文案待更改
                ShowNotice(new []{"notice 1", "notice 2", "notice 3"});
                knife.SetActive(true);
                return;
            }
            SplitTextIntoParagraphs(_endings["A"]);
        }else if (card1 == "CA013" && card2 == "CA023" && card3 == "CA037")
        {
            // todo 结局B
            if (!_isCutLine)
            {
                // todo 文案待更改
                ShowNotice(new []{"notice 1", "notice 2", "notice 3"});
                knife.SetActive(true);
                return;
            }
            SplitTextIntoParagraphs(_endings["B"]);
        }else if (card1 == "CA014" && card2 == "CA024" && card3 == "CA038")
        {
            // todo 结局C
            if (!_isCutLine)
            {
                // todo 文案待更改
                ShowNotice(new []{"notice 1", "notice 2", "notice 3"});
                knife.SetActive(true);
                return;
            }
            SplitTextIntoParagraphs(_endings["C"]);
        }
        else
        {
            // 排列不正确提示
            ShowNotice(new []{_errorLogicCardNotice[_errorLogicCardNoticeIndex]});
            _errorLogicCardNoticeIndex = (_errorLogicCardNoticeIndex + 1) % 3;
            return;
        }
        // todo 进入结局对话
        DeductionCanvas.SetActive(false);
        endingCanvas.SetActive(true);
        // DialogueManager.Instance.SwitchToEndingDialogue();
    }

    // 显示提示
    public void ShowNotice(string[] text)
    {
        noticeCanvas.GetComponent<NoticeCanvas>().SetContent(text);
        noticeCanvas.SetActive(true);
    }

    // 禁用所有交互
    public void DisableAllInteractable(bool isOnlyCard = false)
    {
        Deduction1.GetComponent<SelectCardItem>().isInteractable = false;
        Deduction2.GetComponent<SelectCardItem>().isInteractable = false;
        Deduction3.GetComponent<SelectCardItem>().isInteractable = false;
        Deduction4.GetComponent<SelectCardItem>().isInteractable = false;
        Deduction5.GetComponent<SelectCardItem>().isInteractable = false;
        Deduction6.GetComponent<SelectCardItem>().isInteractable = false;
        LogicCard1.GetComponent<LogicCard>().isInteractable = false;
        LogicCard2.GetComponent<LogicCard>().isInteractable = false;
        LogicCard3.GetComponent<LogicCard>().isInteractable = false;
        if (!isOnlyCard)
        {
            validateEndingButton.GetComponent<ValidateEndingButton>().isInteractable = false;
        }
        
    }
    
    // 启用所有交互
    public void EnableAllInteractable()
    {
        Deduction1.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction2.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction3.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction4.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction5.GetComponent<SelectCardItem>().isInteractable = true;
        Deduction6.GetComponent<SelectCardItem>().isInteractable = true;
        LogicCard1.GetComponent<LogicCard>().isInteractable = true;
        LogicCard2.GetComponent<LogicCard>().isInteractable = true;
        LogicCard3.GetComponent<LogicCard>().isInteractable = true;
        validateEndingButton.GetComponent<ValidateEndingButton>().isInteractable = true;
    }
    
    // todo 测试用
    public void EnterEnding(string ending)
    {
        Debug.Log(_endings[ending]);
        SplitTextIntoParagraphs(_endings[ending]);
        DeductionCanvas.SetActive(false);
        endingCanvas.SetActive(true);
    }
    
    // 结局文本分段
    private void SplitTextIntoParagraphs(string text)
    {
        _paragraphs.Clear();
        var lines = text.Split('\n');
        Debug.Log(lines.Length);
        var currentParagraph = "";

        foreach (var line in lines)
        {
            Debug.Log(line);
            if (string.IsNullOrWhiteSpace(line))
            {
                if (!string.IsNullOrWhiteSpace(currentParagraph))
                {
                    _paragraphs.Add(currentParagraph.Trim());
                    currentParagraph = "";
                }
            }
            else
            {
                if (currentParagraph.Length > 0)
                {
                    currentParagraph += "\n";
                }
                currentParagraph += line;
            }
        }

        if (!string.IsNullOrWhiteSpace(currentParagraph))
        {
            _paragraphs.Add(currentParagraph.Trim());
        }
    }

    // 结局画布获取下一段文字
    public string GetNextParagraph()
    {
        if (_paragraphIndex >= _paragraphs.Count) return null;
        var text = _paragraphs[_paragraphIndex];
        _paragraphIndex++;
        return text;

    }
}
