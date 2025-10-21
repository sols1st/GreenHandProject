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
    private Dictionary<string, Card> _allCards = new Dictionary<string, Card>();
    private List<string> _gotCards = new List<string>();
    private GameObject _endPoint;
    private Dictionary<string, string> _endings  = new Dictionary<string, string>();
    public GameObject deductionCanvas;
    private string _endingSelectedCards;
    public GameObject endingCanvas;
    private GameObject _sceneCanvas;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCardFromFile();
        LoadEndingFromFile();

    }

    private void LoadCardFromFile()
    {
        var path = Path.Combine(Application.dataPath, "Resources/CardTestFiles/CardTestData.csv");

        // 清空现有数据
        _allCards.Clear();
        _gotCards.Clear();


        var csvLines = File.ReadAllLines(path);
       
        // 跳过标题行，从第二行开始处理
        for (var i = 1; i < csvLines.Length; i++)
        {
            var line = csvLines[i].Split(',');

            // 创建卡牌对象
            var card = new Card()
            {
                name = line[0].Trim(),
                summary = line[1].Trim(),
                function = line[2].Trim(),
                plot = line[3].Trim(),
                type = line[4].Trim(),
                image = line[5].Trim(),
            };
            _allCards.Add(card.name, card);
        }
    }

    private void LoadEndingFromFile()
    {
        var path = Path.Combine(Application.dataPath, "Resources/CardTestFiles/EndingTest.csv");

        // 清空现有数据
        _endings.Clear();
        var allLines = File.ReadAllText(path);
        var lines = ParseCsvWithLineBreaks(allLines);

        // 跳过标题行，从第二行开始处理
        for (var i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.Count >= 3)
            {
                // 第1列和第2列组合作为key
                var key = line[0].Trim() + line[1].Trim();
                // 第3列作为值
                var value = line[2].Trim();

                // 添加到字典中
                _endings[key] = value;
            }
        }
    }

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

    public bool HasCards(string[] cards)
    {
        foreach (var card in cards)
        {
            if (!_gotCards.Contains(card)) return false;
        }
        return true;
    }

    public void GetNewCard(string card)
    {
        _gotCards.Add(card);
        ActivateNewCardUI(card);
        _endPoint?.GetComponent<EndPoint>().CheckIsActive();
    }

    private void ActivateNewCardUI(string cardName)
    {
        var card = _allCards[cardName];
        // todo 路径修改
        // var imagePath = "CardTestFiles/Images" + card.image;
        var imagePath = "CardTestFiles/Images/CardTestImage";
        var image = Resources.Load<Sprite>(imagePath);
        newCardImage.sprite = image;
        newCardName.text = card.name;
        newCard.SetActive(true);
    }

    public void UpdateEndPoint(GameObject endPoint)
    {
        _endPoint = endPoint;
    }

    public List<string> GotCards()
    {
        return _gotCards;
    }

    public bool IsEnding(string firstCardName, string secondCardName)
    {
        var cardNames = firstCardName + secondCardName;
        if (_endings.ContainsKey(cardNames))
        {
            _endingSelectedCards = cardNames;
            return true;
        }
        cardNames = secondCardName + firstCardName;
        if (_endings.ContainsKey(cardNames))
        {
            _endingSelectedCards = cardNames;
            return true;
        }
        return false;
    }

    public void ShowDeductionCanvas()
    {
        deductionCanvas.SetActive(true);
    }
    
    public void ShowEnding()
    {
        endingCanvas.GetComponent<Ending>().SetText(_endings[_endingSelectedCards]);
        endingCanvas.SetActive(true);
    }

    public void UpdateSceneCanvas(GameObject canvas)
    {
        _sceneCanvas = canvas;
    }
    
    public void HideSceneCanvas()
    {
        _sceneCanvas.SetActive(false);
    }
}
