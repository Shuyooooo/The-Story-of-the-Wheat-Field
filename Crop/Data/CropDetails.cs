using UnityEngine;

[System.Serializable]
public class CropDetails
{
    public int seedItemID;

    [Header("��ͬ�׶���Ҫ������")]
    public int[] growthDays;

    /// <summary>
    /// �Զ���ȡ�ܵĳɳ���������
    /// </summary>
    public int TotalGrowthDays
    {
        get
        {
            int amount = 0;//��ʱ����
            foreach (var days in growthDays) 
            {
                amount += days;
            }
            return amount;
        }
    }

    [Header("��ͬ�����׶���ƷPrefab")]
    public GameObject[] growthPrefab;

    [Header("��ͬ�׶ε�ͼƬ")]
    public Sprite[] growthSprites;

    [Header("����ֲ�ļ���")]
    public Season[] season;

    [Space]
    [Header("�ո��")]
    public int[] harvestToolItemID;

    [Header("ÿ�ֹ���ʹ�ô���")]
    public int[] requireActionCount;

    [Header("ũ����״̬ת��ID")]
    public int CropTransferID;

    [Space]
    [Header("�ո��ʵ��Ϣ")]
    public int[] producedItemID;//��Ӧ�Ĺ�ʵ
    public int[] producedMinAmount;//������ɵ����ֵ
    public int[] producedMaxAmount;//������ɵ���Сֵ
    public Vector2 spawnRadius;//������ɵķ�Χ

    [Header("�ٴ�����ʱ��")]
    public int daysToRegrow;
    public int regrowTimes;

    [Header("Options")]
    public bool generateAtPlayerPosition;//�Ƿ��������������
    public bool hasAnimation;//�Ƿ��ж���
    public bool hasParticalEffect;//�Ƿ������Ӷ���

    
    public ParticleEffectType effectType;
    public Vector3 effectPos;
    public SoundName soundEffect;

    /// <summary>
    /// ��鵱ǰ�����Ƿ����
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public bool CheckToolAailable(int toolID)
    {
        //ѭ�����й��߿��Ƿ��п���
        foreach(var tool in harvestToolItemID) 
        {
            if(tool == toolID)
                return true;
        }
        return false;
    }

    public int GetTotalRequireCount(int toolID)
    {
        //��������ѭ�����������й���
        for(int i = 0; i < harvestToolItemID.Length;i++)
        {
            if (harvestToolItemID[i] == toolID)
                return requireActionCount[i];
        }
        return -1;
    }
}
