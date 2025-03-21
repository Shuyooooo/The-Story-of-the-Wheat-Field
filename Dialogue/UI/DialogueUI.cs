using Mfarm.Dialogue;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueBox;//整一个DialogueGameObject
    public Text dialogueText;
    public Image faceLeft, faceRight;
    public Text nameLeft, nameRight;
    public GameObject ContinueBox;

    private void Awake()
    {
        ContinueBox.SetActive(false);
    }

    private void OnEnable()
    {
        EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
    }

    
    private void OnDisable()
    {
        EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
    }

    private void OnShowDialogueEvent(DialoguePiece piece)
    {
        StartCoroutine(ShowDialogue(piece));
    }

    private IEnumerator ShowDialogue(DialoguePiece piece)
    {
        if(piece != null)//好习惯便于debug
        {
            piece.isDone = false;//如果重复触发对话，这一步保证每次都可以触发

            dialogueBox.SetActive(true);//把面板打开
            ContinueBox.SetActive(false);//还没到按空格继续的时候

            dialogueText.text = string.Empty;

            if(piece.name != string.Empty)//如果说名字部分是空字符串
            {
                if(piece.onLeft)
                {
                    faceRight.gameObject.SetActive(false) ;
                    faceLeft.gameObject.SetActive(true) ;
                    faceLeft.sprite = piece.faceImage;
                    nameLeft.text = piece.name;
                }
                else
                {
                    faceRight.gameObject.SetActive(true);
                    faceLeft.gameObject.SetActive(false);
                    faceRight.sprite = piece.faceImage;
                    nameRight.text = piece.name;
                }
            }
            else
            {
                faceRight.gameObject.SetActive(false);
                faceLeft.gameObject.SetActive(false);
                nameLeft.gameObject.SetActive(false);
                nameRight.gameObject.SetActive(false);
            }
            yield return dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion();

            piece.isDone = true;//这一条piece的isDone == true

            if(piece.isDone && piece.hasToPause)
            {
                ContinueBox.SetActive(true);
            }           
        }
        else
        {
            //piece都从堆栈里面拿完了
            dialogueBox?.SetActive(false);
            yield break;
        }
    }

}
