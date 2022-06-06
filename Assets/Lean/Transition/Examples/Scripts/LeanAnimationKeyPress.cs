using UnityEngine;
using UnityEngine.Events;
using Lean.Transition; // Make sure you add this if you want to use Lean Transition!

// We put this example code in a namespace, so it doesn't clutter the project up
namespace Lean.Transition.Examples
{
	/// <summary>This component executes the specified transitions when you press a key.</summary>
	[HelpURL(LeanTransition.HelpUrlPrefix + "LeanAnimationKeyPress")]
	[AddComponentMenu(LeanTransition.ComponentMenuPrefix + "Lean Animation Key Press")]
	public class LeanAnimationKeyPress : LeanAnimation
	{
		/// <summary>The animation will execute when this key is pressed.</summary>
		public KeyCode RequiredKey;

		/// <summary>The event will execute when <b>RequiredKey</b> is pressed.</summary>
		public UnityEvent OnAnimation;

		// Update is automatically called every game loop
		void Update()
		{
			// Required key was pressed down?
			if (Input.GetKeyDown(RequiredKey) == true)
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
}