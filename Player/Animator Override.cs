using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.Inventory;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;

    public SpriteRenderer holdItem;


    [Header("各部分动画列表")]
    public List<AnimatorTypes> animatorTypes;

    private Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();

        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name, anim);//初始化字典
        }
    }


    //呼叫事件
    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }
    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }

    private void OnHarvestAtPlayerPosition(int ID)
    {
        //通过传入的ID找到当前物品，
        //把当前物品的inventorymanager里面的WorldSprite给到
        //即将在玩家头上生成的sprite
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        if(holdItem.enabled == false)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(1f);
        holdItem.enabled = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        holdItem.enabled = false;//把举着的东西的sprite显示关闭
        SwitchAnimator(PartType.None);//把动画里手放下来（None）
    }

    

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        //WORKFLOW:不同的工具返回不同的动画在这里补全
        PartType currentType = itemDetails.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.Furniture => PartType.Carry,
            ItemType.CollectTool => PartType.Collect,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            ItemType.ChopTool => PartType.Chop,
            _ => PartType.None
        };

        if(isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {            
            if(currentType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite;//世界地图上的sprite
                holdItem.enabled = true;//显示sprite可见
            }
            else
            {
                holdItem.enabled = false;
            }
            
        }
        SwitchAnimator(currentType);
        


    }
    //现在工作的controlleroverride是在player中挂载的代码给出的
    //AnimatorType给出的，是需要自主赋值的——拖进去。确认点选以后
    //如果是种子就进行一个ItemType——PartType的转换，然后把新的PartType值给
    //currentType，带入切换动画函数。一个一个子动画顺着找，如果现在的类型
    //在player里面找到了，比如是carry或是tool，就把animatoroverride里面
    //的controller（我给的）告诉字典里面正在工作的动画。

    private void SwitchAnimator(PartType partType)
    {
        foreach(var item in animatorTypes)
        {
            if(item.partType == partType)
            {
                //Debug.Log(item.overridecontroller.name);
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overridecontroller;
            }
        }
    }
}
