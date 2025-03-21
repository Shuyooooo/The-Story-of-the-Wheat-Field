using Mfarm.Dialogue;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueBox;//��һ��DialogueGameObject
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
        if(piece != null)//��ϰ�߱���debug
        {
            piece.isDone = false;//����ظ������Ի�����һ����֤ÿ�ζ����Դ���

            dialogueBox.SetActive(true);//������
            ContinueBox.SetActive(false);//��û�����ո������ʱ��

            dialogueText.text = string.Empty;

            if(piece.name != string.Empty)//���˵���ֲ����ǿ��ַ���
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

            piece.isDone = true;//��һ��piece��isDone == true

            if(piece.isDone && piece.hasToPause)
            {
                ContinueBox.SetActive(true);
            }           
        }
        else
        {
            //piece���Ӷ�ջ����������
            dialogueBox?.SetActive(false);
            yield break;
        }
    }

}
