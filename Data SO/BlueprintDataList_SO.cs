using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlueprintDataList_SO" , menuName = "Inventory/BlueprintDataList_SO")]
public class BlueprintDataList_SO : ScriptableObject
{
    //蓝图清单
    public List<BluePrintDetails> blueprintDataList;

    public BluePrintDetails GetBlueprintDetails(int itemID)
    {
        return blueprintDataList.Find(b => b.ID == itemID);
    }
}

//一个 “蓝图” 类
[System.Serializable]
public class BluePrintDetails
{
    public int ID;
    public InventoryItem[] resourceItem = new InventoryItem[4];
    public GameObject buildPrefab;
}
