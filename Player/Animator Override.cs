using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.Inventory;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;

    public SpriteRenderer holdItem;


    [Header("�����ֶ����б�")]
    public List<AnimatorTypes> animatorTypes;

    private Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();

        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name, anim);//��ʼ���ֵ�
        }
    }


    //�����¼�
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
        //ͨ�������ID�ҵ���ǰ��Ʒ��
        //�ѵ�ǰ��Ʒ��inventorymanager�����WorldSprite����
        //���������ͷ�����ɵ�sprite
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
        holdItem.enabled = false;//�Ѿ��ŵĶ�����sprite��ʾ�ر�
        SwitchAnimator(PartType.None);//�Ѷ������ַ�������None��
    }

    

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        //WORKFLOW:��ͬ�Ĺ��߷��ز�ͬ�Ķ��������ﲹȫ
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
                holdItem.sprite = itemDetails.itemOnWorldSprite;//�����ͼ�ϵ�sprite
                holdItem.enabled = true;//��ʾsprite�ɼ�
            }
            else
            {
                holdItem.enabled = false;
            }
            
        }
        SwitchAnimator(currentType);
        


    }
    //���ڹ�����controlleroverride����player�й��صĴ��������
    //AnimatorType�����ģ�����Ҫ������ֵ�ġ����Ͻ�ȥ��ȷ�ϵ�ѡ�Ժ�
    //��������Ӿͽ���һ��ItemType����PartType��ת����Ȼ����µ�PartTypeֵ��
    //currentType�������л�����������һ��һ���Ӷ���˳���ң�������ڵ�����
    //��player�����ҵ��ˣ�������carry����tool���Ͱ�animatoroverride����
    //��controller���Ҹ��ģ������ֵ��������ڹ����Ķ�����

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
