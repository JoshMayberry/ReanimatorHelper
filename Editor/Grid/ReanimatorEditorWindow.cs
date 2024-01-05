using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jmayberry.ReanimatorHelper.Editor {
    // Use: [Unity Dialogue System](https://www.youtube.com/playlist?list=PL0yxB6cCkoWK38XT4stSztcLueJ_kTx5f)
    public class ReanimatorEditorWindow : EditorWindow {
		protected float splitterPos = 250f;
		protected bool isResizing;
		protected Rect splitterRect;
		protected TriggerArea triggerArea;
		private ReanimatorGraphView reanimatorGraph;

		[MenuItem("Window/Reanimator")]
		public static void ShowWindow() {
			GetWindow<ReanimatorEditorWindow>("Reanimator");
		}

		protected void OnEnable() {
			triggerArea = new TriggerArea();

			reanimatorGraph = new ReanimatorGraphView(this) {
				name = "Reanimator Graph"
			};
		}

		protected void OnGUI() {
			EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			Rect graphRect = new Rect(splitterPos + 6, 0, position.width - 3, position.height);
			GUILayout.BeginArea(graphRect);
			DrawGraph(graphRect);
			GUILayout.EndArea();

			triggerArea.DrawArea(splitterPos);
			DrawSplitter();

			EditorGUILayout.EndHorizontal();
			ProcessEvents(Event.current);
			if (GUI.changed) Repaint();
		}

		protected void DrawSplitter() {
			splitterRect = new Rect(splitterPos, 0, 2, position.height);
			EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
			GUI.DrawTexture(splitterRect, EditorGUIUtility.whiteTexture);
		}

		protected void DrawGraph(Rect rect) {
			if (reanimatorGraph.parent == null) {
				rootVisualElement.Add(reanimatorGraph);
			}

			reanimatorGraph.style.position = Position.Absolute;
			reanimatorGraph.style.left = rect.xMin;
			reanimatorGraph.style.top = rect.yMin;
			reanimatorGraph.style.width = rect.width;
			reanimatorGraph.style.height = rect.height;
		}

		protected void ProcessEvents(Event e) {
			switch (e.type) {
				case EventType.MouseDown:
					if (splitterRect.Contains(e.mousePosition)) {
						isResizing = true;
					}
					break;
				case EventType.MouseUp:
					isResizing = false;
					break;
				case EventType.MouseDrag:
					if (isResizing) {
						splitterPos = e.mousePosition.x;
						Repaint();
					}
					break;
			}
		}
	}

	public class TriggerArea {
		public void DrawArea(float width) {
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.ExpandHeight(true));
			GUILayout.Label("Triggers", EditorStyles.boldLabel);
			EditorGUILayout.EndVertical();
		}
	}
}
