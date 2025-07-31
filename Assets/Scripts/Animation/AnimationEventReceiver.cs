using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    [SerializeField] List<EnhancedAnimationSystem.AnimationEvent> events = new List<EnhancedAnimationSystem.AnimationEvent>();

    public void OnAnimationEventTriggered(string name)
    {
        EnhancedAnimationSystem.AnimationEvent matchingEvent = events.Find(ae => ae.eventName == name);
        matchingEvent?.OnAnimationEvent?.Invoke();
    }
}
