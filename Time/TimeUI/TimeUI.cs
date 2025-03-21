using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public RectTransform DayNightImage;//�����ҹ����ת
    public RectTransform clockParent;//һ��һ���Ǹ���
    public Image seasonImage;//���ڵĲ廭
    public TextMeshProUGUI dateText;//���ڵ�text
    public TextMeshProUGUI timeText;//ʱ���text

    public Sprite[] seasonSprites;//�ĸ����ڵ�ͼƬ�����л�

    private List<GameObject> clockBlocks = new List<GameObject>();//�Ǽ���ʱ�����

    private void Awake()
    {
        for (int i = 0; i < clockParent.childCount; i++)
        {
            clockBlocks.Add(clockParent.GetChild(i).gameObject);//��ʼ��С����
            clockParent.GetChild(i).gameObject.SetActive(false);//������С���Ӷ����ɼ�
        }
    }

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvent;
    }


    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvent;
    }

    private void OnGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        
        dateText.text = month.ToString("00") + "/" + day.ToString("00") + "/" + year;
        seasonImage.sprite = seasonSprites[(int)season];
        
        SwitchHourImage(hour);
        DayNightImageRotation(hour);
        
    }

    private void OnGameMinuteEvent(int minute, int hour ,int day,Season season)
    {
        timeText.text = hour.ToString("00") + ":" + minute.ToString("00");//�涨format˫λ��ʾ
        
    }

    /// <summary>
    /// ����Сʱ�л�ʱ�����ʾ
    /// </summary>
    /// <param name="hour"></param>
    private void SwitchHourImage(int hour)//�л�С���ӵĲ�ͼ
    {
        int index = hour / 4;

        if (index == 0)
        {
            foreach (var item in clockBlocks)
            {
                item.SetActive(false);//��index==0���ر�ȫ������
            }
        }
        else
        {
            for (int i = 0; i < clockBlocks.Count; i++)//index��0������£�һ��һ��
            {
                if (i < index + 1)//��������index=2��i��0��ʼ����[0],[1],[2]С���Ӷ�Ҫ������
                {
                    //Debug.Log("index: " + index);
                    clockBlocks[i].SetActive(true);
                   
                }
                else//���index==1/4�����һ���������Ժ󣬺���Ķ��ǹرյ�
                {
                    clockBlocks[i].SetActive(false);
                    
                }
                
            }
        }
    }

    private void DayNightImageRotation(int hour)
    {
        var target = new Vector3(0, 0, hour * 15 -90);
        DayNightImage.DORotate(target, 1f, RotateMode.Fast);
    }
}



