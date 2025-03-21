using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public RectTransform DayNightImage;//白天黑夜的轮转
    public RectTransform clockParent;//一格一格那个钟
    public Image seasonImage;//季节的插画
    public TextMeshProUGUI dateText;//日期的text
    public TextMeshProUGUI timeText;//时间的text

    public Sprite[] seasonSprites;//四个季节的图片用于切换

    private List<GameObject> clockBlocks = new List<GameObject>();//那几个时间格子

    private void Awake()
    {
        for (int i = 0; i < clockParent.childCount; i++)
        {
            clockBlocks.Add(clockParent.GetChild(i).gameObject);//初始化小格子
            clockParent.GetChild(i).gameObject.SetActive(false);//让所有小格子都不可见
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
        timeText.text = hour.ToString("00") + ":" + minute.ToString("00");//规定format双位显示
        
    }

    /// <summary>
    /// 根据小时切换时间块显示
    /// </summary>
    /// <param name="hour"></param>
    private void SwitchHourImage(int hour)//切换小格子的插图
    {
        int index = hour / 4;

        if (index == 0)
        {
            foreach (var item in clockBlocks)
            {
                item.SetActive(false);//当index==0，关闭全部格子
            }
        }
        else
        {
            for (int i = 0; i < clockBlocks.Count; i++)//index非0的情况下，一个一个
            {
                if (i < index + 1)//比如现在index=2，i从0开始，则[0],[1],[2]小格子都要亮起来
                {
                    //Debug.Log("index: " + index);
                    clockBlocks[i].SetActive(true);
                   
                }
                else//如果index==1/4，则第一个亮起来以后，后面的都是关闭的
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



