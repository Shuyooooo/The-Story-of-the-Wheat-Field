using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Mfarm.Save;


public class Player : MonoBehaviour,Isavable
{
    private Rigidbody2D player;

    public float speed = 4.0f;  
    private float inputX;
    private float inputY;

    //由一个水平变量和垂直变量合成一个方向
    //我不知道是否可以理解为包含了两个float变量的“合量”
    private Vector2 movementInput;

    private Animator[] animators;
    private bool isMoving;
    private bool inputDisable;
    private float mouseX;
    private float mouseY;
    private bool UseTool;

    public string GUID => GetComponent<DataGUID>().guid;



    // Start is called before the first frame update
    private void Awake()
    {
        player = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
        inputDisable = true;
    }

    private void Start()
    {
        //创建接口类型，等于调用的接口本身
        Isavable savable = this;
        savable.RegisterSavable();
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

   
    private void Update()
    {
        if (!inputDisable)//当可以输入的时候
            PlayerInput();//调用移动函数
        else
            isMoving = false;//当禁止移动函数时
        SwitchAnimation();//切换动画
    }

    private void OnEndGameEvent()
    {
        inputDisable = true;
    }


    private void OnStartNewGameEvent(int obj)
    {
        inputDisable = false;
        //transform.position = Settings.playerStartPos;
    }

    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch(gameState)
        {
            case GameState.Gameplay:
                inputDisable = false;
                break;
            case GameState.Pause:
                inputDisable = true;
                break;

        }
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos,ItemDetails itemDetails)
    {
        if (UseTool)
            return;
       
        //播放动画
        if(itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity
            && itemDetails.itemType != ItemType.Furniture)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - transform.position.y - 0.85f;

            //考虑斜方向
            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))//如果说X轴的偏移明显大于Y
                mouseY = 0;//就认为是X轴方向上的animator
            else
                mouseX = 0;    
            //确定播放动画的方向

            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
        }
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
        
    }

    //协程（辅助执行）
    private IEnumerator UseToolRoutine(Vector3 mouseWorlrdPos,ItemDetails itemDetails)
    {
        UseTool = true;//让判断值为真
        inputDisable = true;//不可以一边砍树一边跑；
        yield return null;
        foreach(var anim in animators)
        {
            anim.SetTrigger("Use Tool");
            //人物的面朝方向
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
        yield return new WaitForSeconds(0.45f);//等一会儿，让动画放完
        EventHandler.CallExecuteActionAfterAnimation(mouseWorlrdPos, itemDetails);    
        yield return new WaitForSeconds(0.25f);
        //等待动画结束
        UseTool = false;
        inputDisable = false;

        
    }
    private void OnMoveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    private void OnAfterSceneLoadedEvent()
    {
        inputDisable = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        inputDisable = true;
    }
  


    private void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        
        if (inputX != 0 && inputY != 0)
        {
            inputX = inputX * 0.7f;
            inputY = inputY * 0.7f;
        }

        //走路速度
        if(Input.GetKey(KeyCode.LeftShift))
        {
          
            inputX = inputX * 0.5f;
            inputY = inputY * 0.5f;
        }
        
            movementInput = new Vector2(inputX, inputY);

        isMoving = movementInput != Vector2.zero;

    }
    private void PlayerMovement()
    {
        Vector2 move = player.position;//把现在的坐标传给一个1维向量
        move.x = move.x + inputX * speed * Time.deltaTime;
        move.y = move.y + inputY * speed * Time.deltaTime;

        //这一句是移动起来的关键，为什么呢 一开始我也不知道
        //但是
        //
        player.MovePosition(move); //把对象移动到新的位置      
    }

    private void FixedUpdate()
    {
        if(!inputDisable)//如果不加这行，场景切换时，人物就一直跑
            PlayerMovement();
    }

  

    private void SwitchAnimation()
    {
        foreach(var anim in animators)//循环每一个animator
        {
            anim.SetBool("isMoving", isMoving);//用isMoving的值给带有isMoving标签的
            anim.SetFloat("MouseX", mouseX);//同上
            anim.SetFloat("MouseY", mouseY);
            if (isMoving)//如果没有没有动（isMoving！= 0）
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }
        }

    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(this.name, new SerializableVector3(transform.position));

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        var targetPosition = saveData.characterPosDict[this.name].ToVector3();

        transform.position = targetPosition;
    }
}
