using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // �����л�����
    public void SwitchToGameScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
