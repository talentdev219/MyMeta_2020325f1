using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Lean.Common;
using Lean.Transition;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Gui
{
	/// <summary>This component allows you to drag the specified RectTransform when you drag on this UI element.</summary>
	[RequireComponent(typeof(RectTransform))]
	[HelpURL(LeanGui.HelpUrlPrefix + "LeanDrag")]
	[AddComponentMenu(LeanGui.ComponentMenuPrefix + "Drag")]
	public class LeanDrag : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		/// <summary>If you want a different RectTransform to be moved while dragging on this UI element, then specify it here. This allows you to turn the current UI element into a drag handle.</summary>
		public RectTransform Target { set { target = value; } get { return target; } } [SerializeField] private RectTransform target;

		/// <summary>Should you be able to drag horizontally?</summary>
		public bool Horizontal { set { horizontal = value; } get { return horizontal; } } [SerializeField] private bool horizontal = true;

		/// <summary>Should the horizontal position value be clamped?</summary>
		public bool HorizontalClamp { set { horizontalClamp = value; } get { return horizontalClamp; } } [SerializeField] private bool horizontalClamp;

		/// <summary>The minimum position value.</summary>
		public float HorizontalMin { set { horizontalMin = value; } get { return horizontalMin; } } [SerializeField] private float horizontalMin;

		/// <summary>The maximum position value.</summary>
		public float HorizontalMax { set { horizontalMax = value; } get { return horizontalMax; } } [SerializeField] private float horizontalMax;

		/// <summary>If you want the position to be magnetized toward the min/max value, then this allows you to set the speed.
		/// -1 = no magnet.</summary>
		public float HorizontalMagnet { set { horizontalMagnet = value; } get { return horizontalMagnet; } } [SerializeField] private float horizontalMagnet = -1.0f;

		/// <summary>Should you be able to drag vertically?</summary>
		public bool Vertical { set { vertical = value; } get { return vertical; } } [SerializeField] private bool vertical = true;

		/// <summary>Should the vertical position value be clamped?</summary>
		public bool VerticalClamp { set { verticalClamp = value; } get { return verticalClamp; } } [SerializeField] private bool verticalClamp;

		/// <summary>The minimum position value.</summary>
		public float VerticalMin { set { verticalMin = value; } get { return verticalMin; } } [SerializeField] private float verticalMin;

		/// <summary>The maximum position value.</summary>
		public float VerticalMax { set { verticalMax = value; } get { return verticalMax; } } [SerializeField] private float verticalMax;

		/// <summary>If you want the position to be magnetized toward the min/max value, then this allows you to set the speed.
		/// -1 = no magnet.</summary>
		public float VerticalMagnet { set { verticalMagnet = value; } get { return verticalMagnet; } } [SerializeField] private float verticalMagnet = -1.0f;

		/// <summary>This allows you to perform a transition when this element begins being dragged.
		/// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
		/// For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color.
		/// NOTE: Any transitions you perform here must be reverted in the <b>Normal Transitions</b> setting using a matching transition component.</summary>
		public LeanPlayer BeginTransitions { get { if (beginTransitions == null) beginTransitions = new LeanPlayer(); return beginTransitions; } } [SerializeField] private LeanPlayer beginTransitions;

		/// <summary>This allows you to perform a transition when this element ends being dragged.
		/// You can create a new transition GameObject by right clicking the transition name, and selecting <b>Create</b>.
		/// For example, the <b>LeanGraphicColor (Graphic.color Transition)</b> component can be used to change the color.</summary>
		public LeanPlayer EndTransitions { get { if (endTransitions == null) endTransitions = new LeanPlayer(); return endTransitions; } } [SerializeField] private LeanPlayer endTransitions;

		/// <summary>This allows you to perform an actions when this element begins being dragged.</summary>
		public UnityEvent OnBegin { get { if (onBegin == null) { onBegin = new UnityEvent(); } return onBegin; } } [SerializeField] private UnityEvent onBegin;

		/// <summary>This allows you to perform an actions when this element ends being dragged.</summary>
		public UnityEvent OnEnd { get { if (onEnd == null) { onEnd = new UnityEvent(); } return onEnd; } } [SerializeField] private UnityEvent onEnd;

		// Is this element currently being dragged?
		[System.NonSerialized]
		protected bool dragging;

		[System.NonSerialized]
		private Vector2 startOffset;

		[System.NonSerialized]
		private Vector2 currentPosition;

		[System.NonSerialized]
		private RectTransform cachedRectTransform;

		[System.NonSerialized]
		private bool cachedRectTransformSet;
		
		public RectTransform TargetTransform
		{
			get
			{
				if (target != null)
				{
					return target;
				}

				if (cachedRectTransformSet == false)
				{
					cachedRectTransform    = GetComponent<RectTransform>();
					cachedRectTransformSet = true;
				}

				return cachedRectTransform;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			// Only allow dragging if certain conditions are met
			if (MayDrag(eventData) == true)
			{
				var vector = default(Vector2);
				var target = TargetTransform;

				// Is this pointer inside this rect transform?
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, eventData.position, eventData.pressEventCamera, out vector) == true)
				{
					dragging        = true;
					currentPosition = target.anchoredPosition;

					if (beginTransitions != null)
					{
						beginTransitions.Begin();
					}

					if (onBegin != null)
					{
						onBegin.Invoke();
					}
				}
			}
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			// Only drag if OnBeginDrag was successful
			if (dragging == true)
			{
				// Only allow dragging if certain conditions are met
				if (MayDrag(eventData) == true)
				{
					var oldVector = default(Vector2);
					var target    = TargetTransform;

					// Get the previous pointer position relative to this rect transform
					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, eventData.position - eventData.delta, eventData.pressEventCamera, out oldVector) == true)
					{
						var newVector = default(Vector2);

						// Get the current pointer position relative to this rect transform
						if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, eventData.position, eventData.pressEventCamera, out newVector) == true)
						{
							var anchoredPosition = target.anchoredPosition;

							currentPosition += (Vector2)(target.localRotation * (newVector - oldVector));

							if (horizontal == true)
							{
								anchoredPosition.x = currentPosition.x;
							}

							if (vertical == true)
							{
								anchoredPosition.y = currentPosition.y;
							}

							ClampPosition(ref anchoredPosition);

							// Offset the anchored position by the difference
							target.anchoredPosition = anchoredPosition;
						}
					}
				}
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			dragging = false;

			if (endTransitions != null)
			{
				endTransitions.Begin();
			}

			if (onEnd != null)
			{
				onEnd.Invoke();
			}
		}

		protected override void Start()
		{
			base.Start();

			transition   = Transition.None;
			interactable = true;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			LeanGui.OnDraggingCheck += DraggingCheck;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			LeanGui.OnDraggingCheck -= DraggingCheck;
		}

		protected virtual void Update()
		{
			var target           = TargetTransform;
			var anchoredPosition = target.anchoredPosition;

			ClampPosition(ref anchoredPosition);

			MagnetPosition(target, ref anchoredPosition);

			target.anchoredPosition = anchoredPosition;
		}

		private void DraggingCheck(ref bool check)
		{
			if (dragging == true)
			{
				check = true;
			}
		}

		private bool MayDrag(PointerEventData eventData)
		{
			return IsActive() && IsInteractable();// && eventData.button == PointerEventData.InputButton.Left;
		}

		private void ClampPosition(ref Vector2 anchoredPosition)
		{
			if (horizontalClamp == true)
			{
				anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, horizontalMin, horizontalMax);
			}

			if (verticalClamp == true)
			{
				anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, verticalMin, verticalMax);
			}
		}

		private void MagnetPosition(RectTransform target, ref Vector2 anchoredPosition)
		{
#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return;
			}
