using MEC;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EnhancedAnimationSystem
{
    [System.Serializable]
    public class AnimationClipState
    {
        public AnimationClip Clip;
        public float Speed = 1f;
        public float NormalizedTime = 0f;
        [Range(0f, 1f)] public float BlendDuration = 0.075f;
        public bool Interrupt = false;

        public AnimationClipState(AnimationClip clip)
        {
            Clip = clip;
        }
    }

    public class QueuePlayable : PlayableBehaviour
    {
        List<AnimationClipState> clipStates;
        List<AnimationClipPlayable> playables;
        int currentClipIndex = -1;
        float timeToNextClip;
        Playable queueMixer;
        Playable ownerMixer;
        PlayableGraph graph;


        public void Initialize(AnimationClipState clipState, Playable owner, PlayableGraph graph)
        {
            ownerMixer = owner;
            this.graph = graph;

            clipStates = new List<AnimationClipState>();
            playables = new List<AnimationClipPlayable>();
            Push(clipState);
        }

        public void Push(AnimationClipState clipState)
        {
            AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clipState.Clip);
            playable.SetSpeed(clipState.Speed);
            playables.Add(playable);
            clipStates.Add(clipState);

            if(queueMixer.IsValid())
            {
                queueMixer.AddInput(playable, 0);
            }
            else
            {
                ownerMixer.SetInputCount(1);
                queueMixer = AnimationMixerPlayable.Create(graph, 1);
                graph.Connect(queueMixer, 0, queueMixer, 0);
                ownerMixer.SetInputWeight(0, 1f);
            }

            graph.Connect(playable, 0, ownerMixer, playables.Count - 1);
            ownerMixer.SetInputWeight(playables.Count - 1, 1.0f);
        }

        public override void PrepareFrame(Playable owner, FrameData info)
        {
            if(queueMixer.GetInputCount() == 0) return;

            // Advance to next clip if necessary.
            timeToNextClip -= (float)info.deltaTime;

            if(timeToNextClip <= 0.0f)
            {
                currentClipIndex++;
                if(currentClipIndex >= queueMixer.GetInputCount())
                {
                    return;
                    //currentClipIndex = 0;
                }
                var currentClip = (AnimationClipPlayable)queueMixer.GetInput(currentClipIndex);

                // Reset the time so that the next clip starts at the correct position.
                currentClip.SetTime(0);

                timeToNextClip = currentClip.GetAnimationClip().length;
            }

            // Adjust the weight of the inputs.
            for(int clipIndex = 0; clipIndex < queueMixer.GetInputCount(); ++clipIndex)
            {
                if(clipIndex == currentClipIndex)
                    queueMixer.SetInputWeight(clipIndex, 1.0f);
                else
                    queueMixer.SetInputWeight(clipIndex, 0.0f);
            }
        }

        public AnimationClipState GetCurrentAnimaitonClipState() => clipStates[currentClipIndex];
    }

    public class EnhancedAnimationController
    {
        PlayableGraph playableGraph;
        readonly AnimationMixerPlayable masterMixer;
        readonly AnimatorControllerPlayable locomotionMixer;
        AnimationClipPlayable oneShotPlayable;

        CoroutineHandle blendInHandle;
        CoroutineHandle blendOutHandle;


        public EnhancedAnimationController(Animator animator, int masterInputCount=2)
        {
            playableGraph = PlayableGraph.Create("Enhanced Animation Controller");
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(playableGraph, "Animation Controller Output", animator);

            masterMixer = AnimationMixerPlayable.Create(playableGraph, masterInputCount);
            output.SetSourcePlayable(masterMixer);

            locomotionMixer = AnimatorControllerPlayable.Create(playableGraph, animator.runtimeAnimatorController);
            masterMixer.ConnectInput(0, locomotionMixer, 0);
            playableGraph.GetRootPlayable(0).SetInputWeight(0, 1);

            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            playableGraph.Play();
        }

        public void Play(AnimationClipState clipState)
        {
            if(oneShotPlayable.IsValid() && oneShotPlayable.GetAnimationClip() == clipState.Clip) return;
            InterruptOneShot();

            oneShotPlayable = AnimationClipPlayable.Create(playableGraph, clipState.Clip);
            oneShotPlayable.SetSpeed(clipState.Speed);

            masterMixer.ConnectInput(1, oneShotPlayable, 0);
            masterMixer.SetInputWeight(1, 1f);

            // Custom blend time
            float blendDuration = Mathf.Max(clipState.BlendDuration, Mathf.Min(clipState.Clip.length * clipState.BlendDuration, clipState.Clip.length / 2));
            BlendIn(blendDuration);
            BlendOut(blendDuration, clipState.Clip.length - blendDuration);
        }

        void InterruptOneShot()
        {
            Timing.KillCoroutines(blendInHandle);
            Timing.KillCoroutines(blendOutHandle);

            masterMixer.SetInputWeight(0, 1f);
            masterMixer.SetInputWeight(1, 0f);

            if(oneShotPlayable.IsValid())
            {
                DisconnectOneShot();
            }
        }

        void DisconnectOneShot()
        {
            masterMixer.DisconnectInput(1);
            playableGraph.DestroyPlayable(oneShotPlayable);
        }

        IEnumerator<float> Blend(float duration, Action<float> blendCallback, float delay = 0f, Action finishCallback = null)
        {
            if(delay > 0f)
            {
                yield return Timing.WaitForSeconds(delay);
            }

            float blendTime = 0f;
            while(blendTime < 1f)
            {
                blendTime += Time.deltaTime / duration;
                blendCallback(blendTime);
                yield return blendTime;
            }

            blendCallback(1f);
            finishCallback?.Invoke();
        }

        void BlendIn(float duration, int blendFromIndex=0, int blendToIndex=1)
        {
            blendInHandle = Timing.RunCoroutine(Blend(duration, blendTime => {
                float weight = Mathf.Lerp(1f, 0f, blendTime);
                masterMixer.SetInputWeight(blendFromIndex, weight);
                masterMixer.SetInputWeight(blendToIndex, 1f - weight);
            }));
        }

        void BlendOut(float duration, float delay, int blendFromIndex=1, int blendToIndex=0)
        {
            blendOutHandle = Timing.RunCoroutine(Blend(duration, blendTime =>
            {
                float weight = Mathf.Lerp(0f, 1f, blendTime);
                masterMixer.SetInputWeight(blendToIndex, weight);
                masterMixer.SetInputWeight(blendFromIndex, 1f - weight);
            }, delay, DisconnectOneShot));
        }

        public void Destroy()
        {
            if(playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
        }

        public AnimationClip GetPlayingClip()
        {
            if(oneShotPlayable.IsValid())
            {
                return oneShotPlayable.GetAnimationClip();
            }

            return locomotionMixer.GetCurrentAnimatorClipInfo(0)[0].clip;
        }
    }
}