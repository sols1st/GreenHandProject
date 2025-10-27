using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // 场景切换方法
    public void SwitchToGameScene()
    {
        // 使用 Unity 内置的 SceneManager，而非自定义类
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}