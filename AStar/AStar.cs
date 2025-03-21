using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.map;
using Unity.Mathematics;
using System;

namespace Mfarm.AStar
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes gridNodes;

        private Node startNode;
        private Node targetNode;

        private int gridWidth;
        private int gridHeight;
        private int originX;
        private int originY;

        private List<Node> openNodeList; //当前选中Node周围的8个点——用于添加临时点
        private HashSet<Node> closedNodeList;//所有被选中的点——用于查找

        private bool pathFound;//是否找到路径

        /// <summary>
        /// 构建路径更新Stack的每一步
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="npcMovementStack"></param>
        public void BuildPath(string sceneName,Vector2Int startPos,Vector2Int endPos,Stack<MovementStep> npcMovementStack)
        {
            //找到了吗？没有的
            pathFound = false;   
            
            if (GenerateGridNodes(sceneName,startPos,endPos))
            {               
                //查找最短路径
                if(FindShortestPath())
                {
                    //Debug.Log(targetNode.gridPosition);
                    //构建NPC移动路径
                    UpdatePathOnMovementStepStack(sceneName, npcMovementStack);                   
                }

            }
        }
        /// <summary>
        /// 构建网格节点信息，初始化openList和HashSetList
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns></returns>
        private bool GenerateGridNodes(string sceneName,Vector2Int startPos,Vector2Int endPos)
        {           
            if (GridMapManager.Instance.GetGridDimensions(sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin))
            {
                //创建一个新的坐标网络
                gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);

                //脚下map的x给NPC身上A*代码传入的gridWitdh，同理传入y
                gridWidth = gridDimensions.x;
                gridHeight = gridDimensions.y;

                //原点数据也从网格地图数据那边拿一份
                originX = gridOrigin.x;
                originY = gridOrigin.y;

                openNodeList = new List<Node>();
                closedNodeList = new HashSet<Node>();
            }
            else//要是没拿到，返回false
            return false;
           

            //去GridNodes里面那执行方法——得到的是开始和结束位置的具体的二维数组
            startNode = gridNodes.GetGridNode(startPos.x - originX,startPos.y - originY);
            targetNode = gridNodes.GetGridNode(endPos.x - originX, endPos.y - originY);

           // Debug.Log(endPos.x + "   ,   "+endPos.y);
            //Debug.Log(startNode.gridPosition);
            //Debug.Log(targetNode.gridPosition);
            //一个一个的 载入是否为obstacle 及 详细的数组数据
            for (int x =0; x < gridWidth;x++)
            {
                for(int y = 0;y < gridHeight;y++)
                {
                    
                    Vector3Int tilePos = new Vector3Int(x + originX,y + originY,0);
                    var key = tilePos.x + "x" + tilePos.y + "y" + sceneName;

                    TileDetails tile = GridMapManager.Instance.GetTileDetails(key);

                    if(tile != null)
                    {
                        //告诉那个网格谁是谁 单位是node

                        Node node = gridNodes.GetGridNode(x, y);

                        if (tile.isNPCObstacle)
                            node.isObstacle = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 找到最短路径所有node添加到closedNodeList
        /// </summary>
        /// <returns></returns>
        private bool FindShortestPath()
        {
            //从第一个点开始 
            openNodeList.Add(startNode);

            while (openNodeList.Count > 0)
            {
                //节点排序，Node内含比较函数
                openNodeList.Sort();
                //排好了，说明最近的就是List里面的第一个
                Node closeNode = openNodeList[0];
              
                openNodeList.RemoveAt(0);
                closedNodeList.Add(closeNode);
                
                              
                if (closeNode == targetNode)
                {
                    pathFound = true;
                    break;
                }
                //计算周围8个Node补充到OpenList
                EvaluateNeighbourNodes(closeNode);
                
            }            
            return pathFound;
        }
        /// <summary>
        /// 评估周围8个点
        /// </summary>
        /// <param name="currentNode">当前选中节点</param>
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            //传入一个Node，把里面的坐标设置为当前node坐标
            Vector2Int currentNodePos = currentNode.gridPosition;
            Node validNeighbourNode;

            //通过这种方式找到上下左右的8个点
            for(int x = -1 ; x <= 1;x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    if(x == 0 && y== 0)
                    
                        //这代表找到的是currentNode
                        continue;

                    //需要一个判断节点是否可用的方法
                    validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y+y);

                    if(validNeighbourNode != null)
                    {
                        //计算距离
                        if (!openNodeList.Contains(validNeighbourNode))
                        {
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode,validNeighbourNode);
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                                                    
                            //链接父节点
                            validNeighbourNode.parentNode = currentNode;
                            openNodeList.Add(validNeighbourNode);   
                        }
                    }                   
                }
            }
        }
        /// <summary>
        /// 找到周围8个点并返回可用点的Valid方法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node GetValidNeighbourNode(int x,int y)
        {
            //如何判定当前点是否是可用点—考虑在边界的情况—判定是否在范围内
            if (x >= gridWidth || y >= gridHeight || x < 0 || y< 0 )
            
                return null;

               //在循环内依次判断周围的8个点
            Node neighbourNode = gridNodes.GetGridNode(x,y);
               
                //如果是+0，+0表明是currentNode自身
            if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
                    return null;//返回空

            else return neighbourNode;                      
        }

        /// <summary>
        /// 测算两个点之间的距离
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private int GetDistance(Node nodeA,Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if(xDistance > yDistance)//说明是水平方向上x更大的时候
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            //垂直方向上y更大的时候
            return 14 * xDistance + 10 * (yDistance - xDistance);      
        }

        /// <summary>
        /// 更新路径每一步的坐标和场景名字
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="npcMovementStep"></param>
        private void UpdatePathOnMovementStepStack(string sceneName,Stack<MovementStep> npcMovementStep)
        {
            //从closeNodeList中倒推回去
            Node nextNode = targetNode;

            while(nextNode != null)
            {
                //疯狂倒推 (在循环里) 推一步，就创建一个新的 时间戳类型  准备压入栈中
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;
                newStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX,nextNode.gridPosition.y + originY);
                

                //压入堆栈
                npcMovementStep.Push(newStep);
                nextNode = nextNode.parentNode;//通过父亲节点移动到再前一个节点
                


            }
        }
    }

    
}
