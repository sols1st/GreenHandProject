using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单交互 - 点击任意位置开始游戏
/// </summary>
public class MainMenuInteractable : Interactable
{
    public bool enableBackupClickDetection = true;//备用点击检测
    private void Update()
    {
        if (enableBackupClickDetection && Input.GetMouseButtonDown(0))
        {
            TriggerOnClick();
        }
    }
    public override void TriggerOnClick()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.StartGame();
        }
        else
        {
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }
    }
}