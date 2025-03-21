using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

/// <summary>
/// 内含Light2D 及 SO文件
/// 用于操作数据
/// </summary>
public class LightController : MonoBehaviour
{
    public LightPatternList_SO lightData;

    private Light2D currentLightToOperate;
    private LightDetails currentLightDetails;

    private void Awake()
    {
        currentLightToOperate = GetComponent<Light2D>();
    }

    //实际切换灯光
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        //季节 + 昼夜  的匹配是唯一的
        currentLightDetails = lightData.GetLightDetails(season, lightShift);       

        if (timeDifference < Settings.LightChangeDuration)
        {
            //（需要达到的灯光值 1，需要操作的值 1-0.7 = 0.3） /  （已经操作了的部分） 时间差/总的duration
            var colorOffset = (currentLightDetails.lightColor - currentLightToOperate.color) / Settings.LightChangeDuration * timeDifference;
            currentLightToOperate.color += colorOffset;
            DOTween.To(() => currentLightToOperate.color, c => currentLightToOperate.color = c, currentLightDetails.lightColor, Settings.LightChangeDuration - timeDifference);
            DOTween.To(() => currentLightToOperate.intensity, i => currentLightToOperate.intensity = i, currentLightDetails.lightAmount, Settings.LightChangeDuration - timeDifference);            
        }

        if(timeDifference > Settings.LightChangeDuration)
        {
            currentLightToOperate.color = currentLightDetails.lightColor;
            currentLightToOperate.intensity = currentLightDetails.lightAmount;
        }

        
    }

}
