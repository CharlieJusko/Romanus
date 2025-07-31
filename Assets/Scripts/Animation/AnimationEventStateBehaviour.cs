using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EnhancedAnimationSystem
{
    public class AnimationEventStateBehaviour : StateMachineBehaviour
    {
        public string eventName;
        [Range(0f, 1f)]
        public float triggerTime;
        public bool onStart = false;
        public bool onExit = false;

        bool hasTriggered = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //base.OnStateEnter(animator, stateInfo, layerIndex);
            hasTriggered = false;
            if(onStart)
            {
                SendAnimationEvent(animator);
                hasTriggered = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //base.OnStateExit(animator, stateInfo, layerIndex);
            if(!hasTriggered && onExit)
            {
                SendAnimationEvent(animator);
                hasTriggered = true;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float currentTime = stateInfo.normalizedTime % 1;
            if(!hasTriggered && !onExit && currentTime >= triggerTime)
            {
                SendAnimationEvent(animator);
                hasTriggered = true;
            }
        }

        void SendAnimationEvent(Animator animator)
        {
            AnimationEventReceiver receiver = animator.GetComponent<AnimationEventReceiver>();
            if(receiver != null )
            {
                receiver.OnAnimationEventTriggered(eventName);
            }
            else if(animator.tag == "Player")
            {
                receiver = animator.GetComponent<Player>().blessing.GetComponent<AnimationEventReceiver>();
                if(receiver != null)
                {
                    receiver.OnAnimationEventTriggered(eventName);
                }
            }
            else
            {
                receiver = animator.GetComponentInParent<AnimationEventReceiver>();
                receiver.OnAnimationEventTriggered(eventName);
            }
        }
    }
}
