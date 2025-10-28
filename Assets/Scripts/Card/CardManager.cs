using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    public GameObject newCard;    
    public Image newCardImage;
    public TMP_Text newCardName;
    public GameObject cardDetailCanvas; 
    public Image cardDetailImage;
    public TMP_Text cardDetailName;
    public GameObject endChoiceCanvas;
    public GameObject endButton;
    public GameObject cardBagButton;
    private Dictionary<string, Card> _allCards = new Dictionary<string, Card>();
    private GameObject _endPoint;
    // public GameObject deductionCanvas;
    private int _currentChapter = 1;
    private bool _isFirst = true;
    
    private void Awake()
    {
        Debug.Log("card manager awake");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCardFromFile();

    }

    // 加载卡牌数据
    private void LoadCardFromFile()
    {
        var path = Path.Combine(Application.dataPath, "Resources/Card/CardData.csv");

        // 清空现有数据
        _allCards.Clear();
        var allContent = File.ReadAllText(path);
        var csvLines = ParseCsvWithLineBreaks(allContent);

        // 跳过标题行，从第二行开始处理
        for (var i = 1; i < csvLines.Count; i++)
        {
            var line = csvLines[i];
            if (line.Count >= 5)
            {
                // 创建卡牌对象
                var card = new Card()
                {
                    id = line[0].Trim(),
                    type = line[1].Trim(),
                    function = line[2].Trim().Split(' '),
                    name = line[3].Trim(),
                    summary = line[4].Trim(),
                    // plot = line[3].Trim(),
                    image = line[0].Trim(),
                    
                };
                // todo 测试使用
                // var testCards = new List<string> { "CA016", "CA020", "CA010", "CA019", "CA028", "CA033"};
                // if (testCards.Contains(card.id)) card.isGot = true;
                card.isGot = true;
                _allCards.Add(card.id, card);
            }
        }
    }

    // 加载结局数据
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

    // 判断是否拥有卡牌
    public bool HasCards(string[] cards)
    {
        foreach (var card in cards)
        {
            if (!_allCards[card].isGot) return false;
        }
        return true;
    }

    // 获取新卡牌
    public void GetNewCard(string cardName)
    {
        var card = _allCards[cardName];
        card.isGot = true;
        // 获取卡牌音效
        AudioManager.Instance.PlaySoundEffect("V003");
        ActivateNewCardUI(cardName);
        // 第一次拿到卡牌的时候，出现卡牌库的展开按钮
        if (_isFirst)
        {
            cardBagButton.SetActive(true);
            _isFirst = false;
        }
        CardBagManager.Instance.RefreshNewCard();
        // todo 判断是否能够结束自由探索
        if (_currentChapter == 1)
        {
            if (HasCards(new [] {"CA003", "CA006"}))
            {
                ShowEndChoice();
            }
        }
        else if(_currentChapter == 2)
        {
            if (HasCards(new [] {"CA016", "CA020"}))
            {
                ShowEndChoice();
            }
        }
    }

    public void ShowEndChoice()
    {
        endButton.SetActive(true);
        endChoiceCanvas.SetActive(true);
    }
    
    // 获取新卡牌UI
    private void ActivateNewCardUI(string cardName)
    {
        var card = _allCards[cardName];
        // todo 路径修改
        // var imagePath = "CardTestFiles/Images" + card.image;
        var imagePath = "Card/DetailImages/" + cardName;
        var image = Resources.Load<Sprite>(imagePath);
        newCardImage.sprite = image;
        // newCardName.text = card.name;
        newCard.SetActive(true);
    }

    // public void UpdateEndPoint(GameObject endPoint)
    // {
    //     _endPoint = endPoint;
    // }

    // 获取指定类型的卡牌
    public List<Card> GetTypeCards(string type = "", string[] functions = null)
    {
        var cards = new List<Card>();
        var gotCards = new List<Card>();
        var notGotCards = new List<Card>();

        foreach (var card in _allCards.Values)
        {
            if (type != "" && card.type != type)
            {
                continue;
            }

            // 如果functions不为null，判断card.functions是否包含functions中的任何一个
            if (functions != null && functions.Length > 0)
            {
                bool hasMatchingFunction = false;
                foreach (var requiredFunction in functions)
                {
                    foreach (var cardFunction in card.function)
                    {
                        if (cardFunction == requiredFunction)
                        {
                            hasMatchingFunction = true;
                            break;
                        }
                    }
                    if (hasMatchingFunction)
                        break;
                }
                if (!hasMatchingFunction)
                {
                    continue;
                }
            }
            if (card.isGot)
            {
                gotCards.Add(card);
            }
            else
            {
                notGotCards.Add(card);
            }
        }

        cards.AddRange(gotCards);
        cards.AddRange(notGotCards);
        return cards;
    }

    // public bool IsEnding(string firstCardName, string secondCardName)
    // {
    //     var cardNames = firstCardName + secondCardName;
    //     if (_endings.ContainsKey(cardNames))
    //     {
    //         _endingSelectedCards = cardNames;
    //         return true;
    //     }
    //     cardNames = secondCardName + firstCardName;
    //     if (_endings.ContainsKey(cardNames))
    //     {
    //         _endingSelectedCards = cardNames;
    //         return true;
    //     }
    //     return false;
    // }

    // public void ShowDeductionCanvas()
    // {
    //     deductionCanvas.SetActive(true);
    // }
    
    // public void ShowEnding()
    // {
    //     endingCanvas.GetComponent<Ending>().SetText(_endings[_endingSelectedCards]);
    //     endingCanvas.SetActive(true);
    // }
    
    public void ShowCardDetail(string cardID)
    {
        // var card = _allCards[cardID];
        // todo 路径修改、详情UI修改
        // var imagePath = "CardTestFiles/Images" + card.image;
        var imagePath = "Card/DetailImages/" + cardID;
        var image = Resources.Load<Sprite>(imagePath);
        cardDetailImage.sprite = image;
        // cardDetailName.text = card.name;
        cardDetailCanvas.SetActive(true); 
    }

    public Card GetCardInfo(string cardID)
    {
        var card = _allCards[cardID];
        return card;
    }

    public void UpdateChapter(int chapter)
    {
        _currentChapter = chapter;
    }
}
