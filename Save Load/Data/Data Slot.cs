using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.Transition;

namespace Mfarm.Save
{
    public class DataSlot
    {
        /// <summary>
        /// 进度条，String是各个Manager的GUID
        /// </summary>
        public Dictionary<string, GameSaveData> dataDict = new Dictionary<string, GameSaveData>();

        #region 用来UI显示进度详情
        public string DataTime
        {
            get
            {
                var key = TimeManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    GameSaveData timeData = dataDict[key];
                    return timeData.timeDict["gameMonth"] + "/" + timeData.timeDict["gameDay"] + "/" + timeData.timeDict["gameYear"] + "/"+(Season)timeData.timeDict["gameSeason"] ;
                    
                }
                else return string.Empty;
            }
        }
        public string DataScene
        {
            get 
            {
                var key = TransitionManager.Instance.GUID;

                if (dataDict.ContainsKey(key)) 
                {
                    GameSaveData transitionData = dataDict[key];
                    return transitionData.dataSceneName switch
                    {
                        "04_Start" => "SeaSide",
                        "01_Field" => "Farm",
                        "02_Home" => "Home",
                        "03_Stall" => "Stall",
                        _ => string.Empty,
                    };
                }
                else return string.Empty;
            }
        }
        #endregion
    }

}
