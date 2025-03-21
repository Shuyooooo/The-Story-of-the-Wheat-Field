using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mfarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemTooltip : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        private SlotUI slotUI;
        private InventoryUI inventoryUI =>GetComponentInParent<InventoryUI>();

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotUI.itemdetails != null)
            {
                inventoryUI.itemtooltip.gameObject.SetActive(true);
                inventoryUI.itemtooltip.SetupTooltip(slotUI.itemdetails, slotUI.slotType);

                inventoryUI.itemtooltip.transform.position = transform.position + Vector3.up * 60;
                inventoryUI.itemtooltip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

                if(slotUI.itemdetails.itemType == ItemType.Furniture)
                {
                    inventoryUI.itemtooltip.resourcePanel.SetActive(true);
                    inventoryUI.itemtooltip.SetupResourcePanel(slotUI.itemdetails.itemID);
                }
                else
                {
                    inventoryUI.itemtooltip.resourcePanel.SetActive(false);
                }
             }
            
            else
            {
                inventoryUI.itemtooltip.gameObject.SetActive(false);
            }

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemtooltip.gameObject.SetActive(false);
        }

        
    }
}
