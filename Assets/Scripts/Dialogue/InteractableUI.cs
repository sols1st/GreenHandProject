using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableUI : MonoBehaviour, IPointerClickHandler
{
    private Interactable interactable;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
        if (interactable == null)
            Debug.LogError($"[{name}] ��Ҫ���ء�Interactable������ű���");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        interactable?.TriggerOnClick(); // ת������¼����Զ����߼�
    }
}