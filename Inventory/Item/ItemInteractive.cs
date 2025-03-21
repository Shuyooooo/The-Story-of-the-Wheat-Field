using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractive : MonoBehaviour
{
    private bool isAnimating;//�Ƿ����ڲ��Ŷ���
    private WaitForSeconds pause = new WaitForSeconds(0.04f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!isAnimating)
        {
            //���˵ ������ײ�������position.x < ��ǰ�����position.x
            if (other.transform.position.x < transform.position.x)
            {
                //��������ҡһҡ�Ķ���                ;
                StartCoroutine(RotateRight());
            }
            else
            {
                //��������ҡһҡ�Ķ���
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isAnimating)
        {
            //���˵ ������ײ�������position.x < ��ǰ�����position.x
            if (other.transform.position.x > transform.position.x)
            {
                //��������ҡһҡ�Ķ���                
                StartCoroutine(RotateRight());
            }
            else
            {
                //��������ҡһҡ�Ķ���
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private IEnumerator RotateLeft()
    {
        int angle = 2;
        isAnimating = true;
        //ͨ��ѭ��ʵ�ֽ������ƶ�
        for(int i = 0;i < 4;i++)
        {
            transform.GetChild(0).Rotate(0, 0, angle);
            yield return pause;
            
        }
        for(int i = 0;i < 4;i++)
        {
            transform.GetChild(0).Rotate(0, 0, angle * -1);
            yield return pause;
        }
        transform.GetChild(0).Rotate(0, 0, angle);
        yield return pause;

        isAnimating = false;
    }

    private IEnumerator RotateRight()
    {
        int angle = -2;
        isAnimating = true;
        //ͨ��ѭ��ʵ�ֽ������ƶ�
        for (int i = 0; i < 4; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0, 0, angle);
            yield return pause;
        }
        for (int i = 0; i < 4; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0, 0, angle * -1);
            yield return pause;
        }
        transform.GetChild(0).Rotate(0, 0, angle);
        yield return pause;

        isAnimating = false;
    }
}
