using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // �����л�����
    public void SwitchToGameScene()
    {
        // ʹ�� Unity ���õ� SceneManager�������Զ�����
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}