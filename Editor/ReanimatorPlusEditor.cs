using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Aarthificial.Reanimation.Editor;
using Aarthificial.Reanimation;

namespace jmayberry.ReanimatorHelper.Editor {
	[CustomEditor(typeof(ReanimatorPlus))]
	public class ReanimatorEditor : UnityEditor.Editor {
		private bool _isExpanded;
		private Reanimator _reanimator;

		public override bool RequiresConstantRepaint() {
			return _isExpanded;
		}
		private void OnEnable() {
			_reanimator = target as Reanimator;
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			_isExpanded = EditorGUILayout.Foldout(_isExpanded, "Driver State", true);
			EditorGUI.EndDisabledGroup();

			if (Application.isPlaying && _isExpanded && _reanimator != null) {
				var boxStyle = new GUIStyle("Box") {
					padding = new RectOffset(8, 8, 8, 8),
					margin = new RectOffset(8, 8, 8, 8)
				};

				EditorGUIUtility.labelWidth /= 2;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical(boxStyle);
				EditorGUILayout.LabelField("Previous", EditorStyles.boldLabel);
				EditorGUILayout.Separator();
				foreach (var pair in _reanimator.State)
					EditorGUILayout.IntField(pair.Key, pair.Value);
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical(boxStyle);
				EditorGUILayout.LabelField("Next", EditorStyles.boldLabel);
				EditorGUILayout.Separator();
				foreach (var pair in _reanimator.NextState)
					EditorGUILayout.IntField(pair.Key, pair.Value);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 0;
			}
			else {
				_isExpanded = false;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}