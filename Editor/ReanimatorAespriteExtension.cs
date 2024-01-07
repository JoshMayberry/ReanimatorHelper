using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

using Aarthificial.Reanimation.Cels;
using Aarthificial.Reanimation.Nodes;
using Aarthificial.Reanimation.Common;
using Aarthificial.Reanimation.Editor.Nodes;
using static PlasticPipe.Server.MonitorStats;
using UnityEngine.WSA;

namespace jmayberry.ReanimatorHelper.Editor {

	[CustomEditor(typeof(SimpleAnimationNode))]
	public class AespriteSimpleAnimationNodeEditor : AnimationNodeEditor {
		readonly static Regex trailingNumbersRegex = new Regex(@"(\s*\d+$)");

		[MenuItem("Assets/Create/Reanimator/Simple Animation (From Sprites)", false, 410)]
		private static void CreateFromAsepriteSprites() {
			var sprites = Selection.objects.OfType<Sprite>().ToArray();

			var cels = sprites.Select(sprite => new SimpleCel(sprite)).ToArray();
			SaveSimpleAnimationNode(cels, sprites[0].name);
		}

		[MenuItem("Assets/Create/Reanimator/Simple Animation (From Sprites)", true, 410)]
		private static bool CreateFromAsepriteSpritesValidation() {
			return Selection.objects.OfType<Sprite>().ToArray().Length > 0;
		}

		[MenuItem("Assets/Create/Reanimator/Simple Animation (From Animation Clip)", false, 420)]
		private static void CreateFromAnimationClip() {
			var clip = Selection.activeObject as AnimationClip;
			SaveSimpleAnimationNode(ExtractCelFromClip(clip), clip.name);
		}

		[MenuItem("Assets/Create/Reanimator/Simple Animation (From Animation Clip)", true, 420)]
		private static bool CreateFromAnimationClipValidation() {
			return Selection.activeObject is AnimationClip;
		}

		[MenuItem("Assets/Create/Reanimator/Simple Animations (From Animation Controller)", false, 430)]
		private static void CreateFromAnimationController() {
			var controller = Selection.activeObject as RuntimeAnimatorController;
			foreach (var clip in controller.animationClips) {
				SaveSimpleAnimationNode(ExtractCelFromClip(clip), clip.name);
			}
		}

		[MenuItem("Assets/Create/Reanimator/Simple Animations (From Animation Controller)", true, 430)]
		private static bool CreateFromAnimationControllerValidation() {
			return Selection.activeObject is RuntimeAnimatorController;
		}

		[MenuItem("Assets/Create/Reanimator/Simple Animations (From Aseprite File)", false, 440)]
		private static void CreateFromAsepriteFile() {
			var gameObject = Selection.activeObject as GameObject;
			if (gameObject == null) return;

			var animator = gameObject.GetComponent<Animator>();
			if (animator == null) return;

			var controller = animator.runtimeAnimatorController;
			if (controller == null) return;

			foreach (var clip in controller.animationClips) {
				SaveSimpleAnimationNode(ExtractCelFromClip(clip), clip.name);
			}
		}

		[MenuItem("Assets/Create/Reanimator/Simple Animations (From Aseprite File)", true, 440)]
		private static bool CreateFromAsepriteFileValidation() {
			if (Selection.activeObject is not GameObject) return false;

			var selectedObject = Selection.activeObject;
			if (selectedObject == null) return false;

			string path = AssetDatabase.GetAssetPath(selectedObject);
			return path.EndsWith(".aseprite", StringComparison.OrdinalIgnoreCase);
		}

