using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单管理器 - 处理主菜单界面的按钮点击
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// 开始游戏按钮点击事件处理
    /// 在主菜单的开始按钮上调用
    /// </summary>
    public void OnStartGameClicked()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.StartGame();
        else
        { 
            // 如果场景加载器不存在，使用直接加载方式
            Debug.LogWarning("SceneLoader未找到，使用直接加载方式");
            UnityEngine.SceneManagement.SceneManager.LoadScene("DemoScene");
        }    
    }
}