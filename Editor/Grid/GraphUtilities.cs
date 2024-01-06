using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.GraphNodes;
using static Codice.Client.BaseCommands.WkStatus.Printers.StatusChangeInfo;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;
using Aarthificial.Reanimation.Common;
using Aarthificial.Reanimation.Cels;

namespace jmayberry.ReanimatorHelper.Utilities {
	public static class GraphUtilities {
		public static Button CreateButton(string text, Action onClick = null, int width = 0) {
			Button button = new Button(onClick) {
				text = text
			};

			if (width > 0) {

				button.style.width = width;
			}

			return button;
		}

		public static Foldout CreateFoldout(string title, bool collapsed = false) {
			Foldout foldout = new Foldout() {
				text = title,
				value = !collapsed
			};

			return foldout;
		}

		public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null, int flexGrow = -1) {
			TextField textField = new TextField() {
				value = value,
				label = label
			};

			if (flexGrow != -1) {
				textField.style.flexGrow = flexGrow;
			}

			if (onValueChanged != null) {
				textField.RegisterValueChangedCallback(onValueChanged);
			}

			return textField;
		}

		public static IntegerField CreateIntegerField(int value = 0, string label = null, EventCallback<ChangeEvent<int>> onValueChanged = null, int flexGrow = -1) {
			IntegerField textField = new IntegerField() {
				value = value,
				label = label
			};

			if (flexGrow != -1) {
				textField.style.flexGrow = flexGrow;
			}

			if (onValueChanged != null) {
				textField.RegisterValueChangedCallback(onValueChanged);
			}

			return textField;
		}

		public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null) {
			TextField textArea = CreateTextField(value, label, onValueChanged);
			textArea.multiline = true;
			return textArea;
		}

		public static string GetCurrentFolderPath() {
			if ((Selection.activeObject == null) || (Selection.activeObject is not GameObject)) {
				if (AssetDatabase.IsValidFolder("Assets/Sprites")) {
					return "Assets/Sprites";
				}

				return "Assets";
			}

			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (AssetDatabase.IsValidFolder(path)) {
				return path;
			}

			return Path.GetDirectoryName(path);
		}
	}
	public static class ReadableNodeUtilities {
		public static string GetName(ControlDriver controlDriver) {
			var field = controlDriver.GetType().GetField("name", BindingFlags.NonPublic | BindingFlags.Instance);
			return (string)field?.GetValue(controlDriver);
		}

		public static void SetName(ControlDriver controlDriver, string value) {
			var field = controlDriver.GetType().GetField("name", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(controlDriver, value);
		}

		public static bool GetAutoIncrement(ControlDriver controlDriver) {
			var field = controlDriver.GetType().GetField("autoIncrement", BindingFlags.NonPublic | BindingFlags.Instance);
			return (bool)field?.GetValue(controlDriver);
		}

		public static void SetAutoIncrement(ControlDriver controlDriver, bool value) {
			var field = controlDriver.GetType().GetField("autoIncrement", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(controlDriver, value);
		}

		public static bool GetPercentageBased(ControlDriver controlDriver) {
			var field = controlDriver.GetType().GetField("percentageBased", BindingFlags.NonPublic | BindingFlags.Instance);
			return (bool)field?.GetValue(controlDriver);
		}

		public static void SetPercentageBased(ControlDriver controlDriver, bool value) {
			var field = controlDriver.GetType().GetField("percentageBased", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(controlDriver, value);
		}

		public static string GetGuid(ControlDriver controlDriver) {
			var field = controlDriver.GetType().GetField("guid", BindingFlags.NonPublic | BindingFlags.Instance);
			return (string)field?.GetValue(controlDriver);
		}

		public static string GetGuid(SwitchNode node) {
			ControlDriver controlDriver = GetControlDriver(node);
			return GetGuid(controlDriver);
		}

		public static string GetGuid(SimpleAnimationNode node) {
			ControlDriver controlDriver = GetControlDriver(node);
			return GetGuid(controlDriver);
		}

		public static string GetGuid(MirroredAnimationNode node) {
			ControlDriver controlDriver = GetControlDriver(node);
			return GetGuid(controlDriver);
		}

		public static ReanimatorNode[] GetNodes(SwitchNode node) {
			var field = node.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
			return (ReanimatorNode[])field?.GetValue(node);
		}

		public static void SetNodes(SwitchNode node, ReanimatorNode[] value) {
			var field = node.GetType().GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static SimpleCel[] GetCels(SimpleAnimationNode node) {
			var field = node.GetType().GetField("cels", BindingFlags.NonPublic | BindingFlags.Instance);
			return (SimpleCel[])field?.GetValue(node);
		}

		public static void SetCels(SimpleAnimationNode node, SimpleCel[] value) {
			var field = node.GetType().GetField("cels", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static MirroredCel[] GetCels(MirroredAnimationNode node) {
			var field = node.GetType().GetField("cels", BindingFlags.NonPublic | BindingFlags.Instance);
			return (MirroredCel[])field?.GetValue(node);
		}

		public static void SetCels(MirroredAnimationNode node, MirroredCel[] value) {
			var field = node.GetType().GetField("cels", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static DriverDictionary GetDriverDictionary(SwitchNode node) {
			var field = node.GetType().GetField("drivers", BindingFlags.NonPublic | BindingFlags.Instance);
			return (DriverDictionary)field?.GetValue(node);
		}

		public static DriverDictionary GetDriverDictionary(SimpleAnimationNode node) {
			var field = node.GetType().GetField("drivers", BindingFlags.NonPublic | BindingFlags.Instance);
			return (DriverDictionary)field?.GetValue(node);
		}

		public static DriverDictionary GetDriverDictionary(MirroredAnimationNode node) {
			var field = node.GetType().GetField("drivers", BindingFlags.NonPublic | BindingFlags.Instance);
			return (DriverDictionary)field?.GetValue(node);
		}

		public static void SetDriverDictionary(SwitchNode node, DriverDictionary value) {
			var field = node.GetType().GetField("drivers", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static void SetDriverDictionary(SimpleAnimationNode node, DriverDictionary value) {
			var field = node.GetType().GetField("drivers", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static void SetDriverDictionary(MirroredAnimationNode node, DriverDictionary value) {
			var field = node.GetType().GetField("drivers", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static ControlDriver GetControlDriver(SwitchNode node) {
			var field = node.GetType().GetField("controlDriver", BindingFlags.NonPublic | BindingFlags.Instance);
			return field?.GetValue(node) as ControlDriver;
		}

		public static ControlDriver GetControlDriver(SimpleAnimationNode node) {
			var field = node.GetType().GetField("controlDriver", BindingFlags.NonPublic | BindingFlags.Instance);
			return field?.GetValue(node) as ControlDriver;
		}

		public static ControlDriver GetControlDriver(MirroredAnimationNode node) {
			var field = node.GetType().GetField("controlDriver", BindingFlags.NonPublic | BindingFlags.Instance);
			return field?.GetValue(node) as ControlDriver;
		}

		public static void SetControlDriver(SwitchNode node, ControlDriver value) {
			var field = node.GetType().GetField("controlDriver", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static void SetControlDriver(SimpleAnimationNode node, ControlDriver value) {
			var field = node.GetType().GetField("controlDriver", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}

		public static void SetControlDriver(MirroredAnimationNode node, ControlDriver value) {
			var field = node.GetType().GetField("controlDriver", BindingFlags.NonPublic | BindingFlags.Instance);
			field?.SetValue(node, value);
		}
	}
}
