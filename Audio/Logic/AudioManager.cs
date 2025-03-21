using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    [Header("音乐数据库")]
    public SoundDetailsList_SO soundDetailsData;
    public SceneSoundList_SO sceneSoundData;

    [Header("Audio Source")]
    public AudioSource ambientMusicSource;
    public AudioSource gameMusicSource;

    private Coroutine soundRoutine;

    [Header("AudioMixer")]
    public AudioMixer audioMixer;

    [Header("Snapshots")]
    public AudioMixerSnapshot normalSnapShot;
    public AudioMixerSnapshot ambientSnapShot;
    public AudioMixerSnapshot muteSnapShot;
    private float musicTransitionSecond = 8f;

    //播放环境音的随机等待时间
    public float MusicStartSeconds => Random.Range(5f, 15f);

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent += OnPlaySoundEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        if (soundRoutine != null)
            StopCoroutine(soundRoutine);
        muteSnapShot.TransitionTo(1f);
    }

    private void OnPlaySoundEvent(SoundName soundName)
    {
        var soundDetails=soundDetailsData.GetSoundDetails(soundName);
        if(soundDetails != null)
        {
            EventHandler.CallInitSoundEffect(soundDetails);
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        
        string currentSceneName = SceneManager.GetActiveScene().name;

        //通过活跃场景名字找到场景声音列表
        SceneSoundItem sceneSound= sceneSoundData.GetSceneSoundItem(currentSceneName);
        if (sceneSound == null)
            return;
        //Debug.Log(soundDetailsData.GetSoundDetails(sceneSound.AmbientMusic));
        SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSound.AmbientMusic);
        SoundDetails GameMusic = soundDetailsData.GetSoundDetails(sceneSound.GameMusic);
              
        if(soundRoutine != null)
            StopCoroutine(soundRoutine);
        soundRoutine = StartCoroutine(PlaySoundRoutine(GameMusic, ambient));
    }

    private IEnumerator PlaySoundRoutine(SoundDetails GameMusic ,SoundDetails AmbientMusic)
    {
        if(GameMusic != null && AmbientMusic != null)
        {
            PlayAmbientMusicClip(AmbientMusic, 1f);
            yield return new WaitForSeconds(MusicStartSeconds);            
            PlayGameMusicClip(GameMusic,musicTransitionSecond);
        }
    }

    private void PlayGameMusicClip(SoundDetails GameMusic,float transitionTime)
    {
        audioMixer.SetFloat("GameMusicVolume", ConvertSoundVolume(GameMusic.soundVolume));
        gameMusicSource.clip = GameMusic.soundClip;
        if(gameMusicSource.isActiveAndEnabled)
            gameMusicSource.Play();

        //开场时的音效是过渡的 增强的
        normalSnapShot.TransitionTo(transitionTime);
    }

    private void PlayAmbientMusicClip(SoundDetails AmbientMusic, float transitionTime)
    {
        audioMixer.SetFloat("GameMusicVolume", ConvertSoundVolume(AmbientMusic.soundVolume));
        ambientMusicSource.clip = AmbientMusic.soundClip;
        if (ambientMusicSource.isActiveAndEnabled)
            ambientMusicSource.Play();

        ambientSnapShot.TransitionTo(musicTransitionSecond);        
    }

    private float ConvertSoundVolume(float amount)
    {
        return (amount * 100 - 80);
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", (value * 100 - 80));
    }
}
