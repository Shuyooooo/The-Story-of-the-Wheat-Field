using UnityEngine;
using System;


[Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int hour, minute, day;
    public int priority;//��ͬ����ʹ�ò�ͬ�����ȼ��ж��¼����Ⱥ�
    public Season season;
    public string targetScene;
    public Vector2Int targetGridPosition;//��Ҫ֪��NPCÿһ����λ��
    public AnimationClip clipAtStop;//��Ҫ֪��ʲô�ڵ��ʲô����
    public bool interactable;//��Ҫ֪���Ƿ��ǿɻ�����

    /// <summary>
    /// ��ʱ���ּ������ĸ���ͼ������
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="day"></param>
    /// <param name="priority"></param>
    /// <param name="season"></param>
    /// <param name="targetScene"></param>
    /// <param name="targetGridPosition"></param>
    /// <param name="clipAtStop"></param>
    /// <param name="interactable"></param>
    public ScheduleDetails(int hour, int minute, int day, int priority, Season season, 
        string targetScene, Vector2Int targetGridPosition, AnimationClip clipAtStop, bool interactable)
    {
        this.hour = hour;
        this.minute = minute;
        this.day = day;
        this.priority = priority;
        this.season = season;
        this.targetScene = targetScene;
        this.targetGridPosition = targetGridPosition;
        this.clipAtStop = clipAtStop;
        this.interactable = interactable;
    }

    public int Time => (hour * 100) + minute;

    /// <summary>
    /// ���ȼ��жϣ�С����ִ��
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(ScheduleDetails other) 
    {
        if(Time == other.Time)
        {
            if (priority > other.priority)//�����������ȼ� > ���������ȼ�
                return 1;
            else 
                return -1;
        }
        else if(Time > other.Time)
        {
            return 1;
        }
        else if (Time < other.Time)
        {
            return -1;
        }

        return 0;
    }
}
