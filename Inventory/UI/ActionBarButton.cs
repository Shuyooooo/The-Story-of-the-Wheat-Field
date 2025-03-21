using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]//ʹ��������ܵ�ʱ��ȷ���������slotUI��
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
                    //���ѡ���ˣ��͸ĳ�ûѡ�У���֮�ĳ�ѡ��
                    slotUI.isSelected = !slotUI.isSelected ;
                    if (slotUI.isSelected)
                        //���¸���
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
