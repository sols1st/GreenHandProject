using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单交互 - 点击任意位置开始游戏
/// </summary>
public class MainMenuInteractable : Interactable
{
    public override void TriggerOnClick()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.StartGame();
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DemoScene");
        }
    }
}