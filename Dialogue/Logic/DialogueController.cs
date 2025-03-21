using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mfarm.Dialogue
{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        private NPCMovement npc => GetComponent<NPCMovement>();
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();

        public UnityEvent OnFinishEvent;

        private Stack<DialoguePiece> dialogueStack;
        private bool canTalk;
        private bool isTalking;
        private GameObject uiSign;
        

        private void Awake()
        {
            //�õ������ո�Ի�����gameObject
            uiSign = transform.GetChild(1).gameObject;
            FillDialogueStack();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //����ܽ����ı���������С�Player����Tag
            if(other.CompareTag("Player"))
            {
                if (other.CompareTag("Player"))
                {
                    canTalk = !npc.isMoving && npc.interactable;
                }               
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                canTalk = false;
               
            }
        }

        private void Update()
        {
            uiSign.SetActive(canTalk);
            
            if (canTalk & Input.GetKeyDown(KeyCode.Space) && !isTalking)
            {               
                StartCoroutine(DialogueRoutine());
                
            }
        }

        private IEnumerator DialogueRoutine()
        {
            isTalking = true;
            if(dialogueStack.TryPop(out DialoguePiece result))
            {              
                //�ѵ���������һ����ȥ��������
                EventHandler.CallShowDialogueEvent(result);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                //����UI��ʾ�Ի�
                yield return new WaitUntil(() => result.isDone);
                isTalking = false;
               
            }
            else
            {
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                EventHandler.CallShowDialogueEvent(null);
                FillDialogueStack();
                isTalking = false;


                if(OnFinishEvent != null)//����Ի����������¼�
                {
                    OnFinishEvent.Invoke();
                    canTalk = false;                    
                }                
            }              
        }
        /// <summary>
        /// �����Ի���ջ
        /// </summary>
        private void FillDialogueStack()
        {
            //��ʼ����ջ
            dialogueStack = new Stack<DialoguePiece>();
            for(int i = dialogueList.Count -1; i>-1;i--)
            {
                //��ÿһ��isDone��ֵ
                dialogueList[i].isDone = false;
                dialogueStack.Push(dialogueList[i]);                
            }
        }
    }
}
