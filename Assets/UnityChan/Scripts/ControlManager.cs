using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
	public GameObject target;
	IPlayBy play;
    void Start()
    {
        play = target.GetComponent<IPlayBy>();
    }

    void OnGUI ()
	{
		GUILayout.Box ("Animations", GUILayout.Width (170), GUILayout.Height (25 * (play.GetAnimationCount() + 2)));
		Rect screenRect = new Rect (10, 25, 150, 25 * (play.GetAnimationCount() + 1));
		GUILayout.BeginArea (screenRect);
		for (int i = 0; i < play.GetAnimationCount(); i++)
		{
			if (GUILayout.RepeatButton (play.GetAnimationName(i)))
			{
				play.SelectAnimation(i);
			}
		}
		GUILayout.EndArea ();
	}
}
