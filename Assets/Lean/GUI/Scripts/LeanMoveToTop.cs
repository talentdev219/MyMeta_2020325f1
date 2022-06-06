using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component will automatically move the specified transform to be the first sibling when you press down on this UI element.</summary>
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanMoveToTop")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Move To Top")]
	public class LeanMoveToTop : MonoBehaviour, IPointerDownHandler
	{
		/// <summary>If you want a different transform to be moved when pressing down on this UI element, then specify it here.
		/// None = The current GameObject's transform.</summary>
		public Transform Target { set { target = value; } get { return target; } } [SerializeField] private Transform target;

		public void OnPointerDown(PointerEventData eventData)
		{
			var finalTransform = target;

			if (finalTransform == null)
			{
				finalTransform = transform;
			}

			finalTransform.SetAsLastSibling();
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanMoveToTop))]
	public class LeanMoveToTop_Editor : LeanInspector<LeanMoveToTop>
	{
		protected override void DrawInspector()
		{
			Draw("target", "If you want a different transform to be moved when pressing down on this UI element, then specify it here.\n\nNone = The current GameObject's transform.");
		}
	}
}
#endif