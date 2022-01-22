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
        int pos_count = 0, rot_count = 0, scale_count = 0;
        for (int p = 0; p < 24; p++)
        {
            wrapper.Seek(p * 0.01f);
            wrapper.DecompressEx(0, 0.0f, 0);
            for (int i = 0; i < wrapper.NumTracks; i++)
            {
                if (wrapper.HasTrackPosition(i))
                {
                    pos_count++;
                }
                if (wrapper.HasTrackRotation(i))
                {
                    rot_count++;
                }
                if (wrapper.HasTrackScale(i))
                {
                    scale_count++;
                }
            }
        }
        Debug.LogFormat("pos_count {0}, rot_count {1}, scale_count {2}", pos_count, rot_count, scale_count);
#if false
        for (int i = 0; i < wrapper.NumTracks; i++)
        {
            Debug.LogFormat("{0} Rot{1} Pos{2} Scale{3}", wrapper.TrackNames[i], wrapper.GetTrackRotation(i).eulerAngles.ToString("F4"), wrapper.GetTrackPosition(i).ToString("F4"), wrapper.GetTrackScale(i).ToString("F4"));
            var go = GameObject.Find(wrapper.TrackNames[i]);
            if (go != null)
            {
                if (wrapper.HasTrackPosition(i))
                {
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
#endif
        wrapper.Dispose();
    }
}
