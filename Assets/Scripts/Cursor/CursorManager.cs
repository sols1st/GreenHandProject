using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor Sprites")]
    [Tooltip("默认光标图片")]
    public Texture2D defaultCursor;

    [Tooltip("向左移动时的光标图片")]
    public Texture2D leftCursor;

    [Tooltip("向右移动时的光标图片")]
    public Texture2D rightCursor;

    [Tooltip("点击时的光标图片")]
    public Texture2D clickCursor;

    [Tooltip("悬停时的光标图片")]
    public Texture2D hoverCursor;

    [Header("Cursor Settings")]
    [Tooltip("光标点击点位置（相对于图片左上角），留空则使用(0,0)确保左上角对齐")]
    public Vector2 hotSpot = Vector2.zero;

    [Tooltip("光标模式")]
    public CursorMode cursorMode = CursorMode.Auto;

    [Header("Movement Detection")]
    [Tooltip("移动速度阈值，超过此值才切换光标")]
    public float movementThreshold = 50f;

    [Tooltip("采样时间间隔（秒）")]
    public float sampleInterval = 0.1f;

    [Header("Behavior Settings")]
    [Tooltip("是否保持最后一次移动方向的光标")]
    public bool maintainLastDirection = true;

    private Texture2D currentCursor;
    private CursorType currentCursorType = CursorType.Default;
    private Vector2 lastMousePosition;
    private Vector2 mouseVelocity;
    private float sampleTimer;
    private bool isCursorEnabled = false;
    private CursorType lastDirectionType = CursorType.Default; // 默认应该是Default，不是Right
    private InputSystem _inputSystem;
    private string lastInteractableName = ""; // 存储上一帧检测到的可交互物体名称

    public static CursorManager Instance { get; private set; }

    public enum CursorType
    {
        System,    // 系统默认光标
        Default,   // 自定义默认光标
        Left,      // 向左移动光标
        Right,     // 向右移动光标
        Hover,     // 悬停光标
        Click      // 点击时光标
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化
        _inputSystem = new InputSystem();
        _inputSystem.Enable();
        _inputSystem.asset.FindActionMap("Game")?.Enable();
        
        lastMousePosition = Input.mousePosition;
        currentCursor = null; // Sprite类型不能直接赋值给Texture2D
        currentCursorType = CursorType.System;
        
    }

    private void Start()
    {
        _inputSystem.Game.Click.performed += OnClickPerformed;
        // 默认禁用自定义光标
        DisableCustomCursor();
    }

    private void Update()
    {
        // 检测可交互物体并处理相关逻辑
        HandleInteractableDetection();
    }

    /// <summary>
    /// 处理可交互物体检测和光标显示
    /// </summary>
    private void HandleInteractableDetection()
    {
        // 检测鼠标下的可交互物体
        var interactableGameObject = GetInteractable();
        var hasInteractable = false;
        string currentInteractableName = "";

        if (interactableGameObject != null)
        {
            var interactable = interactableGameObject.GetComponent<Interactable>();
            if (interactable != null && interactable.isInteractable && interactable.isHover)
            {
                hasInteractable = true;
                currentInteractableName = interactableGameObject.name;

                // 只有当前物体名称与上一帧不同时才播放音效
                if (currentInteractableName != lastInteractableName)
                {
                    AudioManager.Instance.PlaySoundEffect("V002");
                }
                HandleOnInteractableHover();
            }
        }

        // 如果没有可交互物体，将名称置空
        if (!hasInteractable)
        {
            currentInteractableName = "";
            HandleOnNoInteractable();
        }

        // 更新上一帧的物体名称
        lastInteractableName = currentInteractableName;

        // 只有在启用自定义光标时才更新光标
        if (isCursorEnabled)
        {
            UpdateCursorDisplay();
        }
    }

    /// <summary>
    /// 当悬停在可交互物体上时的处理
    /// </summary>
    private void HandleOnInteractableHover()
    {
        
        // 更新状态
        if (!isCursorEnabled) return;
        SetHoverCursor();
        lastDirectionType = CursorType.Hover;

    }

    /// <summary>
    /// 当没有可交互物体时的处理
    /// </summary>
    private void HandleOnNoInteractable()
    {
        if (isCursorEnabled)
        {
            // 没有可交互物体时的处理逻辑
            if (_inputSystem.Game.Click.IsPressed())
            {
                // 点击状态下显示click光标
                SetClickCursor();
            }
            else
            {
                // 没有点击且没有可交互物体时，更新移动检测
                UpdateMouseMovement();

                // 根据移动情况设置最后方向类型
                if (Mathf.Abs(mouseVelocity.x) > movementThreshold)
                {
                    if (mouseVelocity.x < 0)
                    {
                        lastDirectionType = CursorType.Left;
                    }
                    else
                    {
                        lastDirectionType = CursorType.Right;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新光标显示
    /// </summary>
    private void UpdateCursorDisplay()
    {
        // 如果当前已经是悬停状态，不需要更新
        if (lastDirectionType == CursorType.Hover)
        {
            return;
        }

        // 根据当前状态更新光标
        if (_inputSystem.Game.Click.IsPressed())
        {
            SetClickCursor();
        }
        else
        {
            // 根据最后方向类型设置光标
            ApplyLastDirection();
        }
    }

    private void OnDestroy()
    {
        _inputSystem.Game.Click.performed -= OnClickPerformed;
        _inputSystem.Disable();
        _inputSystem = null;
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        var interactableGameObject = GetInteractable();
        if (interactableGameObject == null) return;
        if (interactableGameObject.layer != LayerMask.NameToLayer("Dialogue") &&
            interactableGameObject.layer != LayerMask.NameToLayer("NewCard"))
        {
            AudioManager.Instance.PlaySoundEffect("V001");
        }
        var interactable = interactableGameObject.GetComponent<Interactable>();
        
        if (interactable != null && interactable.isInteractable)
        {
            interactable.TriggerOnClick();
        }
    }

    private GameObject GetInteractable()
    {
        var pointerPosition = Mouse.current != null && Mouse.current.position.ReadValue() != default
            ? Mouse.current.position.ReadValue()
            : Vector2.zero;
        // HandleMouseClick(pointerPosition);
        var interactable = DetectCanvasUIGO(pointerPosition);
        if (interactable != null)
        {
            return interactable;
        }
        interactable = DetectCanvas2DObjectsGO(pointerPosition);
        if (interactable != null)
        {
            return interactable;
        }
        interactable = Detect2DObjectGO(pointerPosition);
        if (interactable != null)
        {
            return interactable;
        }
        return null;
    }

    /// <summary>
    /// 处理鼠标点击事件，返回GameObject
    /// </summary>
    private GameObject DetectCanvasUIGO(Vector2 mousePosition)
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0)
        {
            return null;
        }
        // 检测目标层级（忽略OverlayUI层级画布）
        var targetLayer = results[0].gameObject.layer;
        var hitObject = results[0].gameObject;
        foreach (var result in results)
        {
            targetLayer = result.gameObject.layer;
            hitObject = result.gameObject;
            if (targetLayer != LayerMask.NameToLayer("OverlayUI"))
            {
                break;
            }
        }
        if (targetLayer == LayerMask.NameToLayer("OverlayUI"))
        {
            return hitObject;
        }
        foreach (var hit in results)
        {
            hitObject = hit.gameObject;
            if (hitObject.layer != targetLayer) return null;
            while (hitObject != null)
            {
                var interactable = hitObject.GetComponent<Interactable>();
                if (interactable != null)
                {
                    return hitObject;
                }
                hitObject = hitObject.transform.parent?.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// 检测2D对象被点击，返回GameObject
    /// </summary>
    private GameObject Detect2DObjectGO(Vector2 mousePosition)
    {
        // 将屏幕坐标转换为世界坐标
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 worldPosition2D = new Vector2(worldPosition.x, worldPosition.y);

        // 使用2D raycast检测所有2D对象
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(worldPosition2D, Vector2.zero);
        foreach (var hit2D in hits2D)
        {
            var interactable = hit2D.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                return hit2D.collider.gameObject;
            }

            var parent = hit2D.collider.transform.parent;
            while (parent != null)
            {
                var parentInteractable = parent.GetComponent<Interactable>();
                if (parentInteractable != null && parentInteractable.isInteractable)
                {
                    parentInteractable.TriggerOnClick();
                    return parent.gameObject;
                }
                parent = parent.parent;
            }
        }
        return null;
    }
    

    /// <summary>
    /// 检测Canvas下的2D物体（使用屏幕坐标，与场景2D物体检测逻辑相同），返回GameObject
    /// </summary>
    public GameObject DetectCanvas2DObjectsGO(Vector2 mousePosition)
    {
        // 使用2D raycast检测所有2D对象
        var hits2D = Physics2D.RaycastAll(mousePosition, Vector2.zero);
        foreach (var hit2D in hits2D)
        {
            // 检查是否有Interactable组件
            var interactable = hit2D.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.isInteractable)
            {
                return hit2D.collider.gameObject;
            }

            // 检查父对象是否有Interactable组件
            var parent = hit2D.collider.transform.parent;
            while (parent != null)
            {
                var parentInteractable = parent.GetComponent<Interactable>();
                if (parentInteractable != null && parentInteractable.isInteractable)
                {
                    return parent.gameObject;
                }
                parent = parent.parent;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 启用自定义光标
    /// </summary>
    public void EnableCustomCursor()
    {
        if (!isCursorEnabled)
        {
            isCursorEnabled = true;

            // 先启用光标显示
            Cursor.visible = true;

            // 延迟一帧再设置自定义光标，确保设置生效
            StartCoroutine(EnableCursorDelayed());
        }
    }

    /// <summary>
    /// 禁用自定义光标，恢复系统光标
    /// </summary>
    public void DisableCustomCursor()
    {
        // if (isCursorEnabled)
        // {
            isCursorEnabled = false;
            StopAllCoroutines(); // 停止所有协程
            ResetToSystemCursor(); // 恢复系统光标
        // }
    }

    /// <summary>
    /// 延迟启用光标的协程
    /// </summary>
    private IEnumerator EnableCursorDelayed()
    {
        // 等待一帧确保系统状态稳定
        yield return null;

        // 设置自定义光标
        SetDefaultCursor();

        // 确保光标可见
        Cursor.visible = true;

        Debug.Log($"Custom cursor enabled, lastDirectionType={lastDirectionType}");
    }
    
    /// <summary>
    /// 更新鼠标移动检测
    /// </summary>
    private void UpdateMouseMovement()
    {
        sampleTimer += Time.deltaTime;

        if (sampleTimer >= sampleInterval)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 deltaPosition = currentMousePosition - lastMousePosition;

            // 计算鼠标速度（像素/秒）
            mouseVelocity = deltaPosition / sampleInterval;

            lastMousePosition = currentMousePosition;
            sampleTimer = 0f;
        }
    }

    /// <summary>
    /// 根据鼠标移动更新光标显示
    /// </summary>
    private void UpdateCursor()
    {
        // 检查鼠标移动速度是否超过阈值
        if (Mathf.Abs(mouseVelocity.x) > movementThreshold)
        {
            // 根据水平移动方向切换光标
            if (mouseVelocity.x < 0)
            {
                SetLeftCursor();
                lastDirectionType = CursorType.Left;
            }
            else
            {
                SetRightCursor();
                lastDirectionType = CursorType.Right;
            }
        }
        else
        {
            // 静止或移动缓慢时的处理
            if (maintainLastDirection)
            {
                // 保持最后一次移动方向的光标
                ApplyLastDirection();
            }
            else
            {
                // 显示默认光标
                SetDefaultCursor();
            }
        }
    }

    /// <summary>
    /// 设置为默认光标
    /// </summary>
    private void SetDefaultCursor()
    {
        SetCursor(defaultCursor, CursorType.Default);
    }

    /// <summary>
    /// 设置为向左移动的光标
    /// </summary>
    private void SetLeftCursor()
    {
        SetCursor(leftCursor, CursorType.Left);
    }

    /// <summary>
    /// 设置为向右移动的光标
    /// </summary>
    private void SetRightCursor()
    {
        // 选择你想要的行为：
        // 方式1：视觉位置一致，左上角点击点（取消下一行注释）
        // SetCursorWithHotSpot(rightCursor, CursorType.Right, Vector2.zero);

        // 方式2：右上角点击点（取消下一行注释）
        SetCursorWithHotSpot(rightCursor, CursorType.Right, new Vector2(rightCursor.width, 0));
    }

    /// <summary>
    /// 设置为点击时的光标
    /// </summary>
    private void SetClickCursor()
    {
        SetCursor(clickCursor, CursorType.Click);
    }

    /// <summary>
    /// 设置为悬停时的光标
    /// </summary>
    private void SetHoverCursor()
    {
        SetCursor(hoverCursor, CursorType.Hover);
    }

    /// <summary>
    /// 使用自定义热点设置光标
    /// </summary>
    private void SetCursorWithHotSpot(Texture2D texture, CursorType type, Vector2 hotSpot)
    {
        if (texture != null)
        {
            Cursor.SetCursor(texture, hotSpot, cursorMode);
            currentCursor = texture;
            currentCursorType = type;
        }
        else if (type == CursorType.Default)
        {
            // 检查是否已经是系统光标
            if (currentCursorType != CursorType.System)
            {
                ResetToSystemCursor();
            }
        }
    }

    
    /// <summary>
    /// 应用最后一次移动方向的光标
    /// </summary>
    private void ApplyLastDirection()
    {
        switch (lastDirectionType)
        {
            case CursorType.Left:
                SetLeftCursor();
                break;
            case CursorType.Right:
                SetRightCursor();
                break;
            case CursorType.Hover:
                SetHoverCursor();
                break;
            default:
                SetDefaultCursor();
                break;
        }
    }

    /// <summary>
    /// 根据鼠标移动方向自动切换光标
    /// </summary>
    public void SetDirectionalCursor(Vector2 mouseMovement)
    {
        if (mouseMovement.x < 0)
        {
            SetLeftCursor();
        }
        else if (mouseMovement.x > 0)
        {
            SetRightCursor();
        }
        else
        {
            SetDefaultCursor();
        }
    }

    /// <summary>
    /// 恢复系统默认光标
    /// </summary>
    public void ResetToSystemCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
        currentCursor = null;
        currentCursorType = CursorType.Default;
    }
    

    /// <summary>
    /// 内部方法：设置光标
    /// </summary>
    private void SetCursor(Texture2D texture, CursorType type)
    {
        if (texture != null)
        {
            // 检查是否与当前光标相同，避免重复设置
            if (currentCursor == texture && currentCursorType == type)
            {
                return;
            }

            // 默认使用(0,0)作为热点，确保自定义光标左上角与系统默认光标左上角对齐
            Vector2 defaultHotSpot = Vector2.zero;

            // 如果用户设置了自定义热点，则使用用户设置，否则使用(0,0)保证左上角对齐
            Vector2 finalHotSpot = (hotSpot != Vector2.zero) ? hotSpot : defaultHotSpot;

            Cursor.SetCursor(texture, finalHotSpot, cursorMode);
            currentCursor = texture;
            currentCursorType = type;
        }
        else if (type == CursorType.Default)
        {
            // 检查是否已经是系统光标
            if (currentCursorType != CursorType.System)
            {
                ResetToSystemCursor();
            }
        }
    }

  }
