using UnityEngine;

[System.Serializable]
public class CropDetails
{
    public int seedItemID;

    [Header("不同阶段需要的天数")]
    public int[] growthDays;

    /// <summary>
    /// 自动获取总的成长周期天数
    /// </summary>
    public int TotalGrowthDays
    {
        get
        {
            int amount = 0;//临时变量
            foreach (var days in growthDays) 
            {
                amount += days;
            }
            return amount;
        }
    }

    [Header("不同生长阶段物品Prefab")]
    public GameObject[] growthPrefab;

    [Header("不同阶段的图片")]
    public Sprite[] growthSprites;

    [Header("可种植的季节")]
    public Season[] season;

    [Space]
    [Header("收割工具")]
    public int[] harvestToolItemID;

    [Header("每种工具使用次数")]
    public int[] requireActionCount;

    [Header("农作物状态转换ID")]
    public int CropTransferID;

    [Space]
    [Header("收割果实信息")]
    public int[] producedItemID;//对应的果实
    public int[] producedMinAmount;//随机生成的最大值
    public int[] producedMaxAmount;//随机生成的最小值
    public Vector2 spawnRadius;//随机生成的范围

    [Header("再次生长时间")]
    public int daysToRegrow;
    public int regrowTimes;

    [Header("Options")]
    public bool generateAtPlayerPosition;//是否在玩家身上生成
    public bool hasAnimation;//是否有动画
    public bool hasParticalEffect;//是否有粒子动画

    
    public ParticleEffectType effectType;
    public Vector3 effectPos;
    public SoundName soundEffect;

    /// <summary>
    /// 检查当前工具是否可用
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public bool CheckToolAailable(int toolID)
    {
        //循环所有工具看是否有可用
        foreach(var tool in harvestToolItemID) 
        {
            if(tool == toolID)
                return true;
        }
        return false;
    }

    public int GetTotalRequireCount(int toolID)
    {
        //在数组内循环，遍历所有工具
        for(int i = 0; i < harvestToolItemID.Length;i++)
        {
            if (harvestToolItemID[i] == toolID)
                return requireActionCount[i];
        }
        return -1;
    }
}
