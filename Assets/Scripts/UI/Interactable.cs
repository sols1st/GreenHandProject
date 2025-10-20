using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public bool isInteractable = true;
    /// <summary>
    /// 当点击此物体时触发
    /// 子类需要实现具体的交互逻辑
    /// </summary>
    public abstract void TriggerOnClick();
}
