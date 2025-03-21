using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mfarm.Inventory;

public class ItemToolTip : MonoBehaviour
{
    //要使变量能够被识别
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Text valueText;
    [SerializeField] private GameObject bottomPart;

    [Header("建造")]
    public GameObject resourcePanel;
    [SerializeField] private Image[] resourceItem;

    public void SetupTooltip(ItemDetails itemDetails,SlotType slotType)
    {
        nameText.text = itemDetails.itemName;

        typeText.text = itemDetails.itemType.ToString();

        descriptionText.text = itemDetails.itemDescription;

        if(itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture)
        {
            bottomPart.SetActive(true);//售价部分显示激活

            var price = itemDetails.itemPrice;
            if (slotType == SlotType.Bag)//如果是在背包中显示
            {
                price = (int)(price * itemDetails.sellPercentage);//售价要打折
            }

            valueText.text = price.ToString();
        }
        else
        {
            bottomPart.SetActive(false);
        }
        //强制重新绘制这个GameObject
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    
    public void SetupResourcePanel(int ID)
    {
        var BlueprintDetails = InventoryManager.Instance.bluePrintData.GetBlueprintDetails(ID);
        //最多只给了3个UI限定，所以循环 资源数组——3次
        for(int i = 0; i < resourceItem.Length;i++)
        {
            //如果蓝图只有两个所需资源，则第三个就可以过滤了
            if(i < BlueprintDetails.resourceItem.Length)
            {
                //需要Icon，需要Amount               
                var item = BlueprintDetails.resourceItem[i]; 
                resourceItem[i].gameObject.SetActive(true);//显示图片
                resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
                resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
            }
            else
            {
                //把多出来的资源关闭
                resourceItem[i].gameObject.SetActive(false);
            }
        }
    }
}

