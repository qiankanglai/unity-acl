using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace UnityChan
{
	public unsafe class PlayByACL : MonoBehaviour, IPlayBy
	{
		public TextAsset[] animations;
		PlayableGraph playableGraph;
		AnimationPlayableOutput playableOutput;
		AnimationScriptPlayable aclPlayable;

		TransformStreamHandle rootHandle;
		NativeArray<TransformStreamHandle> handles;
		NativeArray<int> handleIndex;
		
		ulong dataHandler = 0;
		System.IntPtr dataPtr = System.IntPtr.Zero;
		System.IntPtr context = System.IntPtr.Zero;

		int numTracks;
		float duration;
		NativeArray<byte> flagBuffer;
		NativeArray<float> resultBuffer;

		string[] handleNames;

		void ReleaseACLContext()
        {
			if (context != System.IntPtr.Zero)
			{
				ACLBinding.PrepareDecompressContext(context);
				context = System.IntPtr.Zero;
			}
			if (dataHandler != 0)
			{
				UnsafeUtility.ReleaseGCObject(dataHandler);
				dataHandler = 0;
			}
			dataPtr = System.IntPtr.Zero;
        }
		
		void AllocACLContext(byte[] data)
        {
			Debug.Assert(dataHandler == 0);
			dataPtr = (System.IntPtr)UnsafeUtility.PinGCArrayAndGetDataAddress(data, out dataHandler);
			Debug.Assert(context == System.IntPtr.Zero);
			context = ACLBinding.PrepareDecompressContext(dataPtr);
        }

		void BuildACLContext(byte[] data)
        {
			ReleaseACLContext();
			AllocACLContext(data);
			
			numTracks = ACLBinding.GetNumTracks(dataPtr);
			duration = ACLBinding.GetDuration(dataPtr);
			string[] TrackNames = new string[numTracks];
			for (int i = 0; i < numTracks; i++)
			{
				TrackNames[i] = Marshal.PtrToStringAnsi(ACLBinding.GetTrackName(dataPtr, i));
			}

			if (resultBuffer.IsCreated)
				resultBuffer.Dispose();
			if (flagBuffer.IsCreated)
				flagBuffer.Dispose();

			resultBuffer = new NativeArray<float>(12 * numTracks, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			flagBuffer = new NativeArray<byte>(numTracks, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

			Debug.Assert(handles.Length == handleNames.Length);
			for (int i = 0; i < handleNames.Length; i++)
			{
				handleIndex[i] = -1;
				for (int j = 0; j < TrackNames.Length; j++)
				{
					if (handleNames[i] == TrackNames[j])
					{
						handleIndex[i] = j;
					}
				}
				/*
				if (handleIndex[i] < 0)
				{
					Debug.LogWarningFormat("Missing {0} in acl track", handleNames[i]);
				}
				*/
			}
        }

		void Start ()
		{
			Animator animator = GetComponent<Animator> ();

			playableGraph = PlayableGraph.Create("PlayAnimationSample");

			rootHandle = animator.BindStreamTransform(transform);
			var transforms = transform.GetComponentsInChildren<Transform>();
			var numTransforms = transforms.Length - 1;
			// prepare context
			handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			handleIndex = new NativeArray<int>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			handleNames = new string[numTransforms];
			// prepare handles
			for (var i = 0; i < numTransforms; ++i)
			{
				handles[i] = animator.BindStreamTransform(transforms[i + 1]);
				handleIndex[i] = -1;
				handleNames[i] = transforms[i + 1].name;
			}

			if (animations.Length > 0)
			{
				BuildACLContext(animations[0].bytes);
				var aclJob = new ACLJob(rootHandle, handles, handleIndex, context, duration, numTracks, resultBuffer, flagBuffer);
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
			handleIndex.Dispose();
			handles.Dispose();
			
			if (resultBuffer.IsCreated)
				resultBuffer.Dispose();
			if (flagBuffer.IsCreated)
				flagBuffer.Dispose();

			ReleaseACLContext();
		}

		public int GetAnimationCount()
        {
			return animations.Length;
        }
		public string GetAnimationName(int index)
        {
			string name = animations[index].name;
			int t = name.IndexOf("@");
			if (t >= 0)
            {
				name = name.Substring(t+1);
            }
			return name;
        }
		public void SelectAnimation(int index)
        {
			BuildACLContext(animations[index].bytes);
			var aclJob = new ACLJob(rootHandle, handles, handleIndex, context, duration, numTracks, resultBuffer, flagBuffer);
			aclPlayable.SetJobData(aclJob);
        }
	}
}
