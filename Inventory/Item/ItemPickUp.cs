using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        private Collider2D collider2d;
        private void OnTriggerEnter2D(Collider2D other)
        {
            Item item = other.GetComponent<Item>();
           
            if(item != null)
            {
                if(item.itemDetails.canPickedup == true)
                {
                    //��ӽ�����
                    InventoryManager.Instance.AddItem(item,true);

                    //������Ч
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);

                }                
            }           
        }
    }
    
}
