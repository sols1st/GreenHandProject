using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SceneController : MonoBehaviour
{
    private InputSystem _inputSystem;
    public GameObject endPoint;
    public GameObject sceneCanvas;

    private void Awake()
    {
        _inputSystem = new InputSystem();
        _inputSystem.Enable();
        _inputSystem.asset.FindActionMap("Game")?.Enable();
    }

    private void Start()
    {
        // 注册InputSystem点击事件
        _inputSystem.Game.Click.performed += OnClickPerformed;
        endPoint.GetComponent<EndPoint>().CheckIsActive();
        CardManager.Instance.UpdateEndPoint(endPoint);
        CardManager.Instance.UpdateSceneCanvas(sceneCanvas);
    }

    private void OnDestroy()
    {
        // 注销InputSystem点击事件
        _inputSystem.Game.Click.performed -= OnClickPerformed;
        _inputSystem.Disable();
        _inputSystem = null;
    }

    /// <summary>
    /// 处理点击事件
    /// </summary>
    private void OnClickPerformed(InputAction.CallbackContext context)
    {

        // 获取鼠标点击位置
        // 检测鼠标位置
        var pointerPosition = Mouse.current != null && Mouse.current.position.ReadValue() != default
            ? Mouse.current.position.ReadValue()
            : Vector2.zero;
        HandleMouseClick(pointerPosition);
    }

    /// <summary>
    /// 处理鼠标点击事件
    /// </summary>
    private void HandleMouseClick(Vector2 mousePosition)
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        

        if (results.Count == 0)
        {
            Detect2DObject();
            return;
        }
        // 检测目标层级（忽略OverlayUI层级画布）
        var targetLayer = results[0].gameObject.layer;
        foreach (var result in results)
        {
            targetLayer = result.gameObject.layer;
            if (targetLayer != LayerMask.NameToLayer("OverlayUI"))
            {
                break;
            }
        }
        if (targetLayer == LayerMask.NameToLayer("OverlayUI"))
        {
            return;
        }
        foreach (var hit in results)
        {
            var hitObject = hit.gameObject;
            if (hitObject.layer != targetLayer) return;
            // 向上查找直到找到有Interactable的父对象
            while (hitObject != null)
            {
                var interactable = hitObject.GetComponent<Interactable>();
                if (interactable != null)
                {
                    if (interactable.isInteractable)
                    {
                        interactable.TriggerOnClick();
                    }
                    return;
                }
                hitObject = hitObject.transform.parent?.gameObject;
            }
        }
        Detect2DObject();
    }

    /// <summary>
    /// 检测2D对象被点击
    /// </summary>
    private void Detect2DObject()
    {
        // 将屏幕坐标转换为世界坐标
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPosition2D = new Vector2(worldPosition.x, worldPosition.y);

        // 使用2D raycast检测所有2D对象
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(worldPosition2D, Vector2.zero);
        foreach (var hit2D in hits2D)
        {
            
            // 检查是否有Interactable组件
            Interactable interactable = hit2D.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.isInteractable)
            {
                interactable.TriggerOnClick();
                return;
            }

            // 检查父对象是否有Interactable组件
            Transform parent = hit2D.collider.transform.parent;
            while (parent != null)
            {
                Interactable parentInteractable = parent.GetComponent<Interactable>();
                if (parentInteractable != null && parentInteractable.isInteractable)
                {
                    parentInteractable.TriggerOnClick();
                    return;
                }
                parent = parent.parent;
            }
        }
    }
}