		private static SimpleCel[] ExtractCelFromClip(AnimationClip clip) {
			var spriteEventsMap = new Dictionary<Sprite, HashSet<string>>();
			var spriteBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip)
				.Where(binding => binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite");

			// Extract sprites and associated events
			foreach (var binding in spriteBindings) {
				ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
				foreach (var frame in keyframes) {
					if (frame.value is Sprite sprite) {
						if (!spriteEventsMap.TryGetValue(sprite, out var events)) {
							events = new HashSet<string>();
							spriteEventsMap[sprite] = events;
						}

						// Extract events
						foreach (var animEvent in AnimationUtility.GetAnimationEvents(clip)) {
							if (animEvent.time == frame.time) {
								events.Add(animEvent.functionName);
							}
						}
					}
				}
			}

			// Create SimpleCel objects
			var cels = new List<SimpleCel>();
			foreach (var spriteEventPair in spriteEventsMap) {
				var drivers = new DriverDictionary();
				foreach (var eventName in spriteEventPair.Value) {
					drivers.keys.Add(eventName);
					drivers.values.Add(0);
				}

				cels.Add(new SimpleCel(spriteEventPair.Key, drivers));
			}

			return cels.ToArray();
		}

		private static void SaveSimpleAnimationNode(SimpleCel[] cels, string name) {
			name = trailingNumbersRegex.Replace(name, "");

			string filepath = AssetDatabase.GetAssetPath(Selection.activeObject);
			string assetFolderPath = GetFolderForAsset(filepath);
			if (!AssetDatabase.IsValidFolder($"{assetFolderPath}/Animations/")) {
				AssetDatabase.CreateFolder(assetFolderPath, "Animations");
			}

			string assetPath = $"{assetFolderPath}/Animations/{name}.asset";
			//string assetPath = $"{assetFolderPath}/Animations/{Path.GetFileNameWithoutExtension(filepath)}_{name}.asset";

			var existingNode = AssetDatabase.LoadAssetAtPath<SimpleAnimationNode>(assetPath);
			if (existingNode == null) {
				var animationNode = SimpleAnimationNode.Create<SimpleAnimationNode>(driver: new ControlDriver("temporary", true), cels: cels);
				animationNode.name = name;
				AssetDatabase.CreateAsset(animationNode, AssetDatabase.GenerateUniqueAssetPath(assetPath));
				AssetDatabase.SaveAssets();
				return;
			}

			UpdateCels(existingNode, cels);
			EditorUtility.SetDirty(existingNode);
			AssetDatabase.SaveAssets();
		}

		private static string GetFolderForAsset(string path) {
			if (string.IsNullOrEmpty(path)) {
				return "Assets";
			}

			if (AssetDatabase.IsValidFolder(path)) {
				return path;
			}

			return Path.GetDirectoryName(path);
		}

		private static void UpdateCels(SimpleAnimationNode existingNode, SimpleCel[] newCels) {
			// cels is protected, so use reflection to modify it
			Type nodeType = typeof(SimpleAnimationNode);
			FieldInfo celsField = nodeType.GetField("cels", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo driversField = typeof(SimpleCel).GetField("drivers", BindingFlags.NonPublic | BindingFlags.Instance);

			SimpleCel[] oldCels = (SimpleCel[])celsField.GetValue(existingNode);
			for (int i = 0; i < newCels.Length; i++) {
				DriverDictionary newDrivers = (DriverDictionary)driversField.GetValue(newCels[i]);
				DriverDictionary mergedDrivers = new DriverDictionary();

				// If the old cels have drivers for this index, merge them
				if (i < oldCels.Length) {
					DriverDictionary oldDrivers = (DriverDictionary)driversField.GetValue(oldCels[i]);

					// Add all old drivers first
					foreach (var key in oldDrivers.keys) {
						int index = oldDrivers.keys.IndexOf(key);
						mergedDrivers.keys.Add(key);
						mergedDrivers.values.Add(oldDrivers.values[index]);
					}

					// Add new drivers, skipping duplicates
					foreach (var key in newDrivers.keys) {
						if (!mergedDrivers.keys.Contains(key)) {
							int index = newDrivers.keys.IndexOf(key);
							mergedDrivers.keys.Add(key);
							mergedDrivers.values.Add(newDrivers.values[index]);
						}
					}
				}
				else {
					// If there are no old drivers, just use the new ones
					mergedDrivers = newDrivers;
				}

				// Update the cel's drivers
				driversField.SetValue(newCels[i], mergedDrivers);
			}
			celsField.SetValue(existingNode, newCels);
		}
	}
}