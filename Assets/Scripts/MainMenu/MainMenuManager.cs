using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // ³¡¾°ÇÐ»»·½·¨
    public void SwitchToGameScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
