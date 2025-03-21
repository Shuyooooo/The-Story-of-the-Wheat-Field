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

           //���fade���滹Ҫ��
           fadeCanvasGroup = FindObjectOfType<CanvasGroup>();//��ʼʱ�ҵ����GameObject                    
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
        /// �����л�
        /// </summary>
        /// <param name="sceneName">Ŀ�곡��</param>
        /// <param name="targetPosition">Ŀ��λ��</param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            //�л�������Ҫ�ѵ�ǰ����ĳ�����ж��
            yield return Fade(1);

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());          

            //Ȼ��ִ�����볡���ĺ���
            yield return LoadSceneSetActive(sceneName);

            //�ƶ���������
            EventHandler.CallMoveToPosition(targetPosition);
            

            //�Ķ�������ԭ����start()����������
            //�����ڡ���Ҫ�ҵĵط���item information��
            EventHandler.CallAfterSceneLoadedEvent();

            yield return Fade(0);
        }

        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);//����һ������

            //��ȡ��Ϸ�е�ǰ�Ѿ����ص�ȫ�������б��ܳ�����Ҫ-1��
            //���������5����������ôGetSceneAt(5-1)�������ǵ�ǰ����ĳ������
            //
            UnityEngine.SceneManagement.Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            //��GetSceneAt��int SceneNumber������
            SceneManager.SetActiveScene(newScene);
        }

        /// <summary>
        /// ���뵭������
        /// </summary>
        /// <param name="targetAlpha">1�Ǻڣ�0��͸��</param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;//����boolֵ

            fadeCanvasGroup.blocksRaycasts = true;//�ڵ��뵭���Ĺ����У���ֹ����ѡ

            //������alphaֵ-Ŀ��alphaֵ��/ ʱ�� = ����
            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.TransitionDuration;

            //�� ����alohaֵ Լ���� Ŀ�� alphaֵ ʱ
            while(!Mathf.Approximately(fadeCanvasGroup.alpha,targetAlpha))
            {
                //��alphaֵ�������ʸı�
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                //������һ��
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
