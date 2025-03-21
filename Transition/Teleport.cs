using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mfarm.Transition
{
    public class Teleport : MonoBehaviour
    {
        [SceneName]
        public string sceneToGo;//要传送到的场景
        public Vector3 positionToGo;//另一个场景的位置

        private void OnTriggerEnter2D(Collider2D other)//如果碰到了GameObject
        {
            if(other.CompareTag("Player"))//看看这个鬼东西是不是NPC
            {
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);//调用事件
            }
        }
    }
}
