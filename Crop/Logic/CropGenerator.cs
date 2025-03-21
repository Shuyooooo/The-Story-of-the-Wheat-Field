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
        public int growthDay;//判断树木当前成长到哪一个阶段

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
            currentGrid = FindObjectOfType<Grid>();//如果它是一个GameObject就在这个类里面找Grid
            GenerateCrop();
        }
        private void GenerateCrop()
        {   
            //世界坐标是小数但网格坐标是整数，所以要Vector2Int!
            Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);

            if(seedItemID != 0) 
            {
                //通过Vector3找到tileDetails
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
