using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.AStar
{
    public class GridNodes
    {
        //��ͼ���塪���߶ȡ����
        private int width;
        private int height;

        //ӵ�����ŵ�ͼ������Ķ�ά����
        private Node[,] gridNode;

        /// <summary>
        /// ���캯����ʼ���ڵ㷶Χ����
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;

            //��ʼ������
            gridNode = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridNode[x, y] = new Node(new Vector2Int(x, y));
                }
            }

        }

        public Node GetGridNode(int xPos, int yPos)
        {
            //�ж�x��y�����Ƿ��ڷ�Χ��
            if (xPos < width && yPos < height)
            {
                //���ﷵ�ص�������Ķ�ά���飬��������
                return gridNode[xPos, yPos];
            }
            Debug.Log("chaole");
            return null;
        }
    }
}
