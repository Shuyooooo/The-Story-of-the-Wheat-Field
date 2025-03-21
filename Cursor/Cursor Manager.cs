using Mfarm.CropPlant;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mfarm.map;
using Mfarm.Inventory;
using UnityEngine.Android;
using System.Runtime.CompilerServices;


public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed,commodity;//拖拽赋值

    private Sprite currentSprite;//存储当前鼠标图片
    private Image cursorImage;

    //建造图标跟随
    private Image buildImage;

    private RectTransform cursorCanvas;
    private Camera mainCamera;//把屏幕坐标转换为世界坐标所需变量
    private Grid currentGrid;//把世界坐标转化为网格坐标所需变量

    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;

    private bool cursorEnable;

    private bool cursorPositionValid;

    private ItemDetails currentItem;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;//挂着player脚本的gameobject的transform
    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    
  

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    

    private void Start()
    {
        //UI的组件都是RectTransform,根据标签找到父物体
        cursorCanvas = GameObject.FindGameObjectWithTag("Cursor Canvas").GetComponent<RectTransform>();
        //找到父物体的第一个子物体
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();

        //拿到建造图标       
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);
        

        currentSprite = normal;
        SetCursorImage(normal);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (cursorCanvas == null) return;

        cursorImage.transform.position = Input.mousePosition;

        if ( !InteractWithUI() && cursorEnable)
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        else
        {
            SetCursorImage(normal);
            //Debug.Log(EventSystem.current.currentSelectedGameObject);
            //Debug.Log(InteractWithUI());
            buildImage.gameObject.SetActive(false);//点了一下要是没有互动就关闭
        }
    }

    private void CheckPlayerInput()
    {
        if(Input.GetMouseButtonDown(0) && cursorPositionValid)
        {
            //一触即发
            EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
        }
    }

    #region 设置鼠标样式
    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite"></param>
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);//设置初始图片完全显示
    }

    /// <summary>
    /// 设置鼠标可用
    /// </summary>
    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
        buildImage.color = new Color(1, 1, 1, 0.6f);
    }

    /// <summary>
    /// 设置鼠标不可用
    /// </summary>
    private void SetCursorInvalid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 0, 0, 0.4f);
        buildImage.color = new Color(1, 0, 1, 0.6f);
    }
    #endregion

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {

        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
            buildImage.gameObject.SetActive(false);
        }
        else
        {
            currentItem = itemDetails;
            //语法糖嘻嘻
            currentSprite = itemDetails.itemType switch
            {
                ItemType.Seed => seed,
                ItemType.Commodity => commodity,
                ItemType.ChopTool => tool,
                ItemType.HoeTool => tool,
                ItemType.WaterTool => tool,
                ItemType.BreakTool => tool,
                ItemType.ReapTool => tool,
                ItemType.Furniture => tool,
                ItemType.CollectTool => tool,
                _ => normal,
            };
           cursorEnable = true;

            //如果点选的是家具,显示图片
            if(itemDetails.itemType == ItemType.Furniture)
            {                                
                buildImage.gameObject.SetActive(true);//显示家具图标
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                buildImage.SetNativeSize();
                buildImage.rectTransform.localScale = new Vector3(0.22f, 0.22f, 1f);
                
            }
        }       
    }

    

    //WORKFLOW:
    /// <summary>
    /// 把屏幕坐标转换为网格坐标
    /// </summary>
    private void CheckCursorValid()
    {
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,-mainCamera.transform.position.z));
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

        var playerGridPos = currentGrid.WorldToCell(PlayerTransform.position);

        //建造图片跟随移动
        buildImage.rectTransform.position = Input.mousePosition;
        

        //判断在使用范围内
        if(Mathf.Abs(mouseGridPos.x - playerGridPos.x) > currentItem.itemUseRadius 
            ||Mathf.Abs( mouseGridPos.y - playerGridPos.y) > currentItem.itemUseRadius)
        {
            SetCursorInvalid();          
            return;
        }
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
                
        if (currentTile != null)
        {
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);//写过的函数
            crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);//单独拿一下点击crop的object
            switch (currentItem.itemType)
            {
                case ItemType.Seed:
                    //如果现在这个瓦片已经挖开了，且种子ID为空（未播种），则显示可播种
                    if (currentTile.daysSinceDig > -1 && currentTile.seedItemID == -1) SetCursorValid(); 
                    else SetCursorInvalid();
                    break;
                case ItemType.Commodity:
                    if (currentTile.canDropItem && currentItem.canDropped) 
                    { SetCursorValid(); } else SetCursorInvalid();
                    break;
                case ItemType.HoeTool:
                    if (currentTile.canDig) { SetCursorValid(); } else SetCursorInvalid();                   
                    break;
                case ItemType.WaterTool:
                    if (currentTile.daysSinceDig > -1 && currentTile.daysSinceWatered == -1)
                        SetCursorValid();
                    else SetCursorInvalid();
                        break;
                case ItemType.BreakTool:
                case ItemType.ChopTool://在小树苗的时候斧头是不能用的
                    if (crop != null)
                    {
                        if (crop.CanHarvest && crop.cropDetails.CheckToolAailable(currentItem.itemID))//如果是可以收割的
                            SetCursorValid();//如果可以收割，就让它可以被点击（排除小树苗阶段）
                        else SetCursorInvalid();
                    }
                    else SetCursorInvalid();
                    break;
                case ItemType.CollectTool:
                    if (currentCrop != null)
                    {
                        if(currentCrop.CheckToolAailable(currentItem.itemID))//如果当前的工具可用
                        if (currentTile.growthDays >= currentCrop.TotalGrowthDays)
                            SetCursorValid();
                        //如果说当前的种子种植天数 > 成长总天数，设置成可点击
                        else SetCursorInvalid();
                    }                    
                    else
                        SetCursorInvalid();
                    break;

                case ItemType.ReapTool:
                    if (GridMapManager.Instance.HavePeapableItemsInRadius(mouseWorldPos, currentItem))
                        SetCursorValid();
                    else SetCursorInvalid();                   
                    break;

                case ItemType.Furniture:
                   
                    buildImage.gameObject.SetActive(true);

                    var bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBlueprintDetails(currentItem.itemID);
                    if (currentTile.canPlacedFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID)
                        && !HaveFurnitureInRadius(bluePrintDetails))
                        SetCursorValid();
                    else SetCursorInvalid();
                    break;
            }            
        }
        else
        {
            SetCursorInvalid();            
        }       
        //Debug.Log("WorldPos:"+mouseWorldPos + "   GridPos:"  + mouseGridPos);
    }

    private bool HaveFurnitureInRadius(BluePrintDetails bluePrintDetails)
    {
        var buildItem = bluePrintDetails.buildPrefab;
        Vector2 point = mouseWorldPos;
        var size = buildItem.GetComponent<BoxCollider2D>().size;

        var otherColl = Physics2D.OverlapBox(point, size, 0);
        
        if(otherColl != null)
            return otherColl.GetComponent<Furniture>();
        return false;
    }

    public Vector3 ChangeToGridPosition(Vector3 mouseWorldPos)
    {
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        int gridSize = 1;        
       
        Vector3 ToGrid = new Vector3(
            Mathf.Round(mouseWorldPos.x / gridSize) * gridSize,
            Mathf.Round(mouseWorldPos.y / gridSize) * gridSize,
            Mathf.Round(mouseWorldPos.z / gridSize) * gridSize
        );

        return ToGrid;
    }



    /// <summary>
    /// 是否与UI互动
    /// </summary>
    /// <returns></returns>
    private bool InteractWithUI()
    {
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        currentGrid = FindObjectOfType<Grid>();
    }

    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
    }


}

