using UnityEngine;
using Cinemachine;

//第一次自己试着写一个脚本，最重要的还是 逻辑 逻辑 逻辑
//逻辑就是“功能的运作和实现过程”
//我自己尝试的逻辑是“首先找到组件中的边界变量，然后获取”
//这并不是一个完整的运行功能

//--------------------------------------------------------------

//正确的逻辑是 ：在加载下个场景时，检索一个“BoundsCollider”标签的GameObject并获取这个GameObject的
//PolygonCollider2D组件，把获取的状态赋值给PolygonCollider2D类型的变量——confiner.

//获取摄像机的“Cinemachine”组件，把之前拿到的 gameobject的PolygonCollider给摄像机的“m_BoundingShape”

public class SwitchBounds : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += SwitchConfinerShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= SwitchConfinerShape;
    }

    private void SwitchConfinerShape()
    {
        PolygonCollider2D confinerShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();

        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();

        confiner.m_BoundingShape2D = confinerShape;

 //Call this if the bounding shape's points change at runtime
        confiner.InvalidatePathCache();
    }
}
