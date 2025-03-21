using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mfarm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemToolTip itemtooltip;
        [Header("拖拽图片")]
        public Image dragItem;
        [Header("玩家背包UI")]
        [SerializeField] private GameObject bagUI;
        private bool bagOpenned;

        [Header("通用背包")]
        [SerializeField] private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;


        [SerializeField] private List<SlotUI> baseBagSlots;
        [SerializeField] private SlotUI[] playerSlots;//告诉数据包含的所有SLOTs


        [Header("交易UI")]
        public TradeUI tradeUI;
        public TextMeshProUGUI playerMoneyText; 

        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }

      

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

        

        private void Start()
        {
            for(int i =0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;//序列号初始化,每个slot都有序列号
            }
            bagOpenned = bagUI.activeInHierarchy;
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
            //Debug.Log(playerMoneyText.text);
        }

        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.B))
                OpenBagUI();
            /*if (Input.GetKeyUp(KeyCode.Escape))
                UpdateSlotHighlight(-1);*/
        }

        private void OnBeforeSceneUnloadEvent()
        {
            UpdateSlotHighlight(-1);
        }

        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            //这个事件最终在SlotUI中执行
            tradeUI.gameObject.SetActive(true);//拖动后，打开交易面板
            tradeUI.SetupTradeUI(item, isSell);//打开面板后，将this.itemDetails给到交易面板
            //然后由交易面板显示出来
        }


        /// <summary>
        /// 打开通用背包UI事件
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //TODO:通用箱子prefab
            GameObject prefab = slotType switch//把转换到的类型给prefab
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };
            //生成背包UI
            baseBag.SetActive(true);
            baseBagSlots = new List<SlotUI>();

            for(int i =0;i < bagData.itemList.Count;i++)
            {
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();                               
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            if(slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-0.75f, 0.5f);
                bagUI.SetActive(true);
                bagOpenned = true;
            }
            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box,bagData.itemList);
        }

        /// <summary>
        ///关闭通用背包UI事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sO"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //关闭UI
            //
            baseBag.SetActive(false);
            itemtooltip.gameObject.SetActive(false);
            UpdateSlotHighlight(-1);

            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();

            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bagUI.SetActive(false);
                bagOpenned = false;
            }
        }


        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch(location)
            {
                case InventoryLocation.Player:
                    for(int i = 0;i<playerSlots.Length;i++)
                        //背包里有多少个，就填多少个
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);//！
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);//数据库信息＋背包数据库的物品数量
                        }
                        else
                        {
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;

                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)                   
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);//！                            
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);//数据库信息＋背包数据库的物品数量
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
            }
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }

      
      
        /// <summary>
        /// 打开关闭背包UI,Button调用事件
        /// </summary>
        public void OpenBagUI()
        {
            bagOpenned = !bagOpenned;

            bagUI.SetActive(bagOpenned);
        }

        /// <summary>
        /// 更新高亮显示
        /// </summary>
        /// <param name="index"></param>
        public void UpdateSlotHighlight(int index)
        {
            foreach (var slot in playerSlots)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighlight.gameObject.SetActive(true);
                }

                else
                {
                    slot.isSelected = false;
                    slot.slotHighlight.gameObject.SetActive(false);
                }
            }
        }
    }
}

   

