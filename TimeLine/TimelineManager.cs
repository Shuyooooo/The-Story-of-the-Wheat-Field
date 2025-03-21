using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;
    private PlayableDirector currentDirector;

    private bool isDone;
    public bool IsDone { set => isDone = value; }
    private bool isPause;

    protected override void Awake()
    {
        base.Awake();
        currentDirector = startDirector;
    }

    private void OnEnable()
    {
        //currentDirector.played += TimelinePlayed;
        //currentDirector.stopped += TimelineStopped;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void OnAfterSceneLoadedEvent()
    {
        currentDirector = FindObjectOfType<PlayableDirector>();
        if (currentDirector != null)
            currentDirector.Play();
    }

    private void Update()
    {
        if (isPause && Input.GetKeyDown(KeyCode.Space) && isDone)
        {
            isPause = false;
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }

    /*private void TimelinePlayed(PlayableDirector director)
    {
        if(director != null)
        {
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        }
    }

    private void TimelineStopped(PlayableDirector director)
    {
        if (director != null)
        {
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
            director.gameObject.SetActive(false);
        }
    }*/


    public void PauseTimeline(PlayableDirector director)
    {
        currentDirector = director;
        //ÔÝÍ£·½·¨
        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
        isPause = true;
    }
}
