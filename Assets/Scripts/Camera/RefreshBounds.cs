using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class RefreshBounds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SwitchConfinerShape();
    }

    private void SwitchConfinerShape()
    {
        PolygonCollider2D confinerShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();

        CinemachineConfiner2D confiner = GetComponent<CinemachineConfiner2D>();

        confiner.m_BoundingShape2D = confinerShape;

        //Çå³ý»º´æ
        confiner.InvalidateCache();
    }
}

