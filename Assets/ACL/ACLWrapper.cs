using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

public unsafe class ACLWrapper : IDisposable
{
    ulong dataHandler;
    IntPtr dataPtr;
    IntPtr context = IntPtr.Zero;
    float[] resultBuffer;
    byte[] flagBuffer;
    
    public float Duration
    {
        get; private set;
    }
    public int NumTracks
    {
        get; private set;
    }
    public string[] TrackNames
    {
        get; private set;
    }
    public int[] ParentTrackIndices
    {
        get; private set;
    }

    public ACLWrapper(byte[] data)
    {
        dataPtr = (IntPtr)UnsafeUtility.PinGCArrayAndGetDataAddress(data, out dataHandler);
        context = ACLBinding.PrepareDecompressContext(dataPtr);

        NumTracks = ACLBinding.GetNumTracks(dataPtr);
        Duration = ACLBinding.GetDuration(dataPtr);
        TrackNames = new string[NumTracks];
        ParentTrackIndices = new int[NumTracks];
        for (int i = 0; i < NumTracks; i++)
        {
            TrackNames[i] = Marshal.PtrToStringAnsi(ACLBinding.GetTrackName(dataPtr, i));
            ParentTrackIndices[i] = ACLBinding.GetParentTrackIndex(dataPtr, i);
        }

        resultBuffer = new float[12 * NumTracks];
        flagBuffer = new byte[NumTracks];
    }

    ~ACLWrapper()
    {
        Dispose();
    }

    public void Seek(float time, ACLBinding.SampleRoundingPolicy policy = ACLBinding.SampleRoundingPolicy.None)
    {
        Debug.Assert(context != IntPtr.Zero);
        ACLBinding.SeekInContext(context, time, policy);
    }
    
    public void Decompress()
    {
        Debug.Assert(context != IntPtr.Zero);
        fixed (float* p = resultBuffer)
        {
            fixed (byte* f = flagBuffer)
            {
                ACLBinding.DecompressTracks(context, (IntPtr)p, (IntPtr)f);
            }
        }
    }
    
    public bool HasTrackRotation(int track_index)
    {
        return (flagBuffer[track_index] & 1) > 0;
    }
    
    public bool HasTrackPosition(int track_index)
    {
        return (flagBuffer[track_index] & 2) > 0;
    }

    public bool HasTrackScale(int track_index)
    {
        return (flagBuffer[track_index] & 4) > 0;
    }


    public Quaternion GetTrackRotation(int track_index)
    {
        return new Quaternion(resultBuffer[12 * track_index + 0], resultBuffer[12 * track_index + 1], resultBuffer[12 * track_index + 2], resultBuffer[12 * track_index + 3]);
    }
    
    public Vector3 GetTrackPosition(int track_index)
    {
        return new Vector3(resultBuffer[12 * track_index + 4], resultBuffer[12 * track_index + 5], resultBuffer[12 * track_index + 6]);
    }

    public Vector3 GetTrackScale(int track_index)
    {
        return new Vector3(resultBuffer[12 * track_index + 8], resultBuffer[12 * track_index + 9], resultBuffer[12 * track_index + 10]);
    }

    public void Dispose()
    {
        ACLBinding.PrepareDecompressContext(context);
        context = IntPtr.Zero;
        UnsafeUtility.ReleaseGCObject(dataHandler);
        dataHandler = 0;
    }
}
