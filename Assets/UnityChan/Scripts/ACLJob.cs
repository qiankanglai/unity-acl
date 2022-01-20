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

    // no check and faster!
    TransformStreamHandle* mHandlesPtr;
    int* mHandleIndexPtr;
    float* mResultPtr;
    byte* mFlagPtr;

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
        
        mHandlesPtr = (TransformStreamHandle*)NativeArrayUnsafeUtility.GetUnsafePtr(mHandles);
        mHandleIndexPtr = (int*)NativeArrayUnsafeUtility.GetUnsafePtr(mHandleIndex);
        mResultPtr = (float*)NativeArrayUnsafeUtility.GetUnsafePtr(mResultBuffer);
        mFlagPtr = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr(mFlagBuffer);

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
        
        if ((mFlagPtr[track_index] & 1) > 0)
            handle.SetLocalRotation(stream, new Quaternion(mResultPtr[12 * track_index + 0], mResultPtr[12 * track_index + 1], mResultPtr[12 * track_index + 2], mResultPtr[12 * track_index + 3]));
            
        if ((mFlagPtr[track_index] & 2) > 0)
            handle.SetLocalPosition(stream, new Vector3(mResultPtr[12 * track_index + 4], mResultPtr[12 * track_index + 5], mResultPtr[12 * track_index + 6]));
            
        if ((mFlagPtr[track_index] & 4) > 0)
            handle.SetLocalScale(stream, new Vector3(mResultPtr[12 * track_index + 8], mResultPtr[12 * track_index + 9], mResultPtr[12 * track_index + 10]));
    }

    public void ProcessRootMotion(AnimationStream stream)
    {
        // Loop
        mTime = Mathf.Repeat(mTime + stream.deltaTime, mDuration);
        // Only Seek & Compress here, no need in ProcessAnimation
        ACLBinding.SeekInContext(mACLContext, mTime, ACLBinding.SampleRoundingPolicy.None);
        ACLBinding.DecompressTracks(mACLContext, (System.IntPtr)mResultPtr, (System.IntPtr)mFlagPtr);

        Process(0, stream, mRootHandle);
    }

    public void ProcessAnimation(AnimationStream stream)
    {
        var numHandles = mHandles.Length;

        for (var i = 0; i < numHandles; ++i)
        {
            Process(mHandleIndexPtr[i], stream, mHandlesPtr[i]);
        }
    }
}
