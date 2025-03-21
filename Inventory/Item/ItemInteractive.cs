using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractive : MonoBehaviour
{
    private bool isAnimating;//是否正在播放动画
    private WaitForSeconds pause = new WaitForSeconds(0.04f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!isAnimating)
        {
            //如果说 其他碰撞的物体的position.x < 当前物体的position.x
            if (other.transform.position.x < transform.position.x)
            {
                //播放往右摇一摇的动画                ;
                StartCoroutine(RotateRight());
            }
            else
            {
                //播放往左摇一摇的动画
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isAnimating)
        {
            //如果说 其他碰撞的物体的position.x < 当前物体的position.x
            if (other.transform.position.x > transform.position.x)
            {
                //播放往右摇一摇的动画                
                StartCoroutine(RotateRight());
            }
            else
            {
                //播放往左摇一摇的动画
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private IEnumerator RotateLeft()
    {
        int angle = 2;
        isAnimating = true;
        //通过循环实现渐渐近移动
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
        //通过循环实现渐渐近移动
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
