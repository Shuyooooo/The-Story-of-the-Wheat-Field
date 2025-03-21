using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class LightManager : MonoBehaviour
{   
    private LightController[] sceneLights;
    private LightShift currentLightShift;
    private Season currentSeason;
    private float timeDifference = Settings.LightChangeDuration;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;       
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent  += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        currentLightShift = LightShift.Morning;       
    }

    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        currentSeason = season;//����ͷ����û�����        
        this.timeDifference = timeDifference;
        if (currentLightShift != lightShift)
        {
            currentLightShift = lightShift;

            foreach(var light in sceneLights)
            {
                light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);              
            }
        }
    }

    /// <summary>
    /// Controllerӵ�����д����ĸ�����ʽ������
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
        //���س����Ժ�ȥ�õ�ǰ�����ĵƹ�����
        //�ж��
        sceneLights = FindObjectsOfType<LightController>();

        foreach (var light in sceneLights)
        {
            //ִ�иı�ƹ�ķ���
            light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);            
        }
    }
}
