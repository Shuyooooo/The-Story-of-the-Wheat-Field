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
    public Sprite normal, tool, seed,commodity;//��ק��ֵ

    private Sprite currentSprite;//�洢��ǰ���ͼƬ
    private Image cursorImage;

    //����ͼ�����
    private Image buildImage;

    private RectTransform cursorCanvas;
    private Camera mainCamera;//����Ļ����ת��Ϊ���������������
    private Grid currentGrid;//����������ת��Ϊ���������������

    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;

    private bool cursorEnable;

    private bool cursorPositionValid;

    private ItemDetails currentItem;
    private Transform PlayerTransform => FindObjectOfType<Player>().transform;//����player�ű���gameobject��transform
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
        //UI���������RectTransform,���ݱ�ǩ�ҵ�������
        cursorCanvas = GameObject.FindGameObjectWithTag("Cursor Canvas").GetComponent<RectTransform>();
        //�ҵ�������ĵ�һ��������
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();

        //�õ�����ͼ��       
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
            buildImage.gameObject.SetActive(false);//����һ��Ҫ��û�л����͹ر�
        }
    }

    private void CheckPlayerInput()
    {
        if(Input.GetMouseButtonDown(0) && cursorPositionValid)
        {
            //һ������
            EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
        }
    }

    #region ���������ʽ
    /// <summary>
    /// �������ͼƬ
    /// </summary>
    /// <param name="sprite"></param>
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);//���ó�ʼͼƬ��ȫ��ʾ
    }

    /// <summary>
    /// ����������
    /// </summary>
    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
        buildImage.color = new Color(1, 1, 1, 0.6f);
    }

    /// <summary>
    /// ������겻����
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
            //�﷨������
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

            //�����ѡ���ǼҾ�,��ʾͼƬ
            if(itemDetails.itemType == ItemType.Furniture)
            {                                
                buildImage.gameObject.SetActive(true);//��ʾ�Ҿ�ͼ��
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                buildImage.SetNativeSize();
                buildImage.rectTransform.localScale = new Vector3(0.22f, 0.22f, 1f);
                
            }
        }       
    }

    

    //WORKFLOW:
    /// <summary>
    /// ����Ļ����ת��Ϊ��������
    /// </summary>
    private void CheckCursorValid()
    {
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,-mainCamera.transform.position.z));
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

        var playerGridPos = currentGrid.WorldToCell(PlayerTransform.position);

        //����ͼƬ�����ƶ�
        buildImage.rectTransform.position = Input.mousePosition;
        

        //�ж���ʹ�÷�Χ��
        if(Mathf.Abs(mouseGridPos.x - playerGridPos.x) > currentItem.itemUseRadius 
            ||Mathf.Abs( mouseGridPos.y - playerGridPos.y) > currentItem.itemUseRadius)
        {
            SetCursorInvalid();          
            return;
        }
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
                
        if (currentTile != null)
        {
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);//д���ĺ���
            crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);//������һ�µ��crop��object
            switch (currentItem.itemType)
            {
                case ItemType.Seed:
                    //������������Ƭ�Ѿ��ڿ��ˣ�������IDΪ�գ�δ���֣�������ʾ�ɲ���
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
                case ItemType.ChopTool://��С�����ʱ��ͷ�ǲ����õ�
                    if (crop != null)
                    {
                        if (crop.CanHarvest && crop.cropDetails.CheckToolAailable(currentItem.itemID))//����ǿ����ո��
                            SetCursorValid();//��������ո���������Ա�������ų�С����׶Σ�
                        else SetCursorInvalid();
                    }
                    else SetCursorInvalid();
                    break;
                case ItemType.CollectTool:
                    if (currentCrop != null)
                    {
                        if(currentCrop.CheckToolAailable(currentItem.itemID))//�����ǰ�Ĺ��߿���
                        if (currentTile.growthDays >= currentCrop.TotalGrowthDays)
                            SetCursorValid();
                        //���˵��ǰ��������ֲ���� > �ɳ������������óɿɵ��
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
    /// �Ƿ���UI����
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

