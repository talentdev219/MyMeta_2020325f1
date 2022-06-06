using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Transition
{
	/// <summary>This class allows you to reference Transforms whose GameObjects contain transition components.
	/// If these transition components define TargetAlias names, then this class will also manage them in the inspector.</summary>
	[System.Serializable]
	public class LeanPlayer
	{
		[SerializeField]
		private float speed;

		[SerializeField]
		private List<Transform> roots;

		[SerializeField]
		private List<string> aliases;

		[SerializeField]
		private List<Object> targets;

		[System.NonSerialized]
		private List<System.Type> types;

		public float Speed
		{
			set
			{
				speed = value;
			}

			get
			{
				return speed;
			}
		}

		public List<System.Type> Types
		{
			get
			{
				return types;
			}
		}

		private static Dictionary<string, Object> aliasTargetPairs = new Dictionary<string, Object>();

		/// <summary>These transforms store the transition components. This can be in the scene, or in a prefab.
		/// NOTE: If it's in a prefab then you will likely need to use the Add method to associate objects with an alias so they can be replaced at runtime. This is because prefabs can't reference scene objects normally.</summary>
		public List<Transform> Roots
		{
			get
			{
				if (roots == null)
				{
					roots = new List<Transform>();
				}

				return roots;
			}
		}

		/// <summary>This method allows you to register the target object with the specified alias.</summary>
		public void Add(string alias, Object target)
		{
			if (aliases == null) aliases = new List<string>();
			if (targets == null) targets = new List<Object>();

			if (aliases.Count != targets.Count)
			{
				Rebuild();
			}

			for (var i = aliases.Count - 1; i >= 0; i--)
			{
				if (aliases[i] == alias)
				{
					targets[i] = target;

					return;
				}
			}

			aliases.Add(alias);
			targets.Add(target);
		}

		/// <summary>This method will begin this transition.</summary>
		public void Begin()
		{
			LeanTransition.CurrentAliases.Clear();

			if (aliases != null && targets != null && aliases.Count == targets.Count)
			{
				for (var i = aliases.Count - 1; i >= 0; i--)
				{
					LeanTransition.CurrentAliases.Add(aliases[i], targets[i]);
				}
			}

			LeanTransition.BeginTransitions(roots, speed);
		}

		/// <summary>This method allows you to rebuild the alias and target associations.</summary>
		public void Rebuild()
		{
			if (aliases == null) aliases = new List<string>();
			if (targets == null) targets = new List<Object>();
			if (types   == null) types   = new List<System.Type>();

			var count = System.Math.Min(aliases.Count, targets.Count);

			aliasTargetPairs.Clear();

			for (var i = 0; i < count; i++)
			{
				aliasTargetPairs.Add(aliases[i], targets[i]);
			}

			aliases.Clear();
			targets.Clear();
			types.Clear();

			var aliasTypePairs = LeanTransition.FindAllAliasTypePairs(roots);

			foreach (var aliasTypePair in aliasTypePairs)
			{
				var target = default(Object);

				aliases.Add(aliasTypePair.Key);
				types.Add(aliasTypePair.Value);

				if (aliasTargetPairs.TryGetValue(aliasTypePair.Key, out target) == true)
				{
					targets.Add(target);
				}
				else
				{
					targets.Add(default(Object));
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Transition
{
	[CustomPropertyDrawer(typeof(LeanPlayer))]
	public class LeanPlayerDrawer : PropertyDrawer
	{
		private static Color color;
		private static float height;
		private static float heightStep;

		private void Validate(SerializedProperty property)
		{
			property.serializedObject.ApplyModifiedProperties();

			var sRoots = property.FindPropertyRelative("roots");

			if (sRoots.arraySize == 0)
			{
				sRoots.arraySize = 1; sRoots.serializedObject.ApplyModifiedProperties();
			}

			foreach (var targetObject in property.serializedObject.targetObjects)
			{
				var transitions = LeanHelper.GetObjectFromSerializedProperty<LeanPlayer>(targetObject, property);

				transitions.Rebuild();
			}

			property.serializedObject.Update();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Validate(property);

			property.serializedObject.Update();

			var sSpeed   = property.FindPropertyRelative("speed");
			var sRoots   = property.FindPropertyRelative("roots");
			var sAliases = property.FindPropertyRelative("aliases");
			var sTargets = property.FindPropertyRelative("targets");
			var count    = System.Math.Min(sAliases.arraySize, sTargets.arraySize);

			height     = base.GetPropertyHeight(property, label);
			heightStep = height + 2.0f;

			return height + (sSpeed.floatValue > 0.0f ? heightStep : 0.0f) + (sRoots.arraySize - 1) * heightStep + count * heightStep;
		}

		private void DrawRoot(GUIContent label, Rect position, SerializedObject sObject, SerializedProperty sSpeed, SerializedProperty sRoots, SerializedProperty sRoot, int index)
		{
			var e     = Event.current;
			var rectL = position; rectL.width = EditorGUIUtility.labelWidth - 16.0f;
			var rectR = position; rectR.xMin += EditorGUIUtility.labelWidth;

			if (e.isMouse == true && e.button == 1 && rectL.Contains(e.mousePosition) == true)
			{
				var menu          = new GenericMenu();
				var methodPrefabs = AssetDatabase.FindAssets("t:GameObject").
					Select((guid) => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid))).
					Where((prefab) => prefab.GetComponent<LeanMethod>() != null);
				var targetComponent = sObject.targetObject as Component;

				if (targetComponent != null)
				{
					if (sRoot.objectReferenceValue == null)
					{
						var title = label.text;

						menu.AddItem(new GUIContent("Create"), false, () =>
							{
								var root = new GameObject("[" + title + "]").transform;

								root.SetParent(targetComponent.transform, false);

								sRoots.GetArrayElementAtIndex(index).objectReferenceValue = root;
								sObject.ApplyModifiedProperties();

								Selection.activeTransform = root;
							});
					}
					else
					{
						menu.AddItem(new GUIContent("Add"), false, () =>
						{
							sRoots.InsertArrayElementAtIndex(index + 1);
							sRoots.GetArrayElementAtIndex(index + 1).objectReferenceValue = null;
							sObject.ApplyModifiedProperties();
						});
					}
				}

				if (sSpeed.floatValue <= 0.0f)
				{
					menu.AddItem(new GUIContent("Speed"), false, () =>
						{
							sSpeed.floatValue = 1.0f;
							sObject.ApplyModifiedProperties();
						});
				}
				else
				{
					menu.AddItem(new GUIContent("Speed"), true, () =>
						{
							sSpeed.floatValue = 0.0f;
							sObject.ApplyModifiedProperties();
						});
				}

				menu.AddSeparator("");

				foreach (var methodPrefab in methodPrefabs)
				{
					var root = methodPrefab.transform;

					menu.AddItem(new GUIContent("Prefab/" + methodPrefab.name), false, () =>
						{
							sRoot.objectReferenceValue = root;
							sObject.ApplyModifiedProperties();
						});
				}

				menu.AddSeparator("");

				menu.AddItem(new GUIContent("Remove"), false, () =>
					{
						sRoots.GetArrayElementAtIndex(index).objectReferenceValue = null;
						sRoots.DeleteArrayElementAtIndex(index);
						sObject.ApplyModifiedProperties();
					});

				menu.ShowAsContext();
			}

			EditorGUI.LabelField(rectL, label);

			EditorGUI.PropertyField(rectR, sRoot, GUIContent.none);
		}

		private void DrawAliasTarget(Rect position, SerializedProperty sAlias, SerializedProperty sTarget, System.Type type)
		{
			EditorGUI.showMixedValue = sAlias.hasMultipleDifferentValues || sTarget.hasMultipleDifferentValues;

			if (sTarget.objectReferenceValue == null)
			{
				GUI.color = Color.red;
			}

			EditorGUI.BeginChangeCheck();

			var target = EditorGUI.ObjectField(position, new GUIContent(sAlias.stringValue, ""), sTarget.objectReferenceValue, type, true);

			GUI.color = color;

			EditorGUI.showMixedValue = false;

			if (EditorGUI.EndChangeCheck() == true)
			{
				sTarget.objectReferenceValue = target;
			}
		}

		private void DrawPlay(Rect position, SerializedObject sObject)
		{
			var rectL = position; rectL.xMin += EditorGUIUtility.labelWidth - 15.0f; rectL.width = 14.0f; rectL.yMin += 1.0f; rectL.yMax -= 1.0f;
			var rectR = rectL; rectR.y -= 1.0f;

			if (GUI.Button(rectL, new GUIContent("", "Clicking this will play the transition now.")) == true)
			{
				foreach (var targetObject in sObject.targetObjects)
				{
					((LeanPlayer)fieldInfo.GetValue(targetObject)).Begin();
				}
			}

			GUI.Label(rectR, "▶", EditorStyles.centeredGreyMiniLabel);
		}

		private void DrawAliases(Rect position, SerializedObject sObject, SerializedProperty property)
		{
			var sAliases   = property.FindPropertyRelative("aliases");
			var sTargets   = property.FindPropertyRelative("targets");
			var aliasCount = System.Math.Min(sAliases.arraySize, sTargets.arraySize);
			var aliasTypes = LeanHelper.GetObjectFromSerializedProperty<LeanPlayer>(sObject.targetObject, property).Types;

			for (var i = 0; i < aliasCount; i++)
			{
				DrawAliasTarget(position, sAliases.GetArrayElementAtIndex(i), sTargets.GetArrayElementAtIndex(i), aliasTypes[i]); position.y += height;
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Validate(property);

			color = GUI.color;
			position.height = height;

			var sObject = property.serializedObject;
			var sSpeed  = property.FindPropertyRelative("speed");
			var sRoots  = property.FindPropertyRelative("roots");

			if (sRoots.arraySize > 0)
			{
				DrawPlay(new Rect(position.x, position.y, position.width, height * sRoots.arraySize), sObject);

				for (var i = 0; i < sRoots.arraySize; i++)
				{
					var sRoot = sRoots.GetArrayElementAtIndex(i); DrawRoot(label, position, sObject, sSpeed, sRoots, sRoot, i); position.y += height;
				}
			}

			EditorGUI.indentLevel++;

			if (sSpeed.floatValue > 0.0f)
			{
				EditorGUI.PropertyField(position, sSpeed); position.y += height;
			}

			DrawAliases(position, sObject, property);
			EditorGUI.indentLevel--;

			GUI.color = color;
		}
	}
}
#endif