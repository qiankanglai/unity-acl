using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

public struct ACLJob : IAnimationJob
{
    TransformStreamHandle mRootHandle;
    NativeArray<TransformStreamHandle> mHandles;
    AnimationClip mClip;
    public ACLJob(TransformStreamHandle rootHandle, NativeArray<TransformStreamHandle> handles, AnimationClip clip)
    {
        mRootHandle = rootHandle;
        mHandles = handles;
        mClip = clip;
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
        var numHandles = mHandles.Length;
        var streamA = stream.GetInputStream(0);
        for (var i = 0; i < numHandles; ++i)
        {
            var handle = mHandles[i];

            var pos = handle.GetLocalPosition(streamA);
            handle.SetLocalPosition(stream, pos);

            var rot = handle.GetLocalRotation(streamA);
            handle.SetLocalRotation(stream, rot);
        }
    }
}
