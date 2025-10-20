using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableUI : MonoBehaviour, IPointerClickHandler
{
    private Interactable interactable;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        if (interactable == null)
            Debug.LogError($"[{name}] 需要挂载「Interactable」子类脚本！");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        interactable?.TriggerOnClick(); // 转发点击事件到自定义逻辑
    }
}