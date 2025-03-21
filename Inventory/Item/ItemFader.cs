using UnityEngine;
using DG.Tweening;


//确保代码挂载的对象一定有SpriteRenderer这个组件
[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void FadeIn()
    {
        //RGB颜色，（1，1，1，透明度）
        Color targetColor =  new Color(1, 1, 1, 1);

        //把现在的颜色在时间内更改为（1，1，1，1）
        spriteRenderer.DOColor(targetColor,Settings.fadeDuration);
    }

    public void FadeOut()
    {
        Color targetColor = new Color(1, 1, 1, Settings.targetAlpha);
        //把现在的颜色在时间内更改为（1，1，1，透明度45%）
        spriteRenderer.DOColor(targetColor, Settings.fadeDuration);
    }
}
