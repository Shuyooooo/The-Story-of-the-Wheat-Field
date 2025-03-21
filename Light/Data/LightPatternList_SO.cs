using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightPatternList_SO", menuName = "Light/Light Pattern")]
public class LightPatternList_SO : ScriptableObject
{
    public List<LightDetails> lightPatternList;

    public LightDetails GetLightDetails(Season season,LightShift lightShift)
    {
        return lightPatternList.Find(L => L.season == season && L.lightShitf == lightShift);
    }
}

/// <summary>
/// 灯光数据详情
/// </summary>
[System.Serializable]
public class LightDetails
{
    public Season season;
    public LightShift lightShitf;
    public Color lightColor;
    public float lightAmount;
}
