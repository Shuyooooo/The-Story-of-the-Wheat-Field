using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mfarm.Save;


public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;
    private Button currentButton;
    /// <summary>
    /// 集中存放的数据
    /// </summary>
    private DataSlot currentData;

    private int Index => transform.GetSiblingIndex();

    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }
    private void OnEnable()
    {
        SetupSlotUI();
    }

    private void SetupSlotUI()
    {
        //一共就3条
        currentData = SaveLoadManager.Instance.LoadDataSlots[Index];

        if(currentData != null)
        {
            dataTime.text = currentData.DataTime;
            dataScene.text = currentData.DataScene;
        }
        else 
        {
            dataTime.text = "The world has yet to begin ,";
            dataScene.text = "the dream has yet to unfold";
        }
    }


    private void LoadGameData()
    {
        if(currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            Debug.Log("新游戏");
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}
