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

    //��һ��ˮƽ�����ʹ�ֱ�����ϳ�һ������
    //�Ҳ�֪���Ƿ�������Ϊ����������float�����ġ�������
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
        //�����ӿ����ͣ����ڵ��õĽӿڱ���
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
        if (!inputDisable)//�����������ʱ��
            PlayerInput();//�����ƶ�����
        else
            isMoving = false;//����ֹ�ƶ�����ʱ
        SwitchAnimation();//�л�����
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
       
        //���Ŷ���
        if(itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity
            && itemDetails.itemType != ItemType.Furniture)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - transform.position.y - 0.85f;

            //����б����
            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))//���˵X���ƫ�����Դ���Y
                mouseY = 0;//����Ϊ��X�᷽���ϵ�animator
            else
                mouseX = 0;    
            //ȷ�����Ŷ����ķ���

            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
        }
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
        
    }

    //Э�̣�����ִ�У�
    private IEnumerator UseToolRoutine(Vector3 mouseWorlrdPos,ItemDetails itemDetails)
    {
        UseTool = true;//���ж�ֵΪ��
        inputDisable = true;//������һ�߿���һ���ܣ�
        yield return null;
        foreach(var anim in animators)
        {
            anim.SetTrigger("Use Tool");
            //������泯����
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
        yield return new WaitForSeconds(0.45f);//��һ������ö�������
        EventHandler.CallExecuteActionAfterAnimation(mouseWorlrdPos, itemDetails);    
        yield return new WaitForSeconds(0.25f);
        //�ȴ���������
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

        //��·�ٶ�
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
        Vector2 move = player.position;//�����ڵ����괫��һ��1ά����
        move.x = move.x + inputX * speed * Time.deltaTime;
        move.y = move.y + inputY * speed * Time.deltaTime;

        //��һ�����ƶ������Ĺؼ���Ϊʲô�� һ��ʼ��Ҳ��֪��
        //����
        //
        player.MovePosition(move); //�Ѷ����ƶ����µ�λ��      
    }

    private void FixedUpdate()
    {
        if(!inputDisable)//����������У������л�ʱ�������һֱ��
            PlayerMovement();
    }

  

    private void SwitchAnimation()
    {
        foreach(var anim in animators)//ѭ��ÿһ��animator
        {
            anim.SetBool("isMoving", isMoving);//��isMoving��ֵ������isMoving��ǩ��
            anim.SetFloat("MouseX", mouseX);//ͬ��
            anim.SetFloat("MouseY", mouseY);
            if (isMoving)//���û��û�ж���isMoving��= 0��
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
