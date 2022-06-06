using UnityEngine;
using UnityEngine.Events;
using Lean.Transition; // Make sure you add this if you want to use Lean Transition!

// We put this example code in a namespace, so it doesn't clutter the project up
namespace Lean.Transition.Examples
{
	/// <summary>This component executes the specified transitions at regular intervals.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanAnimationRepeater")]
	[AddComponentMenu(LeanTransition.ComponentMenuPrefix + "Lean Animation Repeater")]
	public class LeanAnimationRepeater : LeanAnimation
	{
		/// <summary>When this reaches 0, the transitions will begin.</summary>
		public float RemainingTime = 1.0f;

		/// <summary>When RemainingTime reaches 0, it will be reset to <b>TimeInterval</b>.</summary>
		public float TimeInterval = 3.0f;

		/// <summary>The event will execute when <b>RemainingTime</b> reaches 0.</summary>
		public UnityEvent OnAnimation;

		// Update is automatically called every game loop
		void Update()
		{
			// Decrease time
			RemainingTime -= Time.deltaTime;

			// Ready to repeat?
			if (RemainingTime <= 0.0f)
			{
				// Reset time
				RemainingTime = TimeInterval;

				// Begin transitions from LeanAnimation
				BeginTransitions();

				// Call event?
				if (OnAnimation != null)
				{
					OnAnimation.Invoke();
				}
			}
		}
	}
}