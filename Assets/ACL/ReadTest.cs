using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

public class ReadTest
{
    [UnityEditor.MenuItem("ACL/ReadTest")]
    static unsafe void Test()
    {
        var bytes = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/UnityChan/Animations/unitychan_JUMP00@JUMP00.bytes");

        ACLWrapper wrapper = new ACLWrapper(bytes.bytes);
        Debug.Log(wrapper.NumTracks);
        Debug.Log(wrapper.Duration);

        wrapper.Seek(0.0f);
        wrapper.Decompress();
        for (int i = 0; i < wrapper.NumTracks; i++)
        {
            Debug.LogFormat("{0} {1} {2} {3}", wrapper.TrackNames[i], wrapper.GetTrackRotation(i).ToString("F4"), wrapper.GetTrackPosition(i).ToString("F4"), wrapper.GetTrackScale(i).ToString("F4"));
        }

        wrapper.Dispose();
    }
}
