using UnityEngine;
using System.Collections.Generic;

//�����е������struct����������ļ���

[System.Serializable]
public class ItemDetails
{
    public int itemID;//��Ʒ��ID����������
    public string itemName;//��Ʒ������
    public ItemType itemType;

    public Sprite itemIcon;//��Ʒͼ��
    public Sprite itemOnWorldSprite;//�����ͼ����ʾ��������Ʒ��ͼƬ

    public string itemDescription;//��Ʒ������
    public int itemUseRadius;//��Ʒ����ʹ�õķ�Χ

    //״̬
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;
    
    //�������
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
    public PartType partType;//����ɶ
    public PartName partName;//�ֵ�״̬
    public AnimatorOverrideController overridecontroller;
}

[System.Serializable]
public class SerializableVector3
{
    public float x,y, z;    

    public SerializableVector3(Vector3 pos)
        //�ô���������������������x��y��z��ֵ
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
        //���������x��y��z����һ���µ�����
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
    //TODO ������Ϣ
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