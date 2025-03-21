using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Mfarm.Inventory
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("组件获取")]
        [SerializeField] private Image slotimage;//在组件中提前获取速度最快   
        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHighlight;
        [SerializeField] private Button button;
   



        [Header("格子类型")]
        public SlotType slotType;
        public bool isSelected;
        public int slotIndex;//在Inventory UI的start函数中给slotIndex赋值了
        //物品信息
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


        //拿到所有格子的控制权，slot+inventory
        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();


        private void Start()
        {
            isSelected = false;

            if (itemdetails == null)
                UpdateEmptySlot();
        }

        /// <summary>
        /// 将Slot更新为空
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected )
            {
                isSelected = false;

                inventoryUI.UpdateSlotHighlight(-1);
                EventHandler.CallItemSelectedEvent(itemdetails, isSelected);
            }
            itemdetails = null;//空指针
            slotimage.enabled = false;
            amountText.text = "";
            button.interactable = false;
        }

        /// <summary>
        /// 更新格子UI和信息
        /// </summary>
        /// <param name="item">ItemDetails</param>
        /// <param name="amount">持有数量</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemdetails = item;
            slotimage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();//类型转换
            slotimage.enabled = true;
            button.interactable = true;//可以被点按           
        }

        /// <summary>
        /// 设置选中为高亮显示
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
                //通知物品被选中的状态
                EventHandler.CallItemSelectedEvent(itemdetails,isSelected);
            }

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;
            //Debug.Log(eventData.pointerCurrentRaycast.gameObject);
            if (eventData.pointerCurrentRaycast.gameObject != null)
            //拖拽释放后，鼠标射线检测到的物品不为空
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();//拿到目标点的slotUI
                int targetIndex = targetSlot.slotIndex;//拿到目标slotUI的Index

                

                //在Player自身背包范围内交换
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                //如果拖动物体的背包类型是Bag  且  目标物体的背包类型是Bag
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }

                //如果说从商店拖动到背包，则为买
                else if(slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)
                {
                    EventHandler.CallShowTradeUI(itemdetails, false);
                }

                //如果说从商店拖动到背包，则为卖
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)
                {
                    EventHandler.CallShowTradeUI(itemdetails, true);
                }
                //储存
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                {
                    //跨背包交换数据
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                }
                inventoryUI.UpdateSlotHighlight(-1);
            }
            //鼠标在地面松开时刻的世界坐标
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
            if (itemAmount != 0)//如果不为空
            {
                inventoryUI.dragItem.enabled = true;//则可拖动
                inventoryUI.dragItem.sprite = slotimage.sprite;//生成可拖拽sprite
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
