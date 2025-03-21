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
    private SortedSet<ScheduleDetails> scheduleSet;//内置了优先级
    private ScheduleDetails currentSchedule;
    public AnimationClip blankAnimationClip;
    private AnimatorOverrideController animOverride;

    //临时存储信息
    public string currentScene;//需要知道现在的场景
    private string targetScene;

    private Vector3Int currentGridPosition;
    private Vector3Int targetGridPosition;
    private Vector3Int nextGridPosition;
    private Vector3 nextWorldPosition;
    public string StartScene{ set => currentScene = value; }


    [Header("移动属性")]
    public float normalSpeed = 2f;
    private float minSpeed = 1;
    private float maxSpeed = 3;
    private Vector2 dir;//判断动画的移动方向

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

    //动画计时器
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

        //一整个增加overrideController的大动作
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

        //计时器
        animationBreakTime -= Time.deltaTime;//用自己定的计时器一直倒计时（帧时间）
        canPlayStopAnimation = animationBreakTime <= 0;//倒计时技术，就播放 伸懒腰
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
    /// 基于同场景判断NPC是否显示
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
        //影子关闭
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;
        //影子关闭
         transform.GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// NPC主要移动方法
    /// </summary>
    private void Movement()
    {
        if (!npcMove)
        {
            //若堆栈计数>0,说明已经有数据
            if (movementStep.Count > 0)
            {
                //取一个出来并使用,也就是下一个格子
                MovementStep step = movementStep.Pop();

                currentScene = step.sceneName;
                CheckVisiable();//看看角色显不显示

                //第一个已经是 startPostion之后的下一个了 
                nextGridPosition = (Vector3Int)step.gridCoordinate;
                //新建一个时间戳，把栈里面记录的 当前要走的这一步的时间戳给临时变量
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
        nextWorldPosition = GetWorldPosition(gridPos);//用“将要行动的nextGridPosition”获取nextWorldPosition
        
        //还有时间用来移动
        //如果存在下一格中的时间 > 现在的游戏时间 ，说明行动过去时间足够
        if(stepTime > GameTime)
        {
            //能用来移动的时间：时间戳记录的时间 - 现在的游戏时间
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            //现在的位置，目的地的位置——距离
            float distance = Vector3.Distance(transform.position, nextWorldPosition);

            //实际的移动速度
            float speed = Mathf.Max(minSpeed, (distance) / timeToMove / Settings.secondThreshold);
            
            if(speed <= maxSpeed)//如果速度小于最大速度
            {
                
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                {
                    //给一个移动方向
                    dir = (nextWorldPosition - transform.position).normalized;

                    //移动量！从每秒转换为每帧，计算移动偏移量
                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);
                    yield return new WaitForFixedUpdate();//执行结束后等待下一次fixed方法再次执行
                }
            }
        }
        //如果时间过了就瞬间移动
        rb.position = nextWorldPosition;//世界坐标
        currentGridPosition = gridPos;//将当前的坐标更新世界坐标
        nextGridPosition = currentGridPosition;//表示物体暂时没有移动目标或是目标已经移动完成
        //transform.position = nextWorldPosition;//更新物体的渲染位置

        npcMove = false;
    }

    private void InitNPC()
    {
        targetScene = currentScene;//记录一下targetScene的状态 说明现在的场景已经是目标场景了
        currentGridPosition = grid.WorldToCell(transform.position);//通过世界地图投影到网格地图，返回现在人物再网格地图的位置

        //挪0.5，挪到中心点上
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2F, currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        //把挪了以后的位置给最终位置
        targetGridPosition = currentGridPosition;       
    }

    /// <summary>
    /// 根据schedule构建路径
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule)
    {
        movementStep.Clear();//每次执行代表创建一个新的路径,先清空一下
        currentSchedule = schedule;//临时传入的参数赋值给现在的schedule
        targetScene = schedule.targetScene;
        targetGridPosition =(Vector3Int) schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAtStop;
        this.interactable = schedule.interactable;

        //如果NPC携带的 schedule记载的场景是当前场景
        if(schedule.targetScene == currentScene)
        {
            //构建最短路径
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementStep);
        }
        //跨场景移动
        else if(schedule.targetScene != currentScene)//首先确定要移动的目的地是另一个场景
        {
            
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if(sceneRoute != null)//如果说“场记”拿到了（不为空）
            {
                //到 场记List里面寻找看看怎么个走法
                for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;//临时变量
                    ScenePath path = sceneRoute.scenePathList[i];//特殊固定点位

                    //如果说不符合特殊固定点位
                    if (path.fromGridCell.x >= Settings.maxGridSize)
                    {
                        //从当前的点走就行了
                        fromPos = (Vector2Int)currentGridPosition;
                    }
                    else//如果符合特殊点，就按照场记走
                        fromPos = path.fromGridCell;

                    if (path.gotoGridCell.x >= Settings.maxGridSize)
                        gotoPos = schedule.targetGridPosition;
                    else
                        gotoPos = path.gotoGridCell;

                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, movementStep);

                }
            }
        }
        
        if (movementStep.Count > 1)//如果堆栈里面有东西了（说明路径已经有了）
        {

            //更新每一步对应的时间戳
            updateTimePnPath();
        }
    }

    private void updateTimePnPath()
    {
        MovementStep previousStep = null;//每一步包含的一些信息的类

        TimeSpan currentGameTime = GameTime;//获取当前的游戏内时间——在TimeManager里面赋值过

        foreach (MovementStep step in movementStep)//从路径堆栈中逐一循环
        {
            //如果说上一步是空的，就说明这是第一步
            if(previousStep == null)
                previousStep = step;//堆栈的第一个给临时变量

            //第一个 已经生成的 堆栈里面的 step 先被赋值时间——用于后面的累计
            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMovementStepTime;//再次创建一个时间管理器

            if(MoveInDiagonal(step,previousStep))//如果是斜方向
                //移动每一步的时间（斜方向）
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            else
                //移动每一步的时间（正方向）
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));

            //累加获得下一步的时间戳
            currentGameTime = currentGameTime.Add(gridMovementStepTime); 
            //把现在的这一步给 下一个的 “上一步”
            previousStep = step;
        }
    }

    /// <summary>
    /// 判断是否是斜方向
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="previousStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep,MovementStep previousStep)
    {
        //如果x ！= x；y ！= y，则说明是斜方向
        return ((currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y));
    }

    /// <summary>
    /// 网格坐标返回世界坐标中心点
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
        //强制面向下        
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);

        animationBreakTime = Settings.animationBreakTime;
        if(stopAnimationClip != null)
        {           
            //stopAnimationClip是手动赋值的
            animOverride[blankAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);//打一个哈欠
            yield return null;//返回携程
            anim.SetBool("EventAnimation", false);//设置false
        }
        else
        {           
            //前面已经创建过一个overrideController了 现在赋值
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
            //实例ID找到唯一动画 
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
