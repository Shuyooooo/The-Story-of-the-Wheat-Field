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

        private List<Node> openNodeList; //��ǰѡ��Node��Χ��8���㡪�����������ʱ��
        private HashSet<Node> closedNodeList;//���б�ѡ�еĵ㡪�����ڲ���

        private bool pathFound;//�Ƿ��ҵ�·��

        /// <summary>
        /// ����·������Stack��ÿһ��
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="npcMovementStack"></param>
        public void BuildPath(string sceneName,Vector2Int startPos,Vector2Int endPos,Stack<MovementStep> npcMovementStack)
        {
            //�ҵ�����û�е�
            pathFound = false;   
            
            if (GenerateGridNodes(sceneName,startPos,endPos))
            {               
                //�������·��
                if(FindShortestPath())
                {
                    //Debug.Log(targetNode.gridPosition);
                    //����NPC�ƶ�·��
                    UpdatePathOnMovementStepStack(sceneName, npcMovementStack);                   
                }

            }
        }
        /// <summary>
        /// ��������ڵ���Ϣ����ʼ��openList��HashSetList
        /// </summary>
        /// <param name="sceneName">������</param>
        /// <param name="startPos">���</param>
        /// <param name="endPos">�յ�</param>
        /// <returns></returns>
        private bool GenerateGridNodes(string sceneName,Vector2Int startPos,Vector2Int endPos)
        {           
            if (GridMapManager.Instance.GetGridDimensions(sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin))
            {
                //����һ���µ���������
                gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);

                //����map��x��NPC����A*���봫���gridWitdh��ͬ����y
                gridWidth = gridDimensions.x;
                gridHeight = gridDimensions.y;

                //ԭ������Ҳ�������ͼ�����Ǳ���һ��
                originX = gridOrigin.x;
                originY = gridOrigin.y;

                openNodeList = new List<Node>();
                closedNodeList = new HashSet<Node>();
            }
            else//Ҫ��û�õ�������false
            return false;
           

            //ȥGridNodes������ִ�з��������õ����ǿ�ʼ�ͽ���λ�õľ���Ķ�ά����
            startNode = gridNodes.GetGridNode(startPos.x - originX,startPos.y - originY);
            targetNode = gridNodes.GetGridNode(endPos.x - originX, endPos.y - originY);

           // Debug.Log(endPos.x + "   ,   "+endPos.y);
            //Debug.Log(startNode.gridPosition);
            //Debug.Log(targetNode.gridPosition);
            //һ��һ���� �����Ƿ�Ϊobstacle �� ��ϸ����������
            for (int x =0; x < gridWidth;x++)
            {
                for(int y = 0;y < gridHeight;y++)
                {
                    
                    Vector3Int tilePos = new Vector3Int(x + originX,y + originY,0);
                    var key = tilePos.x + "x" + tilePos.y + "y" + sceneName;

                    TileDetails tile = GridMapManager.Instance.GetTileDetails(key);

                    if(tile != null)
                    {
                        //�����Ǹ�����˭��˭ ��λ��node

                        Node node = gridNodes.GetGridNode(x, y);

                        if (tile.isNPCObstacle)
                            node.isObstacle = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// �ҵ����·������node��ӵ�closedNodeList
        /// </summary>
        /// <returns></returns>
        private bool FindShortestPath()
        {
            //�ӵ�һ���㿪ʼ 
            openNodeList.Add(startNode);

            while (openNodeList.Count > 0)
            {
                //�ڵ�����Node�ں��ȽϺ���
                openNodeList.Sort();
                //�ź��ˣ�˵������ľ���List����ĵ�һ��
                Node closeNode = openNodeList[0];
              
                openNodeList.RemoveAt(0);
                closedNodeList.Add(closeNode);
                
                              
                if (closeNode == targetNode)
                {
                    pathFound = true;
                    break;
                }
                //������Χ8��Node���䵽OpenList
                EvaluateNeighbourNodes(closeNode);
                
            }            
            return pathFound;
        }
        /// <summary>
        /// ������Χ8����
        /// </summary>
        /// <param name="currentNode">��ǰѡ�нڵ�</param>
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            //����һ��Node�����������������Ϊ��ǰnode����
            Vector2Int currentNodePos = currentNode.gridPosition;
            Node validNeighbourNode;

            //ͨ�����ַ�ʽ�ҵ��������ҵ�8����
            for(int x = -1 ; x <= 1;x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    if(x == 0 && y== 0)
                    
                        //������ҵ�����currentNode
                        continue;

                    //��Ҫһ���жϽڵ��Ƿ���õķ���
                    validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y+y);

                    if(validNeighbourNode != null)
                    {
                        //�������
                        if (!openNodeList.Contains(validNeighbourNode))
                        {
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode,validNeighbourNode);
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                                                    
                            //���Ӹ��ڵ�
                            validNeighbourNode.parentNode = currentNode;
                            openNodeList.Add(validNeighbourNode);   
                        }
                    }                   
                }
            }
        }
        /// <summary>
        /// �ҵ���Χ8���㲢���ؿ��õ��Valid����
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node GetValidNeighbourNode(int x,int y)
        {
            //����ж���ǰ���Ƿ��ǿ��õ㡪�����ڱ߽��������ж��Ƿ��ڷ�Χ��
            if (x >= gridWidth || y >= gridHeight || x < 0 || y< 0 )
            
                return null;

               //��ѭ���������ж���Χ��8����
            Node neighbourNode = gridNodes.GetGridNode(x,y);
               
                //�����+0��+0������currentNode����
            if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
                    return null;//���ؿ�

            else return neighbourNode;                      
        }

        /// <summary>
        /// ����������֮��ľ���
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private int GetDistance(Node nodeA,Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if(xDistance > yDistance)//˵����ˮƽ������x�����ʱ��
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            //��ֱ������y�����ʱ��
            return 14 * xDistance + 10 * (yDistance - xDistance);      
        }

        /// <summary>
        /// ����·��ÿһ��������ͳ�������
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="npcMovementStep"></param>
        private void UpdatePathOnMovementStepStack(string sceneName,Stack<MovementStep> npcMovementStep)
        {
            //��closeNodeList�е��ƻ�ȥ
            Node nextNode = targetNode;

            while(nextNode != null)
            {
                //����� (��ѭ����) ��һ�����ʹ���һ���µ� ʱ�������  ׼��ѹ��ջ��
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;
                newStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX,nextNode.gridPosition.y + originY);
                

                //ѹ���ջ
                npcMovementStep.Push(newStep);
                nextNode = nextNode.parentNode;//ͨ�����׽ڵ��ƶ�����ǰһ���ڵ�
                


            }
        }
    }

    
}
