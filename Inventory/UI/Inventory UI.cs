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
        [Header("��קͼƬ")]
        public Image dragItem;
        [Header("��ұ���UI")]
        [SerializeField] private GameObject bagUI;
        private bool bagOpenned;

        [Header("ͨ�ñ���")]
        [SerializeField] private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;


        [SerializeField] private List<SlotUI> baseBagSlots;
        [SerializeField] private SlotUI[] playerSlots;//�������ݰ���������SLOTs


        [Header("����UI")]
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
                playerSlots[i].slotIndex = i;//���кų�ʼ��,ÿ��slot�������к�
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
            //����¼�������SlotUI��ִ��
            tradeUI.gameObject.SetActive(true);//�϶��󣬴򿪽������
            tradeUI.SetupTradeUI(item, isSell);//�����󣬽�this.itemDetails�����������
            //Ȼ���ɽ��������ʾ����
        }


        /// <summary>
        /// ��ͨ�ñ���UI�¼�
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //TODO:ͨ������prefab
            GameObject prefab = slotType switch//��ת���������͸�prefab
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };
            //���ɱ���UI
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
            //����UI��ʾ
            OnUpdateInventoryUI(InventoryLocation.Box,bagData.itemList);
        }

        /// <summary>
        ///�ر�ͨ�ñ���UI�¼�
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sO"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //�ر�UI
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
                        //�������ж��ٸ���������ٸ�
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);//��
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);//���ݿ���Ϣ���������ݿ����Ʒ����
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
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);//��                            
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);//���ݿ���Ϣ���������ݿ����Ʒ����
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
        /// �򿪹رձ���UI,Button�����¼�
        /// </summary>
        public void OpenBagUI()
        {
            bagOpenned = !bagOpenned;

            bagUI.SetActive(bagOpenned);
        }

        /// <summary>
        /// ���¸�����ʾ
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

   

