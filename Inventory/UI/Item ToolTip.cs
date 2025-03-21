using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mfarm.Inventory;

public class ItemToolTip : MonoBehaviour
{
    //Ҫʹ�����ܹ���ʶ��
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Text valueText;
    [SerializeField] private GameObject bottomPart;

    [Header("����")]
    public GameObject resourcePanel;
    [SerializeField] private Image[] resourceItem;

    public void SetupTooltip(ItemDetails itemDetails,SlotType slotType)
    {
        nameText.text = itemDetails.itemName;

        typeText.text = itemDetails.itemType.ToString();

        descriptionText.text = itemDetails.itemDescription;

        if(itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture)
        {
            bottomPart.SetActive(true);//�ۼ۲�����ʾ����

            var price = itemDetails.itemPrice;
            if (slotType == SlotType.Bag)//������ڱ�������ʾ
            {
                price = (int)(price * itemDetails.sellPercentage);//�ۼ�Ҫ����
            }

            valueText.text = price.ToString();
        }
        else
        {
            bottomPart.SetActive(false);
        }
        //ǿ�����»������GameObject
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    
    public void SetupResourcePanel(int ID)
    {
        var BlueprintDetails = InventoryManager.Instance.bluePrintData.GetBlueprintDetails(ID);
        //���ֻ����3��UI�޶�������ѭ�� ��Դ���顪��3��
        for(int i = 0; i < resourceItem.Length;i++)
        {
            //�����ͼֻ������������Դ����������Ϳ��Թ�����
            if(i < BlueprintDetails.resourceItem.Length)
            {
                //��ҪIcon����ҪAmount               
                var item = BlueprintDetails.resourceItem[i]; 
                resourceItem[i].gameObject.SetActive(true);//��ʾͼƬ
                resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
                resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
            }
            else
            {
                //�Ѷ��������Դ�ر�
                resourceItem[i].gameObject.SetActive(false);
            }
        }
    }
}

