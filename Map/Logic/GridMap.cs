using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[ExecuteInEditMode]
public class GridMap : MonoBehaviour
{
    public MapData_SO mapData;
    public GridType gridType;
    private Tilemap currentTilemap;


    private void OnEnable()
    {
        if (!Application.IsPlaying(this))
            //如果说目前的脚本 没有 在运行
        {
            //拿到组件
            currentTilemap = GetComponent<Tilemap>();

            //打开的时候把数据清空
            if (mapData != null)
                mapData.tileProperties.Clear();
        }

    }

    private void OnDisable()
    {
        if(!Application.IsPlaying(this))
        {
            //关闭的时候再拿一遍组件（重新获取新的信息）
            currentTilemap = GetComponent<Tilemap>();  
            
            UpdateTileProperties();
#if UNITY_EDITOR
            //关闭的时候如果mapData不为空就储存信息（标记为dirty）
            if (mapData != null)
                EditorUtility.SetDirty(mapData);
#endif
        }
    }

    private void UpdateTileProperties()
    {
        //所有绘制的瓦片地图的大小已经固定
        currentTilemap.CompressBounds();
        if (!Application.IsPlaying(this))
        {
            if (mapData != null)
            {
                //已绘制范围的左下角坐标
                Vector3Int startPos = currentTilemap.cellBounds.min;
                //已绘制范围的右上角坐标
                Vector3Int endPos = currentTilemap.cellBounds.max;

                for (int x = startPos.x; x < endPos.x; x++)
                {
                    for (int y = startPos.y; y < endPos.y; y++)
                    {
                        TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));

                        //拿到了每一个所绘制的瓦片，把他们的信息写进tileproperty里面
                        if(tile != null)
                        {
                            TileProperty newTile = new TileProperty
                            {
                                tileCoordinate = new Vector2Int(x, y),
                                gridType = this.gridType,
                                boolTypeValue = true,
                            };

                            mapData.tileProperties.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}


