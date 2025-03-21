using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]//使用这个功能的时候，确保对象绑定了slotUI类
    public class ActionBarButton : MonoBehaviour
    {
        public KeyCode key;
        private SlotUI slotUI;
        private bool canUse = true;

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        private void OnEnable()
        {
            EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        }

        

        private void Update()
        {
            if(Input.GetKeyDown(key) && canUse)
            {
                if(slotUI.itemdetails != null)
                {
                    //如果选中了，就改成没选中，反之改成选中
                    slotUI.isSelected = !slotUI.isSelected ;
                    if (slotUI.isSelected)
                        //更新高亮
                        slotUI.inventoryUI.UpdateSlotHighlight(slotUI.slotIndex);
                    else
                        slotUI.inventoryUI.UpdateSlotHighlight(-1);

                    EventHandler.CallItemSelectedEvent(slotUI.itemdetails, slotUI.isSelected);
                    //Debug.Log(1);
                }
            }
        }

        private void OnUpdateGameStateEvent(GameState gameState)
        {
            canUse = gameState == GameState.Gameplay;
        }
    }
}
