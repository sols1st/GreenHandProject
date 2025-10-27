using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tab : Interactable
{
    public bool isSelected = false;
    public List<Tab> tabs = new List<Tab>();
    public GameObject background;
    private string[] _functions;

    public override void TriggerOnClick()
    {
        if (!isSelected)
        {
            UpdateSelected();
        }
    }

    public void UpdateFunctions(string[] functions)
    {
        _functions = functions;
    }
    
    public void UpdateSelected()
    {
        isSelected = true;
        CardBagManager.Instance.Refresh(false, name, true, _functions);
        background.SetActive(true);
        foreach (var tab in tabs)
        {
            tab.UpdateSelectOthers();
        }
    }

    public void UpdateSelectOthers()
    {
        isSelected = false;
        background.SetActive(false);
    }
}
