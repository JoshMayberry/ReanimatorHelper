// Code from: https://forum.unity.com/threads/truncated-filename-in-project-window.405866/#post-9341405

// TODO: Move to CustomAttributes module

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace jmayberry.Tools.ProjectWindowNames {
	[InitializeOnLoad]
	public static class ProjectWindowShowFullAssetName {
		static GUIStyle style;
		static GUIContent assetGUIContent;
		static Color32 backgroundColor;
		static Color32 selectedFrameColor;
		private static bool isEnabled = true;

		[MenuItem("Tools/Show Full Names in Project View")]
		private static void ToggleCustomProjectView() {
			isEnabled = !isEnabled;
			Menu.SetChecked("Tools/Show Full Names in Project View", isEnabled);
			EditorApplication.RepaintProjectWindow(); // Refresh the project window to reflect changes
		}

		[MenuItem("Custom Tools/Toggle Custom Project View", true)]
		private static bool ToggleCustomProjectViewValidate() {
			Menu.SetChecked("Tools/Show Full Names in Project View", isEnabled);
			return true;
		}

		static ProjectWindowShowFullAssetName() {
			EditorApplication.projectWindowItemInstanceOnGUI -= Draw;
			EditorApplication.projectWindowItemInstanceOnGUI += Draw;
			style = new GUIStyle {
				fontSize = 10,
				wordWrap = true,
				alignment = TextAnchor.UpperCenter,
				margin = new RectOffset(0, 0, 0, 0),
			};
			assetGUIContent = new GUIContent();
			if (EditorGUIUtility.isProSkin) {
				backgroundColor = new Color32(51, 51, 51, 255);
				selectedFrameColor = new Color32(44, 93, 135, 255);
			}
			else {
				backgroundColor = new Color32(190, 190, 190, 255);
				selectedFrameColor = new Color32(58, 114, 176, 255);
			}
		}

		static int lastFrameCount;
		static bool isRenaming;
		static void Draw(int instanceID, Rect selectionRect) {
			if (!isEnabled) return;

			if (lastFrameCount != Time.frameCount) {
				isRenaming = IsRenaming();
				lastFrameCount = Time.frameCount;
			}
			if (isRenaming) return;
			if (selectionRect.height <= 20) return;

			var path = AssetDatabase.GetAssetPath(instanceID);
			if (string.IsNullOrWhiteSpace(path)) return;

			// Checking if it is a sub-asset (like frames in an Aseprite file)
			if (AssetDatabase.IsSubAsset(instanceID)) {
				HandleSubAsset(instanceID, selectionRect);
				return; // Skip further processing for sub-assets
			}

			// Existing logic for normal assets
			var assetName = Path.GetFileNameWithoutExtension(path);
			RenderAssetLabel(assetName, instanceID, selectionRect);
		}

		// Handle drawing of sub-assets (like frames in an Aseprite file)
		static void HandleSubAsset(int instanceID, Rect selectionRect) {
			var parentAssetPath = AssetDatabase.GetAssetPath(instanceID);
			var parentAsset = AssetDatabase.LoadMainAssetAtPath(parentAssetPath);

			// Getting all sub-assets of the parent asset
			var subAssets = AssetDatabase.LoadAllAssetsAtPath(parentAssetPath);

			foreach (var subAsset in subAssets) {
				if (subAsset.GetInstanceID() == instanceID) {
					// Found the specific sub-asset, now display its name
					var subAssetName = subAsset.name; // This should give the specific name of the sub-asset
					RenderAssetLabel(subAssetName, instanceID, selectionRect);
					break;
				}
			}
		}

		// Refactored method to render asset label
		static void RenderAssetLabel(string assetName, int instanceID, Rect selectionRect) {
			var icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(instanceID));

			assetGUIContent.text = assetName;
			float textHeight = style.CalcHeight(assetGUIContent, selectionRect.width);

			// Adjust the rectangle height to fit the text
			var nameRect = new Rect(selectionRect.x, selectionRect.yMax - textHeight, selectionRect.width, textHeight);

			bool selected = Selection.instanceIDs.Contains(instanceID);

			if (EditorGUIUtility.isProSkin) {
				style.normal.textColor = Color.white;
			}
			else {
				style.normal.textColor = selected ? Color.white : Color.black;
			}

			var selectedFrameRect = new Rect(nameRect.x - 6, nameRect.y - 1, nameRect.width + 12, textHeight + 3);

			EditorGUI.DrawRect(selectedFrameRect, selected ? selectedFrameColor : backgroundColor);
			EditorGUI.DrawRect(new Rect(selectedFrameRect.xMax - 1, selectedFrameRect.y, 1, selectedFrameRect.height), backgroundColor);
			GUI.Label(nameRect, assetName, style);

			if (icon) {
				var iconRect = new Rect(nameRect.x, nameRect.y - 15, 14, 14);
				EditorGUI.DrawRect(iconRect, new Color(0, 0, 0, 0.5f));
				GUI.DrawTexture(iconRect, icon);
			}
		}

		static private Assembly asm;
		static private EditorWindow projectBrowserWindow;
		static private Type typeProjectBrowser;
		static bool IsRenaming() {
			if (asm == null) asm = Assembly.Load("UnityEditor.dll");
			var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
			if (typeProjectBrowser == null) typeProjectBrowser = asm.GetType("UnityEditor.ProjectBrowser");
			if (projectBrowserWindow == null) projectBrowserWindow = EditorWindow.GetWindow(typeProjectBrowser, false, null, false);
			var m_ListAreaState = projectBrowserWindow.GetType().GetField("m_ListAreaState", flag).GetValue(projectBrowserWindow);
			var m_RenameOverlay = m_ListAreaState.GetType().GetField("m_RenameOverlay", flag).GetValue(m_ListAreaState);
			return (bool)m_RenameOverlay.GetType().GetField("m_IsRenaming", flag).GetValue(m_RenameOverlay);
		}
	}
}