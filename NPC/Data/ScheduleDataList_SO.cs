using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "ScheduleDataList_SO" , menuName = "NPC Schedule/ScheduleDataList")]
public class ScheduleDataList_SO : ScriptableObject
{
   public List<ScheduleDetails> schedulesList;
}
