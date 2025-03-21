using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItemFader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ItemFader[] faders = collision.GetComponentsInChildren<ItemFader>();
        
        if(faders.Length > 0)//都拿到的情况下
        {
            foreach (var item in faders)//把每一个挂载了ItemFader的子对象
            {
                item.FadeOut();//搞透明
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ItemFader[] faders = collision.GetComponentsInChildren<ItemFader>();

        if (faders.Length > 0)
        {
            foreach (var item in faders)
            {
                item.FadeIn();
            }
        }
    }



    private BoxCollider2D player;

    private void Start()
    {
        player = GetComponent<BoxCollider2D>();
    }
   
}
