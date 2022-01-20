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

        wrapper.Seek(0.5f);
        wrapper.Decompress();
        for (int i = 0; i < wrapper.NumTracks; i++)
        {
            Debug.LogFormat("{0} Rot{1} Pos{2} Scale{3}", wrapper.TrackNames[i], wrapper.GetTrackRotation(i).eulerAngles.ToString("F4"), wrapper.GetTrackPosition(i).ToString("F4"), wrapper.GetTrackScale(i).ToString("F4"));
            var go = GameObject.Find(wrapper.TrackNames[i]);
            if (go != null)
            {
                if (wrapper.HasTrackPosition(i))
                {
                    Debug.LogFormat("{0} {1}-{2} {3}", wrapper.TrackNames[i], go.transform.localPosition.ToString("F4"), wrapper.GetTrackPosition(i).ToString("F4"), (wrapper.GetTrackPosition(i) - go.transform.localPosition).magnitude);
                    go.transform.localPosition = wrapper.GetTrackPosition(i);
                }
                if (wrapper.HasTrackRotation(i))
                {
                    go.transform.localRotation = wrapper.GetTrackRotation(i);
                }
                if (wrapper.HasTrackScale(i))
                {
                    go.transform.localScale = wrapper.GetTrackScale(i);
                }
            }
        }

        wrapper.Dispose();
    }
}
