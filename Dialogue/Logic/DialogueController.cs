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
            //拿到“按空格对话”的gameObject
            uiSign = transform.GetChild(1).gameObject;
            FillDialogueStack();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //如果跑进来的别个人身上有“Player”的Tag
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
                //把弹出来的那一个拿去传递数据
                EventHandler.CallShowDialogueEvent(result);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                //传到UI显示对话
                yield return new WaitUntil(() => result.isDone);
                isTalking = false;
               
            }
            else
            {
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                EventHandler.CallShowDialogueEvent(null);
                FillDialogueStack();
                isTalking = false;


                if(OnFinishEvent != null)//如果对话结束后有事件
                {
                    OnFinishEvent.Invoke();
                    canTalk = false;                    
                }                
            }              
        }
        /// <summary>
        /// 构建对话堆栈
        /// </summary>
        private void FillDialogueStack()
        {
            //初始化堆栈
            dialogueStack = new Stack<DialoguePiece>();
            for(int i = dialogueList.Count -1; i>-1;i--)
            {
                //把每一个isDone赋值
                dialogueList[i].isDone = false;
                dialogueStack.Push(dialogueList[i]);                
            }
        }
    }
}
