using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

namespace UnityChan
{
	public class ACLTest : MonoBehaviour
	{
		public TextAsset[] animations;
		PlayableGraph playableGraph;
		AnimationPlayableOutput playableOutput;
		TransformStreamHandle rootHandle;
		AnimationScriptPlayable aclPlayable;
		NativeArray<TransformStreamHandle> handles;
		string[] handleNames;

		void Start ()
		{
			Animator animator = GetComponent<Animator> ();

			playableGraph = PlayableGraph.Create("PlayAnimationSample");

			rootHandle = animator.BindStreamTransform(transform);
			var transforms = transform.GetComponentsInChildren<Transform>();
			var numTransforms = transforms.Length - 1;
			handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			handleNames = new string[numTransforms];

			for (var i = 0; i < numTransforms; ++i)
			{
				handles[i] = animator.BindStreamTransform(transforms[i + 1]);
				handleNames[i] = transforms[i + 1].name;
			}

			if (animations.Length > 0)
			{
				var aclJob = new ACLJob(rootHandle, handles, handleNames, new ACLWrapper(animations[0].bytes));
				aclPlayable = AnimationScriptPlayable.Create(playableGraph, aclJob);
				aclPlayable.SetProcessInputs(false);
			}

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
			for (int i = 0; i < animations.Length; i++)
			{
				if (GUILayout.RepeatButton (animations[i].name))
				{
					var aclJob = new ACLJob(rootHandle, handles, handleNames, new ACLWrapper(animations[i].bytes));
					aclPlayable.SetJobData(aclJob);
					aclPlayable.SetTime(0);
				}
			}
			GUILayout.EndArea ();
		}
	}
}
