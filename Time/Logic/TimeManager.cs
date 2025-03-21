using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mfarm.Save;

public class TimeManager : Singleton<TimeManager>,Isavable
{
    private int gameSecond, gameMinute,gameHour,gameDay,gameMonth,gameYear;
    private Season gameSeason = Season.Spring;
    private int monthInSeason = 3;

    public bool gameClockPause;
    private float tikTime;

    private float timeDifference;
    public TimeSpan GameTime => new TimeSpan(gameHour,gameMinute,gameSecond);

    public string GUID => GetComponent<DataGUID>().guid;

    
    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;


    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

   
    private void Start()
    {
        Isavable savable = this;
        savable.RegisterSavable();
        gameClockPause = true;      
        //切换灯光
        //EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }

    private void Update()
    {
        if (!gameClockPause)//如果游戏没有暂停
        {
            tikTime += Time.deltaTime;//计时器就一直走
            if (tikTime >= Settings.secondThreshold)//如果秒数>秒数的阈值
            {
                tikTime -= Settings.secondThreshold;//秒数变化
                UpdateGameTime();
            }
        }

        if (Input.GetKey(KeyCode.T))
        {
            for (int i = 0; i < 60; i++)
            {
                UpdateGameTime();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            gameDay++;
            EventHandler.CallGameDayEvent(gameDay, gameSeason);
            EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        }
    }

    private void OnEndGameEvent()
    {
        gameClockPause = true;
    }


    private void OnStartNewGameEvent(int obj)
    {
        NewGameTime();
        //EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        //EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);        
        gameClockPause = false;
    }

    private void OnUpdateGameStateEvent(GameState currentGameState)
    {
        gameClockPause = currentGameState == GameState.Pause;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        gameClockPause = true;
    }

    private void OnAfterSceneLoadedEvent()
    {
        gameClockPause = false; 
        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);

    }


   

    //初始化所有时间变量
    private void NewGameTime()
    {
        gameSecond = 0;
        gameMinute = 30;
        gameHour = 7;
        gameDay = 12;
        gameMonth = 06;
        gameYear = 2016;
        gameSeason = Season.Summer;        

    }

    private void UpdateGameTime()
    {
        gameSecond++;
        if(gameSecond > Settings.secondHold)
        {
            gameMinute++;
            gameSecond = 0;

            if(gameMinute > Settings.minuteHold)
            {
                gameHour++;
                gameMinute = 0;
                
                if (gameHour > Settings.hourHold)
                {
                    gameDay++;
                    gameHour = 0;

                    if(gameDay > Settings.dayHold)
                    {
                        gameDay = 1;//如果天数到30天了，那么就把月份加1（到下一个月）
                        gameMonth++;
                        
                        if(gameMonth > 12)
                            gameMonth = 1;

                        monthInSeason--;//如果season计算到0，就下一个季节
                        if(monthInSeason ==0)
                        {
                            monthInSeason = 3;

                            int seasonNumber = (int)gameSeason;//enum类型变量
                            seasonNumber++;

                            if(seasonNumber > Settings.seasonHold)
                            {
                                seasonNumber = 0;//序号
                                gameYear++;
                            }
                            gameSeason = (Season)seasonNumber;
                            if(gameYear > 9999)
                            {
                                gameYear = 2015;
                            }
                        }
                        //用来刷新地图和农作物生长
                        EventHandler.CallGameDayEvent(gameDay, gameSeason);
                    }                    
                }
                EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
            }
            EventHandler.CallGameMinuteEvent(gameMinute,gameHour, gameDay,gameSeason);//在这里面可以直接传了
                                                                                      ////切换灯光           
            EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);           
        }               
    }

    private LightShift GetCurrentLightShift()
    {
        if(GameTime >= Settings.MorningTime && GameTime < Settings.NightTime)
        {
            
            timeDifference =(float)(GameTime - Settings.MorningTime).TotalMinutes;            
            return LightShift.Morning;
        }

        if(GameTime >= Settings.NightTime || GameTime < Settings.MorningTime)
        {
            timeDifference =Mathf.Abs((float)(GameTime - Settings.NightTime).TotalMinutes);
            return LightShift.Night;
        }
        return LightShift.Morning;
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("gameYear", gameYear);
        saveData.timeDict.Add("gameSeason", (int)gameSeason);
        saveData.timeDict.Add("gameMonth", gameMonth);
        saveData.timeDict.Add("gameDay", gameDay);
        saveData.timeDict.Add("gameHour", gameHour);
        saveData.timeDict.Add("gameMinute", gameMinute);
        saveData.timeDict.Add("gameSecond", gameSecond);

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        gameYear = saveData.timeDict["gameYear"];
        gameSeason = (Season)saveData.timeDict["gameSeason"];
        gameMonth = saveData.timeDict["gameMonth"];
        gameDay = saveData.timeDict["gameDay"];
        gameHour = saveData.timeDict["gameHour"];
        gameMinute = saveData.timeDict["gameMinute"];
        gameSecond = saveData.timeDict["gameSecond"];
    }
}
