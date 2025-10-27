using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickDiagnostic : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("=== 点击诊断 ===");
            Debug.Log($"鼠标位置: {Input.mousePosition}");
            Debug.Log($"SceneLoader 实例: {SceneLoader.Instance != null}");
            Debug.Log($"当前活动场景: {SceneManager.GetActiveScene().name}");

            // 直接尝试加载场景
            if (SceneLoader.Instance != null)
            {
                Debug.Log("通过 SceneLoader 加载场景");
                SceneLoader.Instance.StartGame();
            }
            else
            {
                Debug.Log("SceneLoader 为 null，直接加载场景");
                SceneManager.LoadScene(2, LoadSceneMode.Single);
            }
        }
    }
}