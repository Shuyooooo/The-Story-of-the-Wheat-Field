using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crop : MonoBehaviour
{
    public CropDetails cropDetails;
    private int harvestActionCount;
    public TileDetails tileDetails;//�ж����ӵ��ظ��ո�
    private Animator anim;
    public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

    private Transform PlayerTransform => FindObjectOfType<Player>().transform;
    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        anim = GetComponentInChildren<Animator>();
        tileDetails = tile;
        //��ȡ����ʹ�ô���
        int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
        if (requireActionCount == -1) return;

       

        //���������
        if (harvestActionCount < requireActionCount)//������� < Ҫ�������
        {
            harvestActionCount++;

            //�ж��Ƿ��ж��� ��ľҡ��
            if (anim != null && cropDetails.hasAnimation)
            {
                //�����ҵ�x�����ƫ��
                if (PlayerTransform.position.x < transform.position.x)
                    anim.SetTrigger("Rotate Right");
                else
                    anim.SetTrigger("Rotate Left");               
            }
            //��������
            if(cropDetails.hasParticalEffect)
                EventHandler.CallParticleEffectEvent(cropDetails.effectType,transform.position + cropDetails.effectPos );
            //��������
            if(cropDetails.soundEffect != SoundName.none)
            {
                var soundDetails = AudioManager.Instance.soundDetailsData.GetSoundDetails(cropDetails.soundEffect);
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }


        }

        if (harvestActionCount >= requireActionCount)//����������ܵļ��� 
        {
            if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation )//�ж�Ϊtrue
            {
                //����ũ����
                SpawnHarvestItems();
            }
            else if(cropDetails.hasAnimation)
            {
                Debug.Log(PlayerTransform.position.x < transform.position.x);
                if (PlayerTransform.position.x < transform.position.x)//��ҵ������Ƿ�С�ڵ�ǰ��������
                    anim.SetTrigger("FallingRight");//˵���������߿�������Ӧ�����ұߵ�
                else
                    anim.SetTrigger("FallingLeft");

                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                //Э��                
                StartCoroutine( HarvestAfterAnimation());
            }
        }
    }
 

    private IEnumerator HarvestAfterAnimation()
    {
        //�������µ���������У������ĳ���Ͳ�˯�ߵ��ǵ�һ�¶�������
        //�������Ժ󣬾�ִ�к���ĳ���
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Ending"))//������end����ʱ��ִ�к�������
        {
            yield return null;
        }

        SpawnHarvestItems();
        //ת�������׶ε�ũ����
        if(cropDetails.CropTransferID > 0)
        {
            CreateTransferCrop();
        }
    }

    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.CropTransferID;//��ת������Ҫ���ɵ���Ʒ���ߵ�ͼ��Ϣ
        tileDetails.daysSinceLastHarvest = -1;//���ڲ����ո���
        tileDetails.growthDays = 0;

        EventHandler.CallRefreshCurrentMap();
    }


    /// <summary>
    /// ����ũ����
    /// </summary>
    public void SpawnHarvestItems()
    {
        //������Ӧ�Ĺ�ʵ����
        for (int i = 0; i < cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce;
            if (cropDetails.producedMaxAmount[i] == cropDetails.producedMinAmount[i])
            {
                //����ֻ����ָ��������
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
                else//�����ͼ������
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
                //��������
                //�߼��ǰ��ܵ����� - ���ظ��ո������ �����˼���ͼ�ȥ���죩
                tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;

                //ˢ�µ�ͼUI
                EventHandler.CallRefreshCurrentMap();

            }
            else
            {
                tileDetails.daysSinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
                //�ѿ�Ҳ��ԭ
                //tileDetails.daysSinceDig = -1;
            }

            Destroy(gameObject);
        }

    }
}

