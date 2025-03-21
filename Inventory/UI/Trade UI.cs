using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Mfarm.Inventory;


public class TradeUI : MonoBehaviour
{
    public Image itemIcon;//商品图标
    public Text itemName;//商品名称
    public InputField tradeAmount;//交易区
    public Button submitButton;//确认/取消
    public Button cancelButton;

    private ItemDetails item;//临时
    private bool isSellTrade;

    private void Awake()
    {
        cancelButton.onClick.AddListener(CancelTrade);//添加点击事件监听器
        submitButton.onClick.AddListener(TradeItem);
    }

    /// <summary>
    /// 设置TradeUI显示详情
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isSell"></param>
    public void SetupTradeUI(ItemDetails item,bool isSell)
    {
        this.item = item;
        itemIcon.sprite = item.itemIcon;
        itemName.text = item.itemName;
        isSellTrade = isSell;
        tradeAmount.text = string.Empty;//默认清空一下
    }

    private void CancelTrade()
    {
        this.gameObject.SetActive(false);//如果不想卖了就把这个窗口关了
    }

    private void TradeItem()
    {
        //执行数据交互的方法,检查方法中的几个变量
        var amount = Convert.ToInt32(tradeAmount.text);
        InventoryManager.Instance.TradeItem(item, amount, isSellTrade);

        CancelTrade();

    }
}
