using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

namespace UnityChan
{
	public class ACLTest : MonoBehaviour
	{
		public AnimationClip[] animations;
		PlayableGraph playableGraph;
		AnimationPlayableOutput playableOutput;
		TransformStreamHandle rootHandle;
		AnimationScriptPlayable aclPlayable;
		NativeArray<TransformStreamHandle> handles;

		void Start ()
		{
			Animator animator = GetComponent<Animator> ();

			playableGraph = PlayableGraph.Create("PlayAnimationSample");

			rootHandle = animator.BindStreamTransform(transform);
			var transforms = transform.GetComponentsInChildren<Transform>();
			var numTransforms = transforms.Length - 1;
			handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (var i = 0; i < numTransforms; ++i)
			{
				Debug.Log(transforms[i + 1].name);
				handles[i] = animator.BindStreamTransform(transforms[i + 1]);
			}

			var aclJob = new ACLJob(rootHandle, handles, animations[0]);
			aclPlayable = AnimationScriptPlayable.Create(playableGraph, aclJob);
			aclPlayable.SetProcessInputs(false);
			aclPlayable.AddInput(AnimationClipPlayable.Create(playableGraph, animations[0]), 0, 1.0f);

			playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
			playableOutput.SetSourcePlayable(aclPlayable);

			playableGraph.Play();
		}

        void OnDestroy()
        {
			playableGraph.Destroy();
			handles.Dispose();
		}

        void OnGUI ()
		{
			GUILayout.Box ("Animations", GUILayout.Width (170), GUILayout.Height (25 * (animations.Length + 2)));
			Rect screenRect = new Rect (10, 25, 150, 25 * (animations.Length + 1));
			GUILayout.BeginArea (screenRect);
			foreach (var animation in animations) 
			{
				if (GUILayout.RepeatButton (animation.name))
				{
					//aclPlayable.AddInput(AnimationClipPlayable.Create(playableGraph, animation), 0, 1.0f);
					//var aclJob = aclPlayable.GetJobData<ACLJob>();
				}
			}
			GUILayout.EndArea ();
		}
	}
}
