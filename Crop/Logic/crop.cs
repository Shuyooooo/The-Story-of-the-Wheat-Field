using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crop : MonoBehaviour
{
    public CropDetails cropDetails;
    private int harvestActionCount;
    public TileDetails tileDetails;//判断种子的重复收割
    private Animator anim;
    public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

    private Transform PlayerTransform => FindObjectOfType<Player>().transform;
    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        anim = GetComponentInChildren<Animator>();
        tileDetails = tile;
        //获取工具使用次数
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if (requireActionCount == -1) return;

       

        //点击计数器
        if (harvestActionCount < requireActionCount)//如果计数 < 要求的综述
        {
            harvestActionCount++;

            //判断是否有动画 树木摇晃
            if (anim != null && cropDetails.hasAnimation)
            {
                //如果玩家的x坐标更偏右
                if (PlayerTransform.position.x < transform.position.x)
                    anim.SetTrigger("Rotate Right");
                else
                    anim.SetTrigger("Rotate Left");               
            }
            //播放粒子
            if(cropDetails.hasParticalEffect)
                EventHandler.CallParticleEffectEvent(cropDetails.effectType,transform.position + cropDetails.effectPos );
            //播放声音
            if(cropDetails.soundEffect != SoundName.none)
            {
                var soundDetails = AudioManager.Instance.soundDetailsData.GetSoundDetails(cropDetails.soundEffect);
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }


        }

        if (harvestActionCount >= requireActionCount)//如果满足了总的计数 
        {
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation )//判定为true
            {
                //生成农作物
                SpawnHarvestItems();
            }
            else if(cropDetails.hasAnimation)
            {
                Debug.Log(PlayerTransform.position.x < transform.position.x);
                if (PlayerTransform.position.x < transform.position.x)//玩家的坐标是否小于当前对象的左边
                    anim.SetTrigger("FallingRight");//说明玩家在左边砍树，树应该向右边倒
                else
                    anim.SetTrigger("FallingLeft");

                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                //协程                
                StartCoroutine( HarvestAfterAnimation());
            }
        }
    }
 

    private IEnumerator HarvestAfterAnimation()
    {
        //在树倒下的这个过程中，其他的程序就不睡眠但是等一下动画放完
        //当倒下以后，就执行后面的程序
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Ending"))//当播放end动画时，执行后面的语句
        {
            yield return null;
        }

        SpawnHarvestItems();
        //转换其他阶段的农作物
        if(cropDetails.CropTransferID > 0)
        {
            CreateTransferCrop();
        }
    }

    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.CropTransferID;//把转换后需要生成的物品告诉地图信息
        tileDetails.daysSinceLastHarvest = -1;//现在不能收割了
        tileDetails.growthDays = 0;

        EventHandler.CallRefreshCurrentMap();
    }


    /// <summary>
    /// 生成农作物
    /// </summary>
    public void SpawnHarvestItems()
    {
        //遍历对应的果实数组
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce;
            if (cropDetails.producedMaxAmount[i] == cropDetails.producedMinAmount[i])
            {
                //代表只生成指定数量的
                amountToProduce = cropDetails.producedMinAmount[i];
            }
            else
            {
                amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
            }

            for (int j = 0; j < amountToProduce; j++)
            {
                if (cropDetails.generateAtPlayerPosition)
                    EventHandler.CallHavestAtPlayerPosition(cropDetails.producedItemID[i]);
                else//世界地图上生成
                {
                    
                    var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;

                    var spawnPos = new Vector3(transform.position.x +Random.Range(dirX, cropDetails.spawnRadius.x * dirX)
                        , transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y),0);

                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);

                }
            }            
        }
        if (tileDetails != null)
        {
            tileDetails.daysSinceLastHarvest++;

            if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes -1 )
            {
                //倒退日期
                //逻辑是把总的日期 - 可重复收割的日期 （过了几天就减去几天）
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;

                //刷新地图UI
                EventHandler.CallRefreshCurrentMap();

            }
            else
            {
                tileDetails.daysSinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
                //把坑也还原
                //tileDetails.daysSinceDig = -1;
            }

            Destroy(gameObject);
        }

    }
}

