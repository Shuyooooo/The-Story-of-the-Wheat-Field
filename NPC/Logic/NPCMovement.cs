using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.AStar;
using UnityEngine.SceneManagement;
using System;
using Mfarm.Save;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(Animator))]
public class NPCMovement : MonoBehaviour,Isavable
{
    public ScheduleDataList_SO scheduleData;
    private SortedSet<ScheduleDetails> scheduleSet;//���������ȼ�
    private ScheduleDetails currentSchedule;
    public AnimationClip blankAnimationClip;
    private AnimatorOverrideController animOverride;

    //��ʱ�洢��Ϣ
    public string currentScene;//��Ҫ֪�����ڵĳ���
    private string targetScene;

    private Vector3Int currentGridPosition;
    private Vector3Int targetGridPosition;
    private Vector3Int nextGridPosition;
    private Vector3 nextWorldPosition;
    public string StartScene{ set => currentScene = value; }


    [Header("�ƶ�����")]
    public float normalSpeed = 2f;
    private float minSpeed = 1;
    private float maxSpeed = 3;
    private Vector2 dir;//�ж϶������ƶ�����

    public bool isMoving;
    private bool npcMove;
    private bool sceneLoaded;
    public bool interactable;
    private Season currentSeason;

    //Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    private Animator anim;
    private Stack<MovementStep> movementStep;
    private Grid grid;
    private Coroutine npcMoveRoutine;

    private bool isInitialised;
    public bool isFirstLoad;

    //������ʱ��
    private float animationBreakTime;
    private bool canPlayStopAnimation;
    private AnimationClip stopAnimationClip;

    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementStep = new Stack<MovementStep>();

        //һ��������overrideController�Ĵ���
        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;
        scheduleSet = new SortedSet<ScheduleDetails>();

        foreach(var schedule in scheduleData.schedulesList)
        {
            scheduleSet.Add(schedule);
            //Debug.Log(scheduleSet.Count);
        }
        
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;

        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;

        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

  
    private void Start()
    {
        Isavable savable = this;
        savable.RegisterSavable();       
    }

    private void Update()
    {
        if (sceneLoaded)
            SwitchAnimation();

        //��ʱ��
        animationBreakTime -= Time.deltaTime;//���Լ����ļ�ʱ��һֱ����ʱ��֡ʱ�䣩
        canPlayStopAnimation = animationBreakTime <= 0;//����ʱ�������Ͳ��� ������
    }

    private void FixedUpdate()
    {
        if(sceneLoaded)
            Movement();
    }

