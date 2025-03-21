using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Singleton<NPCManager>
{
    
    public SceneRouteDataList_SO sceneRouteData;
    public List<NPCPosition> npcPositionList;

    private Dictionary<string,SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();


    protected override void Awake()
    {
        base.Awake();

        InitSceneRouteDict();
    }

    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        foreach (NPCPosition character in npcPositionList)
        {
            character.npc.position = character.position;
            character.npc.GetComponent<NPCMovement>().currentScene = character.startScene;
        }
    }

    private void InitSceneRouteDict()
    {
        //查找是否有SceneRoute类
        if(sceneRouteData.sceneRouteList.Count > 0)
        {
            foreach (SceneRoute route in sceneRouteData.sceneRouteList)
            {
                var key = route.fromSceneName + route.gotoSceneName;

                if (sceneRouteDict.ContainsKey(key))
                    continue;
                else
                    sceneRouteDict.Add(key, route);
                //Debug.Log(key + "-----" + route);
            }
        }
    }

    /// <summary>
    /// 获得两个场景间的路径
    /// </summary>
    /// <param name="fromSceneName"></param>
    /// <param name="gotoSceneName"></param>
    /// <returns></returns>
    public SceneRoute GetSceneRoute(string fromSceneName,string gotoSceneName)
    {
        return sceneRouteDict[fromSceneName + gotoSceneName];
    }
}
