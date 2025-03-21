using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.AStar
{
    public class GridNodes
    {
        //地图整体——高度、宽度
        private int width;
        private int height;

        //拥有整张地图的坐标的二维数组
        private Node[,] gridNode;

        /// <summary>
        /// 构造函数初始化节点范围数组
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;

            //初始化数组
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
            //判断x，y坐标是否在范围内
            if (xPos < width && yPos < height)
            {
                //这里返回的是网格的二维数组，都是正数
                return gridNode[xPos, yPos];
            }
            Debug.Log("chaole");
            return null;
        }
    }
}
