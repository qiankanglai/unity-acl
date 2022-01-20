using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

public struct ACLJob : IAnimationJob
{
    TransformStreamHandle mRootHandle;
    NativeArray<TransformStreamHandle> mHandles;
    ACLWrapper mACLAnimation;
    int[] mHandleIndex;
    float time;

    public ACLJob(TransformStreamHandle rootHandle, NativeArray<TransformStreamHandle> handles, string[] handleNames, ACLWrapper wrapper)
    {
        mRootHandle = rootHandle;
        mHandles = handles;
        mACLAnimation = wrapper;
        time = 0;

        Debug.Assert(handles.Length == handleNames.Length);
        mHandleIndex = new int[handleNames.Length];
        for (int i = 0; i < handleNames.Length; i++)
        {
            mHandleIndex[i] = -1;
            for (int j = 0; j < mACLAnimation.TrackNames.Length; j++)
            {
                if (handleNames[i] == mACLAnimation.TrackNames[j])
                {
                    mHandleIndex[i] = j;
                }
            }
            if (mHandleIndex[i] < 0)
            {
                Debug.LogWarningFormat("Missing {0} in acl track", handleNames[i]);
            }
        }
        mACLAnimation.Decompress();
        first = true;   // test
    }
    public void ProcessRootMotion(AnimationStream stream)
    {
        // Get root position and rotation.
        var rootPosition = mRootHandle.GetPosition(stream);
        var rootRotation = mRootHandle.GetRotation(stream);

        // The root always follow the given position and rotation.
        mRootHandle.SetPosition(stream, rootPosition);
        mRootHandle.SetRotation(stream, rootRotation);
    }
    public void ProcessAnimation(AnimationStream stream)
    {
        time += stream.deltaTime;
        mACLAnimation.Seek(time);
        //Debug.Log(time);
        mACLAnimation.Decompress();	// TODO: crash?
        //Debug.Log(mACLAnimation.GetTrackRotation(1).eulerAngles.ToString("F4"));

        var numHandles = mHandles.Length;

        for (var i = 0; i < numHandles; ++i)
        {
            if (mHandleIndex[i] < 0)
                continue;
            
            var handle = mHandles[i];

            if (mACLAnimation.HasTrackPosition(i))
                handle.SetLocalPosition(stream, mACLAnimation.GetTrackPosition(mHandleIndex[i]));
            
            if (mACLAnimation.HasTrackRotation(i))
                handle.SetLocalRotation(stream, mACLAnimation.GetTrackRotation(mHandleIndex[i]));
            
            if (mACLAnimation.HasTrackScale(i))
                handle.SetLocalScale(stream, mACLAnimation.GetTrackScale(mHandleIndex[i]));
        }
    }
}
