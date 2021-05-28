using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace UnityChan
{
	public class ACLTest : MonoBehaviour
	{
		public AnimationClip[] animations;
		Animator anim;
		PlayableGraph playableGraph;
		AnimationPlayableOutput playableOutput;

		void Start ()
		{
			anim = GetComponent<Animator> ();

			playableGraph = PlayableGraph.Create("PlayAnimationSample");
			playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", anim);

			var clipPlayable = AnimationClipPlayable.Create(playableGraph, animations[0]);
			playableOutput.SetSourcePlayable(clipPlayable);
			playableGraph.Play();
		}

		void OnGUI ()
		{
			GUILayout.Box ("Face Update", GUILayout.Width (170), GUILayout.Height (25 * (animations.Length + 2)));
			Rect screenRect = new Rect (10, 25, 150, 25 * (animations.Length + 1));
			GUILayout.BeginArea (screenRect);
			foreach (var animation in animations) {
				if (GUILayout.RepeatButton (animation.name)) {
					var clipPlayable = AnimationClipPlayable.Create(playableGraph, animation);
					playableOutput.SetSourcePlayable(clipPlayable);
					playableGraph.Play();
				}
			}
			GUILayout.EndArea ();
		}
	}
}
