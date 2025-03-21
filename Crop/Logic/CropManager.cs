using UnityEngine;

namespace Mfarm.CropPlant
{

    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;//创建数据库
        private Transform cropParent;
        private Grid currentGrid;
        private Season currentSeason;

        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            //切换场景时保存数据
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }

        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
        }

        private void OnAfterSceneLoadedEvent()
        {
            //切换场景时去拿到数据
            currentGrid = FindObjectOfType<Grid>();
            //因为种子都默认生成在cropParent下面，所以这一步相当于拿到所有
            //已经生成的种子的位置
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(ID);//找到种子信息
            if(currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1)//如果种子信息存在且在播种季节
            {
                //Step1--开始播种
                tileDetails.seedItemID = ID;
                tileDetails.growthDays = 0;
                //显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }
            //如果已经有种子了（比如说刷新时）
            else if (tileDetails.seedItemID != -1)
            {
                //显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }

        }

        /// <summary>
        /// 显示农作物
        /// </summary>
        /// <param name="tileDetails">瓦片信息</param>
        /// <param name="cropDetails">种子信息</param>
        private void DisplayCropPlant(TileDetails tileDetails,CropDetails cropDetails)
        {
            //成长阶段
            int growthStages = cropDetails.growthDays.Length;//(长度是手动给的)
            int currentStage = 0;
            int dayCounter = cropDetails.TotalGrowthDays;//TotalGrowthDays是一个propertiy方法

            //倒序计算当前的成长阶段
            for(int i = growthStages -1; i >= 0;i--)
            {
                //如果说已经记录的成长天数 > 总天数,代表已经成熟
                if(tileDetails.growthDays >= dayCounter)
                {
                    currentStage = i;
                    break;
                }
                dayCounter -= cropDetails.growthDays[i]; //daycount从第i个阶段开始计数
            }

            //获取当前阶段的Prefab
            GameObject cropPrefab = cropDetails.growthPrefab[currentStage];//拿到了种子         
            Sprite cropSprite = cropDetails.growthSprites[currentStage];//种子的图片            
           
            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f,0);//种子的位置         

            GameObject cropInstance = Instantiate(cropPrefab,pos,Quaternion.identity,cropParent);
            
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            cropInstance.GetComponent<crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<crop>().tileDetails = tileDetails;
           
        }

        /// <summary>
        /// 通过物品ID查找种子信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID)
        {
            //d的类型是cropDetails
            return cropData.cropDetailsList.Find(c => c.seedItemID == ID);
        }

        private bool SeasonAvailable(CropDetails crop)
        {
            for (int i = 0; i < crop.season.Length; i++)
            {
                if (crop.season[i] == currentSeason)
                    return true;
            }
            return false;
        }

    }

   
        
}

