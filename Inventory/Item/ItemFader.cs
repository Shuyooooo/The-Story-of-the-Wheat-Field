using UnityEngine;
using DG.Tweening;


//ȷ��������صĶ���һ����SpriteRenderer������
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
        //RGB��ɫ����1��1��1��͸���ȣ�
        Color targetColor =  new Color(1, 1, 1, 1);

        //�����ڵ���ɫ��ʱ���ڸ���Ϊ��1��1��1��1��
        spriteRenderer.DOColor(targetColor,Settings.fadeDuration);
    }

    public void FadeOut()
    {
        Color targetColor = new Color(1, 1, 1, Settings.targetAlpha);
        //�����ڵ���ɫ��ʱ���ڸ���Ϊ��1��1��1��͸����45%��
        spriteRenderer.DOColor(targetColor, Settings.fadeDuration);
    }
}
