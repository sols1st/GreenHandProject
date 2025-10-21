using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 对话事件系统 - 负责处理对话中的各种事件触发
/// 使用单例模式，跨场景不销毁
/// </summary>
public class DialogueEventSystem : MonoBehaviour
{
    // 单例实例，可以通过DialogueEventSystem.Instance访问
    public static DialogueEventSystem Instance;

    public UnityEvent OnShowBigPoster;//显示大字报事件
    public UnityEvent OnGameEnd;//游戏结束事件

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁
        }
        else
        {
            Destroy(gameObject); // 如果已有实例，销毁重复的对象
        }
        InitializeEvents();
    }
        private void InitializeEvents()
    {
        if (OnShowBigPoster == null)
            OnShowBigPoster = new UnityEvent();
        if (OnGameEnd == null)
            OnGameEnd = new UnityEvent();
    }

    /// <summary>
    /// 触发大字报显示事件
    /// 供其他脚本调用
    /// </summary>
    public void TriggerBigPoster()
    {
        Debug.Log("DialogueEventSystem: 触发大字报显示");
        OnShowBigPoster.Invoke();
    }
}


