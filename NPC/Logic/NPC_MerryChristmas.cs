using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class NPC_MerryChristmas : MonoBehaviour
{
    public GameObject MerryChristmas;

    private void Start()
    {
        Transform panel = MerryChristmas.transform.GetChild(0);
    }
    
    private void ToRicky(Transform panel)
    {
        if (panel != null)
        {
            panel.gameObject.SetActive(true); // �����Ӷ���
        }
        else
        {
            Debug.LogError("��һ���Ӷ����� Panel ��δ�ҵ�");
        }
    }
    // ����Ӷ����Ƿ���ڲ����� "Panel"
    
}


