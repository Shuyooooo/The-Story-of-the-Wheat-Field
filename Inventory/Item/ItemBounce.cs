using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mfarm.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTrans;//�����ƶ�

        private BoxCollider2D coll;       

        public float gravity = -3.5f;

        private bool isGround;//�Ƿ����

        private float distance;

        private Vector2 direction;//����
        private Vector3 targetPos;//λ�� ����

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
        /// ��������һ����Ʒ���ڶ���
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dir"></param>
        public void InitBounceItem(Vector3 target,Vector2 dir)
        {
            coll.enabled = false;//�ر���Ʒ����ײ��Ȼ�ᱻ����ʰȡ
            direction = dir;
            targetPos = target;
            distance = Vector3.Distance(target,transform.position);//Ŀ��-��ǰλ�� �������

            spriteTrans.position += Vector3.up * 1.5f;//ͷ����������(0,1.5,0)
        }

        private void Bounce()
        {
            isGround = spriteTrans.position.y <= transform.position.y;//�����������(��1.5С)

            if(Vector3.Distance(transform.position,targetPos) > 0.1f)//����Ӱ�ӵ���û
            {
                //transform��position�Ǹ������������
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;//*distance����Ϊ����Խ������ƶ�Խ��
            }

            if(!isGround)//��Ʒ��Ӱ�ӣ��������壩��y�����껹û���غ�
            {
                spriteTrans.position += Vector3.up * gravity * Time.deltaTime;//�ƶ���Ʒ
            }
            else//Ҫ�������
            {
                spriteTrans.position = transform.position;
                coll.enabled = true;
            }
        }


    }
}
