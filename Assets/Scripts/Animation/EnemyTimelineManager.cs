using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTimelineManager : MonoBehaviour
{
    [SerializeField] Animator targetAnimator;
    public bool triggerCutsceneAnimaiton;
    public string cutsceneAnimaitonTag = "Execute";

    // Update is called once per frame
    void Update()
    {
        if(targetAnimator != null)
        {
            if(targetAnimator.GetCurrentAnimatorStateInfo(0).IsTag(cutsceneAnimaitonTag))
            {
                triggerCutsceneAnimaiton = false;
                targetAnimator.SetBool(cutsceneAnimaitonTag, false);
            } else if(triggerCutsceneAnimaiton)
            {
                targetAnimator.SetTrigger(cutsceneAnimaitonTag);
                triggerCutsceneAnimaiton = false;
            }
        }
    }
}
