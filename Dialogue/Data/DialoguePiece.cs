using UnityEngine;
using UnityEngine.Events;

namespace Mfarm.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")]

        public Sprite faceImage;//界面
        public bool onLeft;//是否在左边
        public string name;//人物名字

        [TextArea]
        public string dialogueText;//对话文案
        public bool hasToPause;//是不是需要等着玩家按空格
        public bool isDone;//是不是说完了
        public UnityEvent afterTalkEvent;
    }
}
