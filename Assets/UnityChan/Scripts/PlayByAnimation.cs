using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace UnityChan
{
	public class PlayByAnimation : MonoBehaviour, IPlayBy
	{
		public AnimationClip[] animations;
		PlayableGraph playableGraph;
		AnimationPlayableOutput playableOutput;

		void Start ()
		{
			Animator animator = GetComponent<Animator> ();

			playableGraph = PlayableGraph.Create("PlayAnimationSample");

			playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
			playableOutput.SetSourcePlayable(AnimationClipPlayable.Create(playableGraph, animations[0]));

			playableGraph.Play();
		}

        void OnDestroy()
        {
			playableGraph.Destroy();
		}
		
		public int GetAnimationCount()
        {
			return animations.Length;
        }
		public string GetAnimationName(int index)
        {
			return animations[index].name;
        }
		public void SelectAnimation(int index)
        {
			playableOutput.SetSourcePlayable(AnimationClipPlayable.Create(playableGraph, animations[index]));
        }
	}
}
