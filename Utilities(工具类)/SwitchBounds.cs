using UnityEngine;
using Cinemachine;

//��һ���Լ�����дһ���ű�������Ҫ�Ļ��� �߼� �߼� �߼�
//�߼����ǡ����ܵ�������ʵ�ֹ��̡�
//���Լ����Ե��߼��ǡ������ҵ�����еı߽������Ȼ���ȡ��
//�Ⲣ����һ�����������й���

//--------------------------------------------------------------

//��ȷ���߼��� ���ڼ����¸�����ʱ������һ����BoundsCollider����ǩ��GameObject����ȡ���GameObject��
//PolygonCollider2D������ѻ�ȡ��״̬��ֵ��PolygonCollider2D���͵ı�������confiner.

//��ȡ������ġ�Cinemachine���������֮ǰ�õ��� gameobject��PolygonCollider��������ġ�m_BoundingShape��

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
