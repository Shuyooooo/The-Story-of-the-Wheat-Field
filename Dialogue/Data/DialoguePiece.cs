using UnityEngine;
using UnityEngine.Events;

namespace Mfarm.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("�Ի�����")]

        public Sprite faceImage;//����
        public bool onLeft;//�Ƿ������
        public string name;//��������

        [TextArea]
        public string dialogueText;//�Ի��İ�
        public bool hasToPause;//�ǲ�����Ҫ������Ұ��ո�
        public bool isDone;//�ǲ���˵����
        public UnityEvent afterTalkEvent;
    }
}
