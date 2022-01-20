using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlManager : MonoBehaviour
{
	public GameObject target;
	List<IPlayBy> duplicateTarget;
	IPlayBy play;
    void Start()
    {
        play = target.GetComponent<IPlayBy>();
		duplicateTarget = new List<IPlayBy>();
    }

    void OnGUI ()
	{
		GUILayout.Box ("Scenes", GUILayout.Width (170), GUILayout.Height (100));
		Rect screenRect = new Rect (10, 25, 150, 100);
		GUILayout.BeginArea (screenRect);
		if (GUILayout.RepeatButton ("Normal Anim"))
		{
			SceneManager.LoadScene("AnimationPlayableTest");
		}
		if (GUILayout.RepeatButton ("ACL Anim"))
		{
			SceneManager.LoadScene("ACLPlayableTest");
		}
		if (GUILayout.RepeatButton ("Add 100 Char"))
		{
			for (int i = 0; i < 100; i++)
            {
				var go2 = Instantiate(target, new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-0.5f, 0.5f), Random.Range(-5.0f, 1.0f)), Quaternion.identity);
				duplicateTarget.Add(go2.GetComponent<IPlayBy>());
            }
		}
		GUILayout.EndArea ();

		GUILayout.Box ("Animations", GUILayout.Width (170), GUILayout.Height (25 * (play.GetAnimationCount() + 2)));
		screenRect = new Rect (10, 125, 150, 25 * (play.GetAnimationCount() + 2));
		GUILayout.BeginArea (screenRect);
		for (int i = 0; i < play.GetAnimationCount(); i++)
		{
			if (GUILayout.RepeatButton (play.GetAnimationName(i)))
			{
				play.SelectAnimation(i);
				foreach (var dplay in duplicateTarget)
                {
					dplay.SelectAnimation(i);
                }
			}
		}
		if (GUILayout.RepeatButton ("Random"))
		{
			play.SelectAnimation(Random.Range(0, play.GetAnimationCount()));
			foreach (var dplay in duplicateTarget)
            {
				dplay.SelectAnimation(Random.Range(0, play.GetAnimationCount()));
            }
		}
		GUILayout.EndArea ();
	}
}
