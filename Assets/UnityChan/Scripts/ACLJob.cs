using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

public struct ACLJob : IAnimationJob
{
    TransformStreamHandle mRootHandle;
    NativeArray<TransformStreamHandle> mHandles;
    int mClipIndex;
    public ACLJob(TransformStreamHandle rootHandle, NativeArray<TransformStreamHandle> handles)
    {
        mRootHandle = rootHandle;
        mHandles = handles;
        mClipIndex = 0;
    }
    public void SetClipIndex(int index)
    {
        mClipIndex = index;
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
        var streamA = stream.GetInputStream(mClipIndex);

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
