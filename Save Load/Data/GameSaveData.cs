using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.Save 
{
    [System.Serializable]
    public class GameSaveData
    {
        public string dataSceneName;

        /// <summary>
        /// 存储人物坐标，String人物名字
        /// </summary>
        public Dictionary<string, SerializableVector3> characterPosDict;

        public Dictionary<string, List<SceneItem>> sceneItemDict;
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict;

        public Dictionary<string, TileDetails> tileDetailsDict;
        public Dictionary<string, bool> firstLoadDict;

        public Dictionary <string, List<InventoryItem>> inventoryDict;
        public Dictionary<string, int> timeDict;
        public int PlayerMoney;

        //NPC
        public string targetScene;
        public bool interactable;
        public int animationInstanceID;
    }
 
}
