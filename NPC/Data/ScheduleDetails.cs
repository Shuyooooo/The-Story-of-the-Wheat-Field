using UnityEngine;
using System;


[Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int hour, minute, day;
    public int priority;//不同季节使用不同的优先级判定事件的先后
    public Season season;
    public string targetScene;
    public Vector2Int targetGridPosition;//需要知道NPC每一步的位置
    public AnimationClip clipAtStop;//需要知道什么节点放什么动画
    public bool interactable;//需要知道是否是可互动的

    /// <summary>
    /// 几时几分几秒在哪个地图的哪里
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
    /// 优先级判断，小的先执行
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(ScheduleDetails other) 
    {
        if(Time == other.Time)
        {
            if (priority > other.priority)//如果主体的优先级 > 其他的优先级
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
