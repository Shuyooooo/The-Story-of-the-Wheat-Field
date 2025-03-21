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

        public List<MapData_SO> mapDataList;//(���ꡢ���͡��Ƿ���)_�ֶ���ֵ ��ק

        [Header("�ֵ���Ƭ�л���Ϣ")]
        public RuleTile digTile;
        public RuleTile waterTile;

        private Tilemap digTilemap;
        private Tilemap waterTilemap;

        //��������+����Ͷ�Ӧ����Ƭ��Ϣ
        private Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();

        //�����Ƿ��һ�μ���
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
                //��ÿһ�����������ּ���firstLoadDict��
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

                //Ԥ������ũ����
                EventHandler.CallGenerateCropEvent();
                firstLoadDict[SceneManager.GetActiveScene().name] = false;               
            }
                RefreshMap();
        }


        

        /// <summary>
        /// ÿ 1 �����һ��
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

                //����5���Զ������� test
                if(tile.Value.daysSinceDig > 5 && tile.Value.seedItemID == -1 )
                {
                    tile.Value.daysSinceDig = -1;
                    tile.Value.canDig = true;//����������������
                    tile.Value.growthDays = -1;//�������ر�����������
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
            //ʹ��mapData�����ֵ�� �ֵ� ��ֵ
            foreach(TileProperty tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y,
                };

                //�ֵ��Key
                string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;//��ʱ����

                //ͨ������ֵ �õ� TileDetails ���յģ�
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
        /// ����key������Ƭ��Ϣ
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
           //���˵ ��ǰ��Ƭ�� string �� detailslist �����string ƥ��
            if(tileDetailsDict.ContainsKey(key))
            {
                //�������tileDetailsDict��key������һ�������Ĳ���
                return tileDetailsDict[key];
            }
            return null;
        }

        /// <summary>
        /// �������λ�ô������귵�ض�Ӧ����Ƭ��Ϣ
        /// </summary>
        /// <param name="mouseGridPos"></param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {           
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;            
            return GetTileDetails(key);
        }

        /// <summary>
        /// ���ݵؿ�����ѡ����ʵĹ��ܶ���
        /// </summary>
        /// <param name="mouseWorldPos">�������</param>
        /// <param name="itemDetails">��Ʒ��Ϣ</param>
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
                        //��Ч
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        currentTile.daysSinceWatered = 0;
                        //��Ч
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;
                    case ItemType.BreakTool:
                    case ItemType.ChopTool:
                        
                        currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        break;
                    case ItemType.CollectTool:
                        //crop currentCrop = GetCropObject(mouseWorldPos);
                        //ִ���ո��
                        currentCrop.ProcessToolAction(itemDetails,currentTile);
                        //��Ч
                        EventHandler.CallPlaySoundEvent(SoundName.Basket);
                        break;                        
                    case ItemType.ReapTool:
                        var reapCount = 0;
                        //����ʱ���ո����reapTool���Ͱ���Χ���Ӳݶ���һ�飨��ʱ�Ӳ��Ѿ���itemsInRadius���List�����ˣ�
                        for (int i = 0; i < itemsInRadius.Count; i++)
                        {                
                            //����������Ч
                            EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery, itemsInRadius[i].transform.position + Vector3.up);
                            //���ɵ���
                            itemsInRadius[i].SpawnHarvestItems();
                            //����ׯ��
                            Destroy(itemsInRadius[i].gameObject);
                            reapCount++;//����һ�ò�+1

                            if(reapCount >= Settings.reapAmount)
                                break;
                        }
                        //��Ч
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);                       
                        break;

                    case ItemType.Furniture:
                        //�ڵ�ͼ��������Ʒ ItemManager
                        //�Ƴ���ǰ��Ʒ��ͼֽ�� InventoryManager
                        //�Ƴ���Դ��Ʒ InventoryManager 
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mouseWorldPos);
                        currentTile.canPlacedFurniture = false;
                        break;
                }
                UpdateTileDetails(currentTile);
            }
        }

        /// <summary>
        /// ͨ���������ж������λ�õ�ũ����
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <returns></returns>
        public crop GetCropObject(Vector2 mouseWorldPos)
        {
            //��������������꣬����һ����Χ�ڵ� colliders[]
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);

            crop currentCrop = null;

            for (int i = 0; i <colliders.Length;i++)
            {
                if (colliders[i].GetComponent<crop>())
                    //�����Χ��Я����crop�ű���collider
                    //���cropֵ������ǰ�� currentCrop
                    currentCrop = colliders[i].GetComponent<crop>();
            }
            return currentCrop;
        }

        public bool HavePeapableItemsInRadius(Vector3 mouseWorldPos,ItemDetails tool)
        {
            itemsInRadius = new List<ReapItem>();
            Collider2D[] colliders = new Collider2D[20];

            Physics2D.OverlapCircleNonAlloc(mouseWorldPos,tool.itemUseRadius, colliders);

            if(colliders.Length >- 0)//�����Χ�ж���
            {
                for(int i =0; i<colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        if(colliders[i].GetComponent<ReapItem>())
                        {
                            //��⵽�Ӳݣ�����list
                            var item = colliders[i].GetComponent<ReapItem>();
                            itemsInRadius.Add(item);
                        }                        
                    }
                }
            }
            return itemsInRadius.Count > 0;
        }

        /// <summary>
        /// ��ʾ�ڿ���Ƭ
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (digTilemap != null)//���˵�õ�����Ϣ��Ϊ��
                digTilemap.SetTile(pos, digTile);//���Ǹ���ͼ��ָ��λ�û��� tutile
        }

        /// <summary>
        /// ��ʾ��ˮ��Ƭ
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (waterTilemap != null)
                waterTilemap.SetTile(pos, waterTile);
        }

        /// <summary>
        /// ���µ�ͼ��Ϣ
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
                digTilemap.ClearAllTiles();//ɾ�����е�digTilemap

            if (waterTilemap != null)
                waterTilemap.ClearAllTiles();

            foreach(var crop in FindObjectsOfType<crop>())
            {
                Destroy(crop.gameObject);
            }

            DisplayMap(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// ���µ�ͼ��Ϣ
        /// </summary>
        /// <param name="sceneName"></param>
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in tileDetailsDict)//ѭ���ֵ������Ƭ
            {
                var key = tile.Key;
                var tileDetails = tile.Value;

                if(key.Contains(sceneName))//���˵�ֵ�� string ����������
                {
                    if (tileDetails.daysSinceDig > -1)//��� �Ѿ����ڹ���
                        SetDigGround(tileDetails);//��ʾ��Ƭ
                    if (tileDetails.daysSinceWatered > -1)//ͬ��
                        SetWaterGround(tileDetails);
                    //����
                    if(tileDetails.seedItemID > -1)//˵������������
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID,tileDetails);
                }
            }
        }

        /// <summary>
        /// ���ݳ������ֹ�������Χ�������Χ��ԭ��
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="gridDimensions"></param>
        /// <param name="gridOrigin"></param>
        /// <returns></returns>
        public bool GetGridDimensions(string sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin)
        {
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            //ѭ����ͼ�����ÿһ��mapdata
            foreach(var mapData in mapDataList)
            {
                if(mapData.sceneName == sceneName)//�����µĳ��� == ��ǰ����ĳ���
                {
                    gridDimensions.x = mapData.gridWidth;//��Ⱥͳ����Ѿ����ˣ���������ڵ������
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
