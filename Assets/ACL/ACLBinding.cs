using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class ACLBinding
{
    [DllImport ("acl_unity")]
    public static extern bool IsCompressedTracksValid (System.IntPtr buffer);
    
    [DllImport ("acl_unity")]
    public static extern int GetNumTracks (System.IntPtr buffer);
    
    [DllImport ("acl_unity")]
    public static extern float GetDuration (System.IntPtr buffer);
    
    [DllImport ("acl_unity")]
    public static extern System.IntPtr GetTrackName (System.IntPtr buffer, int track_index);
    
    [DllImport ("acl_unity")]
    public static extern int GetParentTrackIndex (System.IntPtr buffer, int track_index);
    
    [DllImport ("acl_unity")]
    public static extern System.IntPtr PrepareDecompressContext (System.IntPtr buffer);

    [DllImport ("acl_unity")]
    public static extern void ReleaseDecompressContext (System.IntPtr context);
    
	public enum SampleRoundingPolicy : int
	{
		None,
		Floor,
		Ceil,
		Nearest,
	};

    [DllImport ("acl_unity")]
    public static extern void SeekInContext (System.IntPtr context, float sample_time, SampleRoundingPolicy rounding_policy);
    
    [DllImport ("acl_unity")]
    public static extern void DecompressTracks (System.IntPtr context, System.IntPtr result, System.IntPtr flag);
    
    [DllImport ("acl_unity")]
    public static extern void DecompressTracksEx (System.IntPtr context, System.IntPtr result, System.IntPtr flag, float pos_threshold, float rot_threshold, float scale_threshold);
}
