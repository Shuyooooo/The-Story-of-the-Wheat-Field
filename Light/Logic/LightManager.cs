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
        currentSeason = season;//这里头的是没问题的        
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
    /// Controller拥有所有创建的各种形式的数据
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
        //加载场景以后去拿当前场景的灯光数据
        //有多个
        sceneLights = FindObjectsOfType<LightController>();

        foreach (var light in sceneLights)
        {
            //执行改变灯光的方法
            light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);            
        }
    }
}
