using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.Transition
{
    public class Teleport : MonoBehaviour
    {
        [SceneName]
        public string sceneToGo;//Ҫ���͵��ĳ���
        public Vector3 positionToGo;//��һ��������λ��

        private void OnTriggerEnter2D(Collider2D other)//���������GameObject
        {
            if(other.CompareTag("Player"))//������������ǲ���NPC
            {
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);//�����¼�
            }
        }
    }
}
