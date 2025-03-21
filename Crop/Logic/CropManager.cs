using UnityEngine;

namespace Mfarm.CropPlant
{

    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;//�������ݿ�
        private Transform cropParent;
        private Grid currentGrid;
        private Season currentSeason;

        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            //�л�����ʱ��������
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
            //�л�����ʱȥ�õ�����
            currentGrid = FindObjectOfType<Grid>();
            //��Ϊ���Ӷ�Ĭ��������cropParent���棬������һ���൱���õ�����
            //�Ѿ����ɵ����ӵ�λ��
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(ID);//�ҵ�������Ϣ
            if(currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1)//���������Ϣ�������ڲ��ּ���
            {
                //Step1--��ʼ����
                tileDetails.seedItemID = ID;
                tileDetails.growthDays = 0;
                //��ʾũ����
                DisplayCropPlant(tileDetails, currentCrop);
            }
            //����Ѿ��������ˣ�����˵ˢ��ʱ��
            else if (tileDetails.seedItemID != -1)
            {
                //��ʾũ����
                DisplayCropPlant(tileDetails, currentCrop);
            }

        }

        /// <summary>
        /// ��ʾũ����
        /// </summary>
        /// <param name="tileDetails">��Ƭ��Ϣ</param>
        /// <param name="cropDetails">������Ϣ</param>
        private void DisplayCropPlant(TileDetails tileDetails,CropDetails cropDetails)
        {
            //�ɳ��׶�
            int growthStages = cropDetails.growthDays.Length;//(�������ֶ�����)
            int currentStage = 0;
            int dayCounter = cropDetails.TotalGrowthDays;//TotalGrowthDays��һ��propertiy����

            //������㵱ǰ�ĳɳ��׶�
            for(int i = growthStages -1; i >= 0;i--)
            {
                //���˵�Ѿ���¼�ĳɳ����� > ������,�����Ѿ�����
                if(tileDetails.growthDays >= dayCounter)
                {
                    currentStage = i;
                    break;
                }
                dayCounter -= cropDetails.growthDays[i]; //daycount�ӵ�i���׶ο�ʼ����
            }

            //��ȡ��ǰ�׶ε�Prefab
            GameObject cropPrefab = cropDetails.growthPrefab[currentStage];//�õ�������         
            Sprite cropSprite = cropDetails.growthSprites[currentStage];//���ӵ�ͼƬ            
           
            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f,0);//���ӵ�λ��         

            GameObject cropInstance = Instantiate(cropPrefab,pos,Quaternion.identity,cropParent);
            
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            cropInstance.GetComponent<crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<crop>().tileDetails = tileDetails;
           
        }

        /// <summary>
        /// ͨ����ƷID����������Ϣ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID)
        {
            //d��������cropDetails
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

