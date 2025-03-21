using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Mfarm.Inventory
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("�����ȡ")]
        [SerializeField] private Image slotimage;//���������ǰ��ȡ�ٶ����   
        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHighlight;
        [SerializeField] private Button button;
   



        [Header("��������")]
        public SlotType slotType;
        public bool isSelected;
        public int slotIndex;//��Inventory UI��start�����и�slotIndex��ֵ��
        //��Ʒ��Ϣ
        public ItemDetails itemdetails;
        public int itemAmount;

        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player,
                };
            }
        }


        //�õ����и��ӵĿ���Ȩ��slot+inventory
        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();


        private void Start()
        {
            isSelected = false;

            if (itemdetails == null)
                UpdateEmptySlot();
        }

        /// <summary>
        /// ��Slot����Ϊ��
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected )
            {
                isSelected = false;

                inventoryUI.UpdateSlotHighlight(-1);
                EventHandler.CallItemSelectedEvent(itemdetails, isSelected);
            }
            itemdetails = null;//��ָ��
            slotimage.enabled = false;
            amountText.text = "";
            button.interactable = false;
        }

        /// <summary>
        /// ���¸���UI����Ϣ
        /// </summary>
        /// <param name="item">ItemDetails</param>
        /// <param name="amount">��������</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemdetails = item;
            slotimage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();//����ת��
            slotimage.enabled = true;
            button.interactable = true;//���Ա��㰴           
        }

        /// <summary>
        /// ����ѡ��Ϊ������ʾ
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemdetails == null) return;
            isSelected = !isSelected;
            //Debug.Log(isSelected);

            inventoryUI.UpdateSlotHighlight(slotIndex);

            if(slotType == SlotType.Bag)
            {
                //֪ͨ��Ʒ��ѡ�е�״̬
                EventHandler.CallItemSelectedEvent(itemdetails,isSelected);
            }

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;
            //Debug.Log(eventData.pointerCurrentRaycast.gameObject);
            if (eventData.pointerCurrentRaycast.gameObject != null)
            //��ק�ͷź�������߼�⵽����Ʒ��Ϊ��
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();//�õ�Ŀ����slotUI
                int targetIndex = targetSlot.slotIndex;//�õ�Ŀ��slotUI��Index

                

                //��Player��������Χ�ڽ���
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                //����϶�����ı���������Bag  ��  Ŀ������ı���������Bag
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }

                //���˵���̵��϶�����������Ϊ��
                else if(slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)
                {
                    EventHandler.CallShowTradeUI(itemdetails, false);
                }

                //���˵���̵��϶�����������Ϊ��
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)
                {
                    EventHandler.CallShowTradeUI(itemdetails, true);
                }
                //����
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                {
                    //�米����������
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                }
                inventoryUI.UpdateSlotHighlight(-1);
            }
            //����ڵ����ɿ�ʱ�̵���������
            else
            {

                if (itemdetails.canDropped == true)
                {
                    var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,- Camera.main.transform.position.z));
                EventHandler.CallInstantiateItemInScene(itemdetails.itemID, pos);

                    
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemAmount != 0)//�����Ϊ��
            {
                inventoryUI.dragItem.enabled = true;//����϶�
                inventoryUI.dragItem.sprite = slotimage.sprite;//���ɿ���קsprite
                inventoryUI.dragItem.SetNativeSize();

                isSelected = true;
                inventoryUI.UpdateSlotHighlight(slotIndex);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }

        
    }

}
