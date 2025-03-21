using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Mfarm.Inventory;


public class TradeUI : MonoBehaviour
{
    public Image itemIcon;//��Ʒͼ��
    public Text itemName;//��Ʒ����
    public InputField tradeAmount;//������
    public Button submitButton;//ȷ��/ȡ��
    public Button cancelButton;

    private ItemDetails item;//��ʱ
    private bool isSellTrade;

    private void Awake()
    {
        cancelButton.onClick.AddListener(CancelTrade);//��ӵ���¼�������
        submitButton.onClick.AddListener(TradeItem);
    }

    /// <summary>
    /// ����TradeUI��ʾ����
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isSell"></param>
    public void SetupTradeUI(ItemDetails item,bool isSell)
    {
        this.item = item;
        itemIcon.sprite = item.itemIcon;
        itemName.text = item.itemName;
        isSellTrade = isSell;
        tradeAmount.text = string.Empty;//Ĭ�����һ��
    }

    private void CancelTrade()
    {
        this.gameObject.SetActive(false);//����������˾Ͱ�������ڹ���
    }

    private void TradeItem()
    {
        //ִ�����ݽ����ķ���,��鷽���еļ�������
        var amount = Convert.ToInt32(tradeAmount.text);
        InventoryManager.Instance.TradeItem(item, amount, isSellTrade);

        CancelTrade();

    }
}