    private void OnStartNewGameEvent(int obj)
    {
        isInitialised = false;
        isFirstLoad = true;
    }


    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;
        if(npcMoveRoutine != null)
        {
            StopCoroutine(npcMoveRoutine);
        }
    }

    private void OnGameMinuteEvent(int minute, int hour,int day, Season season)
    {
        int time = (hour * 100) + minute;
        currentSeason = season;

        ScheduleDetails matchSchedule = null;
        foreach(var schedule in scheduleSet)
        {
            if(schedule.Time == time)
            {
                if (schedule.day != day && schedule.day != 0)
                    continue;
                if (schedule.season != season)
                    continue;
                matchSchedule = schedule;
            }
            else if (schedule.Time > time)
            {
                break;
            }
            if (matchSchedule != null)
                BuildPath(matchSchedule);
            
        }
    }


    private void OnBeforeSceneUnloadEvent()
    {
        sceneLoaded = false;
    }

    private void OnAfterSceneLoadedEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisiable();

        if (!isInitialised)
        {
            InitNPC();
            isInitialised = true;
        }
        sceneLoaded = true;

        if(!isFirstLoad)
        {
            currentGridPosition = grid.WorldToCell(transform.position);
            var schedule = new ScheduleDetails(0, 0, 0, 0, currentSeason,targetScene,(Vector2Int)targetGridPosition,stopAnimationClip , interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
    }

    /// <summary>
    /// ����ͬ�����ж�NPC�Ƿ���ʾ
    /// </summary>
    private void CheckVisiable()
    {
        if (currentScene == SceneManager.GetActiveScene().name)
            SetActiveInScene();
        else SetInactiveInScene();
    }

    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;
        //Ӱ�ӹر�
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;
        //Ӱ�ӹر�
         transform.GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// NPC��Ҫ�ƶ�����
    /// </summary>
    private void Movement()
    {
        if (!npcMove)
        {
            //����ջ����>0,˵���Ѿ�������
            if (movementStep.Count > 0)
            {
                //ȡһ��������ʹ��,Ҳ������һ������
                MovementStep step = movementStep.Pop();

                currentScene = step.sceneName;
                CheckVisiable();//������ɫ�Բ���ʾ

                //��һ���Ѿ��� startPostion֮�����һ���� 
                nextGridPosition = (Vector3Int)step.gridCoordinate;
                //�½�һ��ʱ�������ջ�����¼�� ��ǰҪ�ߵ���һ����ʱ�������ʱ����
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(nextGridPosition,stepTime);
            }   
            else if (!isMoving && canPlayStopAnimation)
            {
                StartCoroutine(SetStopAnimation());
            }
        }
    }

    private void MoveToGridPosition(Vector3Int gridPos,TimeSpan stepTime)
    {
        npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }
    
    private IEnumerator MoveRoutine(Vector3Int gridPos,TimeSpan stepTime)
    {
        npcMove = true;
        nextWorldPosition = GetWorldPosition(gridPos);//�á���Ҫ�ж���nextGridPosition����ȡnextWorldPosition
        
        //����ʱ�������ƶ�
        //���������һ���е�ʱ�� > ���ڵ���Ϸʱ�� ��˵���ж���ȥʱ���㹻
        if(stepTime > GameTime)
        {
            //�������ƶ���ʱ�䣺ʱ�����¼��ʱ�� - ���ڵ���Ϸʱ��
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            //���ڵ�λ�ã�Ŀ�ĵص�λ�á�������
            float distance = Vector3.Distance(transform.position, nextWorldPosition);

            //ʵ�ʵ��ƶ��ٶ�
            float speed = Mathf.Max(minSpeed, (distance) / timeToMove / Settings.secondThreshold);
            
            if(speed <= maxSpeed)//����ٶ�С������ٶ�
            {
                
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                {
                    //��һ���ƶ�����
                    dir = (nextWorldPosition - transform.position).normalized;

                    //�ƶ�������ÿ��ת��Ϊÿ֡�������ƶ�ƫ����
                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);
                    yield return new WaitForFixedUpdate();//ִ�н�����ȴ���һ��fixed�����ٴ�ִ��
                }
            }
        }
        //���ʱ����˾�˲���ƶ�
        rb.position = nextWorldPosition;//��������
        currentGridPosition = gridPos;//����ǰ�����������������
        nextGridPosition = currentGridPosition;//��ʾ������ʱû���ƶ�Ŀ�����Ŀ���Ѿ��ƶ����
        //transform.position = nextWorldPosition;//�����������Ⱦλ��

        npcMove = false;
    }

    private void InitNPC()
    {
        targetScene = currentScene;//��¼һ��targetScene��״̬ ˵�����ڵĳ����Ѿ���Ŀ�곡����
        currentGridPosition = grid.WorldToCell(transform.position);//ͨ�������ͼͶӰ�������ͼ���������������������ͼ��λ��

        //Ų0.5��Ų�����ĵ���
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2F, currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        //��Ų���Ժ��λ�ø�����λ��
        targetGridPosition = currentGridPosition;       
    }

    /// <summary>
    /// ����schedule����·��
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule)
    {
        movementStep.Clear();//ÿ��ִ�д�����һ���µ�·��,�����һ��
        currentSchedule = schedule;//��ʱ����Ĳ�����ֵ�����ڵ�schedule
        targetScene = schedule.targetScene;
        targetGridPosition =(Vector3Int) schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAtStop;
        this.interactable = schedule.interactable;

        //���NPCЯ���� schedule���صĳ����ǵ�ǰ����
        if(schedule.targetScene == currentScene)
        {
            //�������·��
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementStep);
        }
        //�糡���ƶ�
        else if(schedule.targetScene != currentScene)//����ȷ��Ҫ�ƶ���Ŀ�ĵ�����һ������
        {
            
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if(sceneRoute != null)//���˵�����ǡ��õ��ˣ���Ϊ�գ�
            {
                //�� ����List����Ѱ�ҿ�����ô���߷�
                for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;//��ʱ����
                    ScenePath path = sceneRoute.scenePathList[i];//����̶���λ

                    //���˵����������̶���λ
                    if (path.fromGridCell.x >= Settings.maxGridSize)
                    {
                        //�ӵ�ǰ�ĵ��߾�����
                        fromPos = (Vector2Int)currentGridPosition;
                    }
                    else//�����������㣬�Ͱ��ճ�����
                        fromPos = path.fromGridCell;

                    if (path.gotoGridCell.x >= Settings.maxGridSize)
                        gotoPos = schedule.targetGridPosition;
                    else
                        gotoPos = path.gotoGridCell;

                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, movementStep);

                }
            }
        }
        
        if (movementStep.Count > 1)//�����ջ�����ж����ˣ�˵��·���Ѿ����ˣ�
        {

            //����ÿһ����Ӧ��ʱ���
            updateTimePnPath();
        }
    }

    private void updateTimePnPath()
    {
        MovementStep previousStep = null;//ÿһ��������һЩ��Ϣ����

        TimeSpan currentGameTime = GameTime;//��ȡ��ǰ����Ϸ��ʱ�䡪����TimeManager���渳ֵ��

        foreach (MovementStep step in movementStep)//��·����ջ����һѭ��
        {
            //���˵��һ���ǿյģ���˵�����ǵ�һ��
            if(previousStep == null)
                previousStep = step;//��ջ�ĵ�һ������ʱ����

            //��һ�� �Ѿ����ɵ� ��ջ����� step �ȱ���ֵʱ�䡪�����ں�����ۼ�
            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMovementStepTime;//�ٴδ���һ��ʱ�������

            if(MoveInDiagonal(step,previousStep))//�����б����
                //�ƶ�ÿһ����ʱ�䣨б����
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            else
                //�ƶ�ÿһ����ʱ�䣨������
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));

            //�ۼӻ����һ����ʱ���
            currentGameTime = currentGameTime.Add(gridMovementStepTime); 
            //�����ڵ���һ���� ��һ���� ����һ����
            previousStep = step;
        }
    }

    /// <summary>
    /// �ж��Ƿ���б����
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="previousStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep,MovementStep previousStep)
    {
        //���x ��= x��y ��= y����˵����б����
        return ((currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y));
    }

    /// <summary>
    /// �������귵�������������ĵ�
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        Vector3 worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2);
    }

    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPosition(targetGridPosition);

        anim.SetBool("isMoving", isMoving);
        if(isMoving)
        {
            anim.SetBool("Exit", true);
            anim.SetFloat("DirX", dir.x);
            anim.SetFloat("DirY", dir.y);
        }
        else
        {
            anim.SetBool("Exit", false);
        }
        
    }

    private IEnumerator SetStopAnimation()
    {
        //ǿ��������        
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);

        animationBreakTime = Settings.animationBreakTime;
        if(stopAnimationClip != null)
        {           
            //stopAnimationClip���ֶ���ֵ��
            animOverride[blankAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);//��һ����Ƿ
            yield return null;//����Я��
            anim.SetBool("EventAnimation", false);//����false
        }
        else
        {           
            //ǰ���Ѿ�������һ��overrideController�� ���ڸ�ֵ
            animOverride[stopAnimationClip] = blankAnimationClip;
            anim.SetBool("EventAnimation", false);
        }
        
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add("targetGridPosition", new SerializableVector3(targetGridPosition));
        saveData.characterPosDict.Add("currentPosition", new SerializableVector3(transform.position));
        saveData.dataSceneName = currentScene;
        saveData.targetScene = this.targetScene;
        if(stopAnimationClip != null)
        {
            //ʵ��ID�ҵ�Ψһ���� 
            saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
        }
        saveData.interactable = this.interactable;
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)currentSeason);
        return saveData;
    }
    

    public void RestoreData(GameSaveData saveData)
    {
        isInitialised = true;
        isFirstLoad = false;

        currentScene = saveData.dataSceneName;
        targetScene = saveData.targetScene;

        Vector3 pos = saveData.characterPosDict["currentPosition"].ToVector3();       
        Vector3Int gridPos = (Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int();

        transform.position = pos;
        targetGridPosition = gridPos;

        if(saveData.animationInstanceID != 0)
        {
            this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }

        this.interactable = saveData.interactable;
        this.currentSeason = (Season)saveData.timeDict["currentSeason"];
    }
}
