using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using Unity.Collections.LowLevel.Unsafe;

public unsafe struct ACLJob : IAnimationJob
{
    TransformStreamHandle mRootHandle;
    [ReadOnly]
    NativeArray<TransformStreamHandle> mHandles;
    [ReadOnly]
    NativeArray<int> mHandleIndex;

    System.IntPtr mACLContext;
    float mDuration;
    int mNumTracks;
    NativeArray<float> mResultBuffer;
    NativeArray<byte> mFlagBuffer;

    float mTime;

    public ACLJob(TransformStreamHandle rootHandle, NativeArray<TransformStreamHandle> handles, NativeArray<int> handleIndex, System.IntPtr aclContext, float duration, int numTracks, NativeArray<float> resultBuffer, NativeArray<byte> flagBuffer)
    {
        mRootHandle = rootHandle;
        mHandles = handles;
        mHandleIndex = handleIndex;

        mACLContext = aclContext;
        mDuration = duration;
        mNumTracks = numTracks;
        mResultBuffer = resultBuffer;
        mFlagBuffer = flagBuffer;

        mTime = 0;
    }

    public void SetTime(float time)
    {
        mTime = Mathf.Repeat(time, mDuration);
    }

    private void Process(int track_index, AnimationStream stream, TransformStreamHandle handle)
    {
        if (track_index < 0)
            return;
        
        if ((mFlagBuffer[track_index] & 1) > 0)
            handle.SetLocalRotation(stream, new Quaternion(mResultBuffer[12 * track_index + 0], mResultBuffer[12 * track_index + 1], mResultBuffer[12 * track_index + 2], mResultBuffer[12 * track_index + 3]));
            
        if ((mFlagBuffer[track_index] & 2) > 0)
            handle.SetLocalPosition(stream, new Vector3(mResultBuffer[12 * track_index + 4], mResultBuffer[12 * track_index + 5], mResultBuffer[12 * track_index + 6]));
            
        if ((mFlagBuffer[track_index] & 4) > 0)
            handle.SetLocalScale(stream, new Vector3(mResultBuffer[12 * track_index + 8], mResultBuffer[12 * track_index + 9], mResultBuffer[12 * track_index + 10]));
    }

    public void ProcessRootMotion(AnimationStream stream)
    {
        // Loop
        mTime = Mathf.Repeat(mTime + stream.deltaTime, mDuration);
        // Only Seek & Compress here, no need in ProcessAnimation
        ACLBinding.SeekInContext(mACLContext, mTime, ACLBinding.SampleRoundingPolicy.None);
        void* p = NativeArrayUnsafeUtility.GetUnsafePtr(mResultBuffer);
        void* f = NativeArrayUnsafeUtility.GetUnsafePtr(mFlagBuffer);
        ACLBinding.DecompressTracks(mACLContext, (System.IntPtr)p, (System.IntPtr)f);

        Process(0, stream, mRootHandle);
    }

    public void ProcessAnimation(AnimationStream stream)
    {
        var numHandles = mHandles.Length;

        for (var i = 0; i < numHandles; ++i)
        {
            Process(mHandleIndex[i], stream, mHandles[i]);
        }
    }
}
