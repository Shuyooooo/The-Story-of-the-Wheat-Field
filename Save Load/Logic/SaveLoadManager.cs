using Mfarm.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// “模块Manager”
/// </summary>
public class SaveLoadManager : Singleton<SaveLoadManager>
{
    private List<Isavable> savableList = new List<Isavable>();

    public List<DataSlot> LoadDataSlots = new List<DataSlot>(new DataSlot[3]);

    private string jsonFolder;
    private int currentDataIndex;

    protected override void Awake()
    {
        base.Awake();
        jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
        ReadSaveData();
    }

    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }
  

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Save(currentDataIndex);
            Debug.Log("保存了");
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Load(currentDataIndex);
            Debug.Log("加载了");
        }
    }

    private void OnEndGameEvent()
    {
        Save(currentDataIndex);
    }

    private void OnStartNewGameEvent(int index)
    {
        currentDataIndex = index;
    }

    public void RegisterSavable(Isavable savable)
    {
        if(!savableList.Contains(savable))
        {
            savableList.Add(savable);
        }
    }

    private void ReadSaveData()
    {
        if(Directory.Exists(jsonFolder))
        {
            for(int i =0;i< LoadDataSlots.Count;i++)
            {
                var resultPath = jsonFolder + "data" + i + ".json";
                if(File.Exists(resultPath))
                {
                    //把文件里面的数据读出来
                    var stringData = File.ReadAllText(resultPath);
                    //反序列化
                    var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                    LoadDataSlots[i] = jsonData;

                }
            }        
        }
    }

    /// <summary>
    /// "芯片" dataSlot集中存放
    /// </summary>
    /// <param name="index"></param>
    private void Save(int index)
    {
        DataSlot dataSlot = new DataSlot();

        foreach(var savable in savableList)
        {
            dataSlot.dataDict.Add(savable.GUID, savable.GenerateSaveData());
        }
        LoadDataSlots[index] = dataSlot;

        var resultPath = jsonFolder + "data" + index + ".json";
        
        //生成json数据
        var jsonData = JsonConvert.SerializeObject(LoadDataSlots[index], Formatting.Indented);

        if(!File.Exists(resultPath))
        {
            Directory.CreateDirectory(jsonFolder);
        }
        //在目标文件夹写入json数据
        File.WriteAllText(resultPath, jsonData);
    }

    public void Load(int index)
    {
        currentDataIndex = index;

        var resultPath = jsonFolder + "data" + index + ".json";

        var stringData = File.ReadAllText(resultPath);

        var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);

        foreach(var savable in savableList)
        {
            savable.RestoreData(jsonData.dataDict[savable.GUID]);
        }
    }
}
