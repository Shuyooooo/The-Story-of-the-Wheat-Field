using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.Inventory;

namespace Mfarm.CropPlant
{
    public class ReapItem : MonoBehaviour
    {
        //����ʲôʱ���õ�cropDetails
        private CropDetails cropDetails;
        private Transform PlayerTransform => FindObjectOfType<Player>().transform;

        public void InitCropData(int ID)
        {
            cropDetails = CropManager.Instance.GetCropDetails(ID);
        }
        public void SpawnHarvestItems()
        {
            //������Ӧ�Ĺ�ʵ����
            for (int i = 0; i < cropDetails.producedItemID.Length; i++)
            {
                int amountToProduce;
                if (cropDetails.producedMaxAmount[i] == cropDetails.producedMinAmount[i])
                {
                    //����ֻ����ָ��������
                    amountToProduce = cropDetails.producedMinAmount[i];
                }
                else
                {
                    amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                }

                //ִ�������ƶ���������Ʒ
                for (int j = 0; j < amountToProduce; j++)
                {
                    if (cropDetails.generateAtPlayerPosition)
                        EventHandler.CallHavestAtPlayerPosition(cropDetails.producedItemID[i]);
                    else//�����ͼ������
                    {

                        var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;

                        var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX)
                            , transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);

                        EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }
        }
    }
}
