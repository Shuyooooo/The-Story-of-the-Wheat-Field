using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mfarm.AStar
{
    public class Node : IComparable <Node>
    {
        public Vector2Int gridPosition;//��������
        public int gCost = 0;//����Start���ӵľ���
        public int hCost = 0;//����Target���ӵľ���
        public int FCost => gCost + hCost;//��ǰ���ӵ�ֵ
        public bool isObstacle = false;//��ǰ�����Ƿ����ϰ�
        public Node parentNode;

        /// <summary>
        /// ��һ���ǽڵ�ĳ�ʼ��
        /// </summary>
        /// <param name="pos"></param>
        public Node(Vector2Int pos)
        {
            gridPosition = pos;
            parentNode = null;
        }

        public int CompareTo(Node other)
        {
            //�Ƚ�ѡ����͵�Fֵ������-1��0��1
            int result = FCost.CompareTo(other.FCost);
            if (result == 0)//FCost���
            {
                //�ͱȽ����㵽�յ�ľ���
                result = hCost.CompareTo(other.hCost);
            }
            return result;
        }
    }
}
