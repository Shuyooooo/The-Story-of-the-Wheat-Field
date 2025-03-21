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
            panel.gameObject.SetActive(true); // 激活子对象
        }
        else
        {
            Debug.LogError("第一个子对象不是 Panel 或未找到");
        }
    }
    // 检查子对象是否存在并且是 "Panel"
    
}


