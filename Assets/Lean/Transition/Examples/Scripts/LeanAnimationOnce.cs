using UnityEngine;
using UnityEngine.Events;
using Lean.Transition; // Make sure you add this if you want to use Lean Transition!

// We put this example code in a namespace, so it doesn't clutter the project up
namespace Lean.Transition.Examples
{
	/// <summary>This component executes the specified transitions at regular intervals.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanAnimationOnce")]
	[AddComponentMenu(LeanTransition.ComponentMenuPrefix + "Lean Animation Once")]
	public class LeanAnimationOnce : LeanAnimation
	{
		/// <summary>The event will execute when <b>RemainingTime</b> reaches 0.</summary>
		public UnityEvent OnAnimation;

		// Start is automatically called once before Update
		void Start()
		{
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