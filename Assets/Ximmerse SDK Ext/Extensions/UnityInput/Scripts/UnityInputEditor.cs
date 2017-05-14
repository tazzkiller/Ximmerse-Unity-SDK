//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Ximmerse.ThirdParty {

	/// <summary>
	/// A utility for configing Unity Input Setting.
	/// </summary>
	[InitializeOnLoad]
	public class UnityInputEditor{

		static UnityInputEditor() {
			UnityInputTransform axes=AssetDatabase.LoadAssetAtPath<UnityInputTransform>("Assets/Ximmerse SDK/Extensions/UnityInput/Others/Unity-Gamepad.asset");
			if(axes==null) axes=AssetDatabase.LoadAssetAtPath<UnityInputTransform>("Assets/Ximmerse SDK Ext/Extensions/UnityInput/Others/Unity-Gamepad.asset");
			if(axes!=null) {
				BindAxes(axes);
			}
		}

		// <!-- Taken from OVR/Moonlight/Editor/OVRMoonlightLoader.cs

		public class Axis {
			public string name = string.Empty;
			public string descriptiveName = string.Empty;
			public string descriptiveNegativeName = string.Empty;
			public string negativeButton = string.Empty;
			public string positiveButton = string.Empty;
			public string altNegativeButton = string.Empty;
			public string altPositiveButton = string.Empty;
			public float gravity = 0.0f;
			public float dead = 0.001f;
			public float sensitivity = 1.0f;
			public bool snap = false;
			public bool invert = false;
			public int type = 2;
			public int axis = 0;
			public int joyNum = 0;

			public void CopyTo(SerializedProperty other) {
				other.FindPropertyRelative("m_Name").stringValue = this.name;
				other.FindPropertyRelative("descriptiveName").stringValue = this.descriptiveName;
				other.FindPropertyRelative("descriptiveNegativeName").stringValue = this.descriptiveNegativeName;
				other.FindPropertyRelative("negativeButton").stringValue = this.negativeButton;
				other.FindPropertyRelative("positiveButton").stringValue = this.positiveButton;
				other.FindPropertyRelative("altNegativeButton").stringValue = this.altNegativeButton;
				other.FindPropertyRelative("altPositiveButton").stringValue = this.altPositiveButton;
				other.FindPropertyRelative("gravity").floatValue = this.gravity;
				other.FindPropertyRelative("dead").floatValue = this.dead;
				other.FindPropertyRelative("sensitivity").floatValue = this.sensitivity;
				other.FindPropertyRelative("snap").boolValue = this.snap;
				other.FindPropertyRelative("invert").boolValue = this.invert;
				other.FindPropertyRelative("type").intValue = this.type;
				other.FindPropertyRelative("axis").intValue = this.axis;
				other.FindPropertyRelative("joyNum").intValue = this.joyNum;
			}
		}

		public static void BindAxis(Axis axis) {
			SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
			SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

			SerializedProperty axisIter = axesProperty.Copy();
			axisIter.Next(true);
			axisIter.Next(true);
			while (axisIter.Next(false))
			{
				if (axisIter.FindPropertyRelative("m_Name").stringValue == axis.name)
				{
					// Axis already exists. Don't create binding.
					return;
				}
			}

			axesProperty.arraySize++;
			serializedObject.ApplyModifiedProperties();

			SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
			axis.CopyTo(axisProperty);
			serializedObject.ApplyModifiedProperties();
			//
			//EditorUtility.SetDirty(axisProperty);
		}

		// Taken from OVR/Moonlight/Editor/OVRMoonlightLoader.cs--》
	

		public static void BindAxes(UnityInputTransform axes) {
			if(axes==null){return;}
			//
			BindAxis(axes.LeftThumbstickX);
			BindAxis(axes.LeftThumbstickY);
			BindAxis(axes.RightThumbstickX);
			BindAxis(axes.RightThumbstickY);
			BindAxis(axes.LeftTrigger);
			BindAxis(axes.RightTrigger);
			//
			BindAxis(axes.DpadX);
			BindAxis(axes.DpadY);
		}

		public static void BindAxis(UnityInputTransform.Axis axis) {
			if(axis==null){return;}
			//
			BindAxis(new Axis{
				name=axis.name,
				joyNum=axis.joyNum,
				axis=axis.axis,
				invert=axis.invert,
			});
		}
	}

}

#endif