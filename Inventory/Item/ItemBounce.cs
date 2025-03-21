using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mfarm.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTrans;//用于移动

        private BoxCollider2D coll;       

        public float gravity = -3.5f;

        private bool isGround;//是否落地

        private float distance;

        private Vector2 direction;//方向
        private Vector3 targetPos;//位置 坐标

        private void Awake()
        {
            spriteTrans = transform.GetChild(0);
            coll = GetComponent<BoxCollider2D>();
            coll.enabled = false;
        }

        private void Update()
        {
            Bounce();
        }

        /// <summary>
        /// 重新生成一个物品用于丢下
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dir"></param>
        public void InitBounceItem(Vector3 target,Vector2 dir)
        {
            coll.enabled = false;//关闭物品的碰撞不然会被重新拾取
            direction = dir;
            targetPos = target;
            distance = Vector3.Distance(target,transform.position);//目标-当前位置 向量相减

            spriteTrans.position += Vector3.up * 1.5f;//头顶的坐标是(0,1.5,0)
        }

        private void Bounce()
        {
            isGround = spriteTrans.position.y <= transform.position.y;//如果落地了落地(比1.5小)

            if(Vector3.Distance(transform.position,targetPos) > 0.1f)//看看影子到了没
            {
                //transform。position是父级物体的坐标
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;//*distance是因为距离越大可以移动越快
            }

            if(!isGround)//物品和影子（父级物体）的y轴坐标还没有重合
            {
                spriteTrans.position += Vector3.up * gravity * Time.deltaTime;//移动物品
            }
            else//要是落地了
            {
                spriteTrans.position = transform.position;
                coll.enabled = true;
            }
        }


    }
}
