using Mfarm.CropPlant;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Mfarm.Save;

namespace Mfarm.map
{
    public class GridMapManager : Singleton<GridMapManager>,Isavable
    {
        [Header("Map Information")]

        public List<MapData_SO> mapDataList;//(坐标、类型、是否标记)_手动赋值 拖拽

        [Header("种地瓦片切换信息")]
        public RuleTile digTile;
        public RuleTile waterTile;

        private Tilemap digTilemap;
        private Tilemap waterTilemap;

        //场景名字+坐标和对应的瓦片信息
        private Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();

        //场景是否第一次加载
        private Dictionary<string, bool> firstLoadDict = new Dictionary<string, bool>();

        private Grid currentGrid;
        private Season currentSeason;

        private List<ReapItem> itemsInRadius;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefreshMap;
        }

      

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefreshMap;
        }

        private void Start()
        {
            Isavable savable = this;
            savable.RegisterSavable();

            foreach (var mapData in mapDataList)
            {
                //把每一个场景的名字加入firstLoadDict中
                firstLoadDict.Add(mapData.sceneName, true);
                InitTileDetailsDict(mapData);
            }
        }

        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

            //DisplayMap(SceneManager.GetActiveScene().name);
            if (firstLoadDict[SceneManager.GetActiveScene().name])
            {

                //预先生成农作物
                EventHandler.CallGenerateCropEvent();
                firstLoadDict[SceneManager.GetActiveScene().name] = false;               
            }
                RefreshMap();
        }


        

        /// <summary>
        /// 每 1 天调用一次
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;

            foreach (var tile in tileDetailsDict)
            {
                if (tile.Value.daysSinceWatered > -1)
                    tile.Value.daysSinceWatered  = -1;
                if(tile.Value.daysSinceDig > -1)
                    tile.Value.daysSinceDig ++;

                //超过5天自动消除坑 test
                if(tile.Value.daysSinceDig > 5 && tile.Value.seedItemID == -1 )
                {
                    tile.Value.daysSinceDig = -1;
                    tile.Value.canDig = true;//设置土地又能挖啦
                    tile.Value.growthDays = -1;//设置土地被挖天数归零
                }

                if(tile.Value.seedItemID != -1 ) 
                {
                    tile.Value.growthDays++;
                }
            }
            RefreshMap();
        }

        private void InitTileDetailsDict(MapData_SO mapData)
        {
            //使用mapData里面的值给 字典 赋值
            foreach(TileProperty tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y,
                };

                //字典的Key
                string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;//临时变量

                //通过返回值 拿到 TileDetails （空的）
                if(GetTileDetails(key) != null)
                {
                    tileDetails = GetTileDetails(key);
                }

                switch(tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlacedFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                }
                if (GetTileDetails(key) != null)
                    tileDetailsDict[key] = tileDetails;
                else
                    tileDetailsDict.Add(key, tileDetails);
            }
        }

        /// <summary>
        /// 根据key返回瓦片信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
           //如果说 当前瓦片的 string 和 detailslist 里面的string 匹配
            if(tileDetailsDict.ContainsKey(key))
            {
                //返回这个tileDetailsDict【key】——一个检索的步骤
                return tileDetailsDict[key];
            }
            return null;
        }

        /// <summary>
        /// 根据鼠标位置传递坐标返回对应的瓦片信息
        /// </summary>
        /// <param name="mouseGridPos"></param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {           
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;            
            return GetTileDetails(key);
        }

        /// <summary>
        /// 根据地块类型选择合适的功能动画
        /// </summary>
        /// <param name="mouseWorldPos">鼠标坐标</param>
        /// <param name="itemDetails">物品信息</param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            var mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

            if(currentTile != null)
            {
                crop currentCrop = GetCropObject(mouseWorldPos);
                //WORKFLOW
                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID, currentTile);
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos,itemDetails.itemType );
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);
                        break;
                    case ItemType.Commodity:
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);       
                        break;
                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        currentTile.daysSinceDig = 0;
                        currentTile.canDig = false;
                        currentTile.canDropItem = false;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        currentTile.daysSinceWatered = 0;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;
                    case ItemType.BreakTool:
                    case ItemType.ChopTool:
                        
                        currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        break;
                    case ItemType.CollectTool:
                        //crop currentCrop = GetCropObject(mouseWorldPos);
                        //执行收割方法
                        currentCrop.ProcessToolAction(itemDetails,currentTile);
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Basket);
                        break;                        
                    case ItemType.ReapTool:
                        var reapCount = 0;
                        //当此时的收割工具是reapTool，就把周围的杂草都过一遍（此时杂草已经在itemsInRadius这个List里面了）
                        for (int i = 0; i < itemsInRadius.Count; i++)
                        {                
                            //播放粒子特效
                            EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery, itemsInRadius[i].transform.position + Vector3.up);
                            //生成稻草
                            itemsInRadius[i].SpawnHarvestItems();
                            //销毁庄稼
                            Destroy(itemsInRadius[i].gameObject);
                            reapCount++;//查完一棵草+1

                            if(reapCount >= Settings.reapAmount)
                                break;
                        }
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);                       
                        break;

                    case ItemType.Furniture:
                        //在地图上生成物品 ItemManager
                        //移除当前物品（图纸） InventoryManager
                        //移除资源物品 InventoryManager 
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mouseWorldPos);
                        currentTile.canPlacedFurniture = false;
                        break;
                }
                UpdateTileDetails(currentTile);
            }
        }

        /// <summary>
        /// 通过物理方法判断鼠标点击位置的农作物
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <returns></returns>
        public crop GetCropObject(Vector2 mouseWorldPos)
        {
            //检测点击的世界坐标，返回一个范围内的 colliders[]
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);

            crop currentCrop = null;

            for (int i = 0; i <colliders.Length;i++)
            {
                if (colliders[i].GetComponent<crop>())
                    //如果周围有携带了crop脚本的collider
                    //则把crop值附给当前的 currentCrop
                    currentCrop = colliders[i].GetComponent<crop>();
            }
            return currentCrop;
        }

        public bool HavePeapableItemsInRadius(Vector3 mouseWorldPos,ItemDetails tool)
        {
            itemsInRadius = new List<ReapItem>();
            Collider2D[] colliders = new Collider2D[20];

            Physics2D.OverlapCircleNonAlloc(mouseWorldPos,tool.itemUseRadius, colliders);

            if(colliders.Length >- 0)//如果周围有东西
            {
                for(int i =0; i<colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        if(colliders[i].GetComponent<ReapItem>())
                        {
                            //检测到杂草，加入list
                            var item = colliders[i].GetComponent<ReapItem>();
                            itemsInRadius.Add(item);
                        }                        
                    }
                }
            }
            return itemsInRadius.Count > 0;
        }

        /// <summary>
        /// 显示挖坑瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (digTilemap != null)//如果说拿到的信息不为空
                digTilemap.SetTile(pos, digTile);//在那个地图的指定位置画出 tutile
        }

        /// <summary>
        /// 显示浇水瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (waterTilemap != null)
                waterTilemap.SetTile(pos, waterTile);
        }

        /// <summary>
        /// 更新地图信息
        /// </summary>
        /// <param name="tileDetails"></param>
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" +SceneManager.GetActiveScene().name;
            if(tileDetailsDict.ContainsKey(key))
            {
                tileDetailsDict[key] = tileDetails;
            }
            else
            {
                tileDetailsDict.Add(key, tileDetails);
            }
        }

        private void RefreshMap()
        {
            if (digTilemap != null)
                digTilemap.ClearAllTiles();//删除现有的digTilemap

            if (waterTilemap != null)
                waterTilemap.ClearAllTiles();

            foreach(var crop in FindObjectsOfType<crop>())
            {
                Destroy(crop.gameObject);
            }

            DisplayMap(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 更新地图信息
        /// </summary>
        /// <param name="sceneName"></param>
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in tileDetailsDict)//循环字典里的瓦片
            {
                var key = tile.Key;
                var tileDetails = tile.Value;

                if(key.Contains(sceneName))//如果说字典的 string 包含场景名
                {
                    if (tileDetails.daysSinceDig > -1)//如果 已经被挖过了
                        SetDigGround(tileDetails);//显示瓦片
                    if (tileDetails.daysSinceWatered > -1)//同上
                        SetWaterGround(tileDetails);
                    //种子
                    if(tileDetails.seedItemID > -1)//说明地里有种子
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID,tileDetails);
                }
            }
        }

        /// <summary>
        /// 根据场景名字构建网格范围，输出范围和原点
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="gridDimensions"></param>
        /// <param name="gridOrigin"></param>
        /// <returns></returns>
        public bool GetGridDimensions(string sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin)
        {
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            //循环地图里面的每一个mapdata
            foreach(var mapData in mapDataList)
            {
                if(mapData.sceneName == sceneName)//当脚下的场景 == 当前传入的场景
                {
                    gridDimensions.x = mapData.gridWidth;//宽度和长度已经有了，告诉网格节点就行了
                    gridDimensions.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;
                    return true;                   
                }
            }
            return false;
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = this.tileDetailsDict;
            saveData.firstLoadDict = this.firstLoadDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.tileDetailsDict = saveData.tileDetailsDict;
            this.firstLoadDict = saveData.firstLoadDict;
        }
    }



   
}
