using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.Inventory;
using UnityEngine.SearchService;

public class Box : MonoBehaviour
{
    public InventoryBag_SO boxBagTemplate;
    public InventoryBag_SO boxBagData;

    public GameObject mouseIcon;
    private bool canOpen = false;
    private bool isOpen;

    public int index;

    private void OnEnable()
    {
        if(boxBagData == null)//为空说明这个箱子没有放过东西
        {
            //更新数据
            boxBagData = Instantiate(boxBagTemplate);
        }

       
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = true;
            mouseIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = false;
            mouseIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if(!isOpen && canOpen && Input.GetMouseButtonDown(1))
        {
            //打开箱子
            EventHandler.CallBaseBagOpenEvent(SlotType.Box, boxBagData);
            isOpen = true; 
        }

        if(!canOpen && isOpen)
        {
            EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
            isOpen = false;
        }
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
            isOpen = false;
        }
    }

    public void InitBox(int boxIndex)
    {
        index = boxIndex;
        var key = this.name + index;
        if (InventoryManager.Instance.GetBoxDataList(key) != null)
        {
            //更新一下
            boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
        }
        else
        {           
            InventoryManager.Instance.AddBoxDataDict(this);
        }
    }
}
