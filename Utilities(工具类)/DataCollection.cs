using UnityEngine;
using System.Collections.Generic;

//把所有的类或是struct都放在这个文件中

[System.Serializable]
public class ItemDetails
{
    public int itemID;//物品的ID，便于搜索
    public string itemName;//物品的名字
    public ItemType itemType;

    public Sprite itemIcon;//物品图标
    public Sprite itemOnWorldSprite;//世界地图上显示出来的物品的图片

    public string itemDescription;//物品的描述
    public int itemUseRadius;//物品可以使用的范围

    //状态
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;
    
    //买卖情况
    public int itemPrice;
    [Range(0,1)]
    public float sellPercentage;
}

[System.Serializable]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
}

[System.Serializable]
public class AnimatorTypes
{
    public PartType partType;//举了啥
    public PartName partName;//手的状态
    public AnimatorOverrideController overridecontroller;
}

[System.Serializable]
public class SerializableVector3
{
    public float x,y, z;    

    public SerializableVector3(Vector3 pos)
        //用传进来的坐标给大类里面的x，y，z赋值
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
        //用类里面的x，y，z构造一个新的坐标
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

[System.Serializable]

public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[System.Serializable]
public class SceneFurniture
{
    //TODO 更多信息
    public int itemID;
    public SerializableVector3 position;
    public int boxIndex;
}

[System.Serializable]
public class TileProperty
{
    public Vector2Int tileCoordinate;
    public GridType gridType;
    public bool boolTypeValue;
}

[System.Serializable]
public class TileDetails
{
    public int gridX, gridY;

    public bool canDig;

    public bool canDropItem;

    public bool canPlacedFurniture;

    public bool isNPCObstacle;

    public int daysSinceDig = -1;

    public int daysSinceWatered = -1;

    public int seedItemID = -1;

    public int growthDays = -1;

    public int daysSinceLastHarvest = -1;
}

[System.Serializable]
public class NPCPosition
{
    public Transform npc;
    public string startScene;
    public Vector3 position;
}

[System.Serializable]

public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}

[System.Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int fromGridCell;
    public Vector2Int gotoGridCell;
}