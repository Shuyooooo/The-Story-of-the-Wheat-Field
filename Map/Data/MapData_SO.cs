using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData_SO" , menuName = "Map/MapData")]
public class MapData_SO : ScriptableObject
{
    [SceneName] public string sceneName;
    [Header("Map Information")]
    public int gridWidth;
    public int gridHeight;

    [Header("Origin at the bottom left corner")]
    public int originX;
    public int originY;

    public List<TileProperty> tileProperties;//包含坐标、地块类型、bool

}
