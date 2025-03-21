using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    //����һ��gameobject��Ϊʹ�ö���ص� prefabs
    public List<GameObject> poolPrefabs;

    //����������б�
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();
    private Queue<GameObject> soundQueue = new Queue<GameObject>();
    private void OnEnable()
    {
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        EventHandler.InitSoundEffect += InitSoundEffect;
    }

    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= InitSoundEffect;
    }

    

    private void Start()
    {
        CreatePool();
    }

  

    /// <summary>
    /// ���ɶ����
    /// </summary>
    private void CreatePool()
    {
        foreach (GameObject item in poolPrefabs)
        {
            //�����GameObject()�ǹ��캯����ֻ���ڴ���ʵ��ʱʹ��
            Transform Parent = new GameObject(item.name).transform;
            Parent.SetParent(transform); //SetParent��transform�ķ��� ���÷���

            //ͬ���˴���Object<GameObject>()Ҳ�����ڳ�ʼ��ʱ�Ĺ��캯��
            var newPool = new ObjectPool<GameObject>(
                () => Instantiate(item,Parent),
                e => { e.SetActive(true); },
                e => { e.SetActive(false); },
                e => { Destroy(e); },
                true,10,20
                );
            poolEffectList.Add(newPool);
        }
    }

    private void OnParticleEffectEvent(ParticleEffectType effectType, Vector3 effectPos)
    {
        //WORKFLOW:������Ч��ȫ
        //�﷨��
        ObjectPool<GameObject> objpool = effectType switch
        {

            ParticleEffectType.LeaveFalling01 => poolEffectList[0],
            ParticleEffectType.LeaveFalling02 => poolEffectList[1],
            ParticleEffectType.Rock => poolEffectList[2],
            ParticleEffectType.ReapableScenery => poolEffectList[3],
            _=> null,
        };
        GameObject obj = objpool.Get();
        obj.transform.position = effectPos;
        StartCoroutine(ReleaseRoutine(objpool, obj));
    }

    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool,GameObject obj)
    {
         var time = new WaitForSeconds(1.5f);
        yield return time;
        pool.Release(obj);
    }

    /*private void InitSoundEffect(SoundDetails soundDetails)
    {
        ObjectPool<GameObject>pool = poolEffectList[4];
        var obj = pool.Get();

        obj.GetComponent<Sound>().SetSound(soundDetails);
        StartCoroutine(DisableSound(pool, obj, soundDetails));
    }

    private IEnumerator DisableSound(ObjectPool<GameObject> pool,GameObject obj,SoundDetails soundDetails)
    {
        //��һ��ʱ��release == ��ǡ�õ�ʱ�䲥������release
        yield return new WaitForSeconds(soundDetails.soundClip.length);
        pool.Release(obj);
    }*/

    private void CreateSoundPool()
    {
        var parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(transform);

        for(int i =0;i<20;i++)//������Ĭ������20��
        {
            GameObject newobj = Instantiate(poolPrefabs[4], parent);
            newobj.SetActive(false);
            soundQueue.Enqueue(newobj);
        }
    }

    private GameObject GetPoolObject()
    {
        if (soundQueue.Count < 2)
            CreateSoundPool();
        return soundQueue.Dequeue();//���ö��е�һ��
    }

    private void InitSoundEffect(SoundDetails soundDetails)
    {
        var obj = GetPoolObject();
        obj.GetComponent<Sound>().SetSound(soundDetails);
        obj.SetActive(true);
        StartCoroutine(DisableSound(obj, soundDetails.soundClip.length));
    }

    private IEnumerator DisableSound(GameObject obj,float duration)
    {
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
        soundQueue.Enqueue(obj);
    }


}

