using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown; // 您的Dropdown组件
    private string defaultOptionText = "Please Select Card";
    
    private void OnEnable()
    {
        // 检查Dropdown是否已赋值
        if (dropdown == null)
        {
            Debug.LogError("TMP_Dropdown component is not assigned!");
            return;
        }

        // 初始化Dropdown
        InitializeDropdown();

        // 获取卡牌列表并填充
        List<string> cards = GetCards();
        PopulateDropdown(cards);
    }
    
    // 获取卡牌列表的虚拟方法，子类可以重写
    protected virtual List<string> GetCards()
    {
         // return new List<string>(){"CN01", "CN02", "CN03"};
        return CardManager.Instance.GotCards();
    }
    
    // 初始化Dropdown
    private void InitializeDropdown()
    {
        // 设置默认值为-1，表示不选中任何值
        dropdown.value = -1;
        dropdown.RefreshShownValue();

        // 确保Dropdown的值变化事件正确设置
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }
    
    // 填充Dropdown选项
    public void PopulateDropdown(List<string> options)
    {
        // 清除所有现有选项
        dropdown.options.Clear();

        // 添加新选项
        foreach (string option in options)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        }

        // 设置为无选择状态（不触发 RefreshShownValue）
        dropdown.value = -1;
        dropdown.captionText.text = defaultOptionText; // 清空显示文本
    }
    
    // 当Dropdown值改变时的回调
    private void OnDropdownValueChanged(int newValue)
    {

        // Debug.Log("选择了卡牌: " + dropdown.options[newValue].text);
    }
    
    // 在被销毁时清理
    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
}


