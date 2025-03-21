using Mfarm.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// ��ģ��Manager��
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
            Debug.Log("������");
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Load(currentDataIndex);
            Debug.Log("������");
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
                    //���ļ���������ݶ�����
                    var stringData = File.ReadAllText(resultPath);
                    //�����л�
                    var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                    LoadDataSlots[i] = jsonData;

                }
            }        
        }
    }

    /// <summary>
    /// "оƬ" dataSlot���д��
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
        
        //����json����
        var jsonData = JsonConvert.SerializeObject(LoadDataSlots[index], Formatting.Indented);

        if(!File.Exists(resultPath))
        {
            Directory.CreateDirectory(jsonFolder);
        }
        //��Ŀ���ļ���д��json����
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
