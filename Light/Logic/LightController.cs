using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

/// <summary>
/// �ں�Light2D �� SO�ļ�
/// ���ڲ�������
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

    //ʵ���л��ƹ�
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        //���� + ��ҹ  ��ƥ����Ψһ��
        currentLightDetails = lightData.GetLightDetails(season, lightShift);       

        if (timeDifference < Settings.LightChangeDuration)
        {
            //����Ҫ�ﵽ�ĵƹ�ֵ 1����Ҫ������ֵ 1-0.7 = 0.3�� /  ���Ѿ������˵Ĳ��֣� ʱ���/�ܵ�duration
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
