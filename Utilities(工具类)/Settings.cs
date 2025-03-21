using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Settings
{
    public const float fadeDuration = 0.35f;

    public const float targetAlpha = 0.45f;

    //ʱ�����
    public const float secondThreshold = 0.1f;//��ֵԽСʱ��Խ��
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 30;
    public const int seasonHold = 3;

    public const float TransitionDuration = 0.8f;
    public const int reapAmount = 2;//�ղݵ�ʱ��һ��ֻ������2��

    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    public const float pixelSize = 0.05f; //���ر�����20*20��ռ1��unit 
    public const float animationBreakTime = 5f;//�������ʱ��
    public const int maxGridSize = 9999;

    //�ƹ�
    public const float LightChangeDuration = 25f;
    public static TimeSpan MorningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan NightTime = new TimeSpan(19, 0, 0);

    //����Ϸ
    public static Vector3 playerStartPos = new Vector3(-9, -2, 0);
    public static Vector3 NpcStartPos = new Vector3(-9,-0.5f, 0);
    public const int playerStartMoney = 100;
}