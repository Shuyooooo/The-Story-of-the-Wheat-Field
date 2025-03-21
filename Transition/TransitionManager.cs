using Mfarm.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Mfarm.Transition
{
    
    public class TransitionManager : Singleton<TransitionManager>,Isavable
    {
        [SceneName]
        public string startSceneName = string.Empty;
        private CanvasGroup fadeCanvasGroup;
        private bool isFade;

        public string GUID => GetComponent<DataGUID>().guid;

        protected override void Awake()
        {
            base.Awake();
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }
        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

      

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }
              
        private void Start()
        {
           Isavable savable = this;
           savable.RegisterSavable();

           //这个fade后面还要用
           fadeCanvasGroup = FindObjectOfType<CanvasGroup>();//开始时找到这个GameObject                    
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private void OnTransitionEvent(string sceneToGo, Vector3 positionToGo)
        {
            if(!isFade)
                StartCoroutine(Transition(sceneToGo,positionToGo));
        }
        /// <summary>
        /// 场景切换
        /// </summary>
        /// <param name="sceneName">目标场景</param>
        /// <param name="targetPosition">目标位置</param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            //切换场景是要把当前激活的场景先卸载
            yield return Fade(1);

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());          

            //然后执行载入场景的函数
            yield return LoadSceneSetActive(sceneName);

            //移动人物坐标
            EventHandler.CallMoveToPosition(targetPosition);
            

            //改动后不是在原来的start()函数里面找
            //而是在“需要找的地方找item information”
            EventHandler.CallAfterSceneLoadedEvent();

            yield return Fade(0);
        }

        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);//叠加一个场景

            //获取游戏中当前已经加载的全部场景列表（总场景数要-1）
            //如果激活了5个场景，那么GetSceneAt(5-1)——就是当前激活的场景序号
            //
            UnityEngine.SceneManagement.Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            //把GetSceneAt（int SceneNumber）激活
            SceneManager.SetActiveScene(newScene);
        }

        /// <summary>
        /// 淡入淡出场景
        /// </summary>
        /// <param name="targetAlpha">1是黑，0是透明</param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;//激活bool值

            fadeCanvasGroup.blocksRaycasts = true;//在淡入淡出的过程中，禁止鼠标点选

            //（现有alpha值-目标alpha值）/ 时间 = 速率
            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.TransitionDuration;

            //当 现有aloha值 约等于 目标 alpha值 时
            while(!Mathf.Approximately(fadeCanvasGroup.alpha,targetAlpha))
            {
                //将alpha值按照速率改变
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                //进入下一步
                yield return null;
            }

            fadeCanvasGroup.blocksRaycasts = false;

            isFade = false;
        }

        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return Fade(1f);

            if(SceneManager.GetActiveScene().name != "PersistentScene")
            {
                EventHandler.CallBeforeSceneUnloadEvent();
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }

            yield return LoadSceneSetActive(sceneName);

            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0);
        }

        private IEnumerator UnloadScene()
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }


        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }
}
