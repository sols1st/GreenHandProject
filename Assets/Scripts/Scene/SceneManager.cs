using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 场景管理器 - 负责全局交互监听和管理
/// 每个场景都需要一个实例来处理该场景的交互逻辑
/// </summary>
public class SceneManager : MonoBehaviour
{

    private InputSystem _inputSystem;

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
        // todo 结束位置检测
    }

    private void OnDestroy()
    {
        // 注销InputSystem点击事件
        _inputSystem.Game.Click.performed -= OnClickPerformed;
    }

    /// <summary>
    /// 处理点击事件
    /// </summary>
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("OnClickPerformed");

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
            return;
        }
        // 检测目标层级
        var targetLayer = results[0].gameObject.layer;
        foreach (var hit in results)
        {
            var hitObject = hit.gameObject;
            if (hitObject.layer != targetLayer) return;
            // 向上查找直到找到有Interactable的父对象
            while (hitObject != null)
            {
                Debug.Log("hitParent:" + hitObject.name);
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
    }
    
}