using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.map;

namespace Mfarm.CropPlant
{
    public class CropGenerator : MonoBehaviour
    {
        private Grid currentGrid;

        public int seedItemID;
        public int growthDay;//�ж���ľ��ǰ�ɳ�����һ���׶�

        private void OnEnable()
        {
            EventHandler.GenerateCropEvent += GenerateCrop;
        }

        private void OnDisable()
        {
            EventHandler.GenerateCropEvent -= GenerateCrop;
        }



        private void Awake()
        {
            currentGrid = FindObjectOfType<Grid>();//�������һ��GameObject���������������Grid
            GenerateCrop();
        }
        private void GenerateCrop()
        {   
            //����������С������������������������ҪVector2Int!
            Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);

            if(seedItemID != 0) 
            {
                //ͨ��Vector3�ҵ�tileDetails
                var tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);

                if(tile == null)
                {
                    tile = new TileDetails();
                    tile.gridX = cropGridPos.x;
                    tile.gridY = cropGridPos.y;
                }
                tile.daysSinceWatered = -1;
                tile.seedItemID = seedItemID;
                tile.growthDays = growthDay;

                GridMapManager.Instance.UpdateTileDetails(tile);
            }
        }
    }

   
}
