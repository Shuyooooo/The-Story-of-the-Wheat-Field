using Mfarm.Inventory;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.CropPlant;

namespace Mfarm.Inventory//Inventory���뼯
{
    public class Item : MonoBehaviour
    {
        public int itemID;

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D coll;
        public ItemDetails itemDetails;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            coll = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);               
            }
        }

        //�߼�����һ�����õ���Ʒ��ID
        public void Init(int ID)
        {
            itemID = ID;

            //��Inventory�������ݣ�
            //Debug.Log(InventoryManager.Instance.GetItemDetails(itemID));
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            

            if (itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.itemIcon;
               

                //�޸���ײ��ĳߴ�
                Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
                coll.size = newSize;
                coll.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
            }

            if(itemDetails.itemType == ItemType.ReapableScenery)
            {
                gameObject.AddComponent<ReapItem>();
                
                //gameObject.GetComponent<ReapItem>().InitCropData(itemID);
                GetComponent<ReapItem>().InitCropData(itemID);
                gameObject.AddComponent<ItemInteractive>();
                



            }
        }      
    }
}