#endif
			if (dragging == false)
			{
				if (horizontal == true && horizontalClamp == true && horizontalMagnet >= 0.0f)
				{
					var factor    = LeanHelper.DampenFactor(horizontalMagnet, Time.deltaTime);
					var middle    = (horizontalMin + horizontalMax) * 0.5f;
					var targetPos = horizontalMin;

					if (anchoredPosition.x > middle)
					{
						targetPos = horizontalMax;
					}

					anchoredPosition.x = Mathf.Lerp(anchoredPosition.x, targetPos, factor);

					target.anchoredPosition = anchoredPosition;
				}

				if (vertical == true && verticalClamp == true && verticalMagnet >= 0.0f)
				{
					var factor    = LeanHelper.DampenFactor(verticalMagnet, Time.deltaTime);
					var middle    = (verticalMin + verticalMax) * 0.5f;
					var targetPos = verticalMin;

					if (anchoredPosition.y > middle)
					{
						targetPos = verticalMax;
					}

					anchoredPosition.y = Mathf.Lerp(anchoredPosition.y, targetPos, factor);

					target.anchoredPosition = anchoredPosition;
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Gui
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanDrag))]
	public class LeanDrag_Editor : LeanInspector<LeanDrag>
	{
		protected override void DrawInspector()
		{
			Draw("m_Navigation");
			Draw("target", "If you want a different RectTransform to be moved while dragging on this UI element, then specify it here. This allows you to turn the current UI element into a drag handle.");

			EditorGUILayout.Separator();

			Draw("horizontal", "Should you be able to drag horizontally?");
			if (Any(t => t.Horizontal == true))
			{
				EditorGUI.indentLevel++;
					Draw("horizontalClamp", "Should the horizontal position value be clamped?", "Clamp");
					if (Any(t => t.HorizontalClamp == true))
					{
						EditorGUI.indentLevel++;
							Draw("horizontalMin", "The minimum position value.", "Min");
							Draw("horizontalMax", "The maximum position value.", "Max");
							Draw("horizontalMagnet", "If you want the position to be magnetized toward the min/max value, then this allows you to set the speed.\n\n-1 = no magnet.", "Magnet");
						EditorGUI.indentLevel--;
					}
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Separator();

			Draw("vertical", "Should you be able to drag vertically?");
			if (Any(t => t.Vertical == true))
			{
				EditorGUI.indentLevel++;
					Draw("verticalClamp", "Should the vertical position value be clamped?", "Clamp");
					if (Any(t => t.VerticalClamp == true))
					{
						EditorGUI.indentLevel++;
							Draw("verticalMin", "The minimum position value.", "Min");
							Draw("verticalMax", "The maximum position value.", "Max");
							Draw("verticalMagnet", "If you want the position to be magnetized toward the min/max value, then this allows you to set the speed.\n\n-1 = no magnet.", "Magnet");
						EditorGUI.indentLevel--;
					}
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Separator();

			Draw("beginTransitions", "This allows you to perform a transition when this element begins being dragged. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the LeanGraphicColor (Graphic.color Transition) component can be used to change the color.\n\nNOTE: Any transitions you perform here must be reverted in the Normal Transitions setting using a matching transition component.");
			Draw("endTransitions", "This allows you to perform a transition when this element ends being dragged. You can create a new transition GameObject by right clicking the transition name, and selecting Create. For example, the LeanGraphicColor (Graphic.color Transition) component can be used to change the color.");

			EditorGUILayout.Separator();

			Draw("onBegin");
			Draw("onEnd");
		}
	}
}
#endif