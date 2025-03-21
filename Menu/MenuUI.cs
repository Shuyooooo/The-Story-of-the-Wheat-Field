using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public GameObject[] panels;

    /// <summary>
    /// 如果点击了第三个，就把1、2垫底
    /// </summary>
    /// <param name="index"></param>
    public void SwitchPanel(int index)
    {
        for (int i =0; i < panels.Length; i++)
        {
            if (i == index) 
            {
                panels[i].transform.SetAsLastSibling();
            }
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("结婚咯");
    }
}
