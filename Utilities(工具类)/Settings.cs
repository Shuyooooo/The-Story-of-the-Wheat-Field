using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Settings
{
    public const float fadeDuration = 0.35f;

    public const float targetAlpha = 0.45f;

    //时间相关
    public const float secondThreshold = 0.1f;//数值越小时间越快
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 30;
    public const int seasonHold = 3;

    public const float TransitionDuration = 0.8f;
    public const int reapAmount = 2;//收草的时候一次只能销毁2个

    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    public const float pixelSize = 0.05f; //像素比例：20*20，占1个unit 
    public const float animationBreakTime = 5f;//动画间隔时间
    public const int maxGridSize = 9999;

    //灯光
    public const float LightChangeDuration = 25f;
    public static TimeSpan MorningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan NightTime = new TimeSpan(19, 0, 0);

    //新游戏
    public static Vector3 playerStartPos = new Vector3(-9, -2, 0);
    public static Vector3 NpcStartPos = new Vector3(-9,-0.5f, 0);
    public const int playerStartMoney = 100;
}