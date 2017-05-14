//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
//using Ximmerse.CrossInput;
using Ximmerse.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ButtonEvent {

	#region Nested Types

	public enum Type {
		Unknown,
		Input_Button,
		Input_Key,
		//CrossInput,
		ControllerInput,
	}

	[System.Serializable]
	public class PersistentListener {

		public static readonly System.Func<bool> GetFalse=(()=>(false));

		public Type type;
		public string stringArg;
		public int intArg;
		[System.NonSerialized]public System.Func<bool> Get     = GetFalse;
		[System.NonSerialized]public System.Func<bool> GetDown = GetFalse;
		[System.NonSerialized]public System.Func<bool> GetUp   = GetFalse;

		public void Initialize(ButtonEvent buttonEvent) {
			switch(type) {
				case Type.Input_Button:
					Get     = ()=>(Input.GetButton(stringArg));
					GetDown = ()=>(Input.GetButtonDown(stringArg));
					GetUp   = ()=>(Input.GetButtonUp(stringArg));
				break;
				case Type.Input_Key:
					Get     = ()=>(Input.GetKey((KeyCode)intArg));
					GetDown = ()=>(Input.GetKeyDown((KeyCode)intArg));
					GetUp   = ()=>(Input.GetKeyUp((KeyCode)intArg));
				break;
				//case Type.CrossInput:
				//	Get     = ()=>(CrossInputManager.GetButton(stringArg));
				//	GetDown = ()=>(CrossInputManager.GetButtonDown(stringArg));
				//	GetUp   = ()=>(CrossInputManager.GetButtonUp(stringArg));
				//break;
				case Type.ControllerInput:
					ControllerInput input=ControllerInputManager.instance.GetControllerInput(stringArg);
					if(input==null) goto default_label;
					Get     = ()=>(input.GetButton((ControllerButton)intArg));
					GetDown = ()=>(input.GetButtonDown((ControllerButton)intArg));
					GetUp   = ()=>(input.GetButtonUp((ControllerButton)intArg));
				break;
				default:
				default_label:
					Get     = GetFalse;
					GetDown = GetFalse;
					GetUp   = GetFalse;
				break;
			}
		}

	}
	
#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(PersistentListener))]
	public class PersistentListenerDrawer:SimplePropertyDrawer{

		public override void OnGUI(Rect position,SerializedProperty property,GUIContent label) {
			switch((Type)(property.FindPropertyRelative("type").intValue)) {
				case Type.Input_Button:
				case Type.Input_Key:
				//case Type.CrossInput:
					numFields=2;
				break;
				case Type.ControllerInput:
					numFields=3;
				break;
				default:
					numFields=1;
				break;
			}
			base.OnGUI(position,property,label);
		}

		protected override void OnDraw(SerializedProperty property,Rect[] rects) {
			int i=0,intArg;
			EditorGUI.PropertyField(rects[i++],property.FindPropertyRelative("type"),s_EmptyLabel);
			switch((Type)(property.FindPropertyRelative("type").intValue)) {
				case Type.Input_Button:
				//case Type.CrossInput:
					if(numFields<2) return;
					EditorGUI.PropertyField(rects[i++],property.FindPropertyRelative("stringArg"),s_EmptyLabel);
				break;
				case Type.Input_Key:
					if(numFields<2) return;
					EditorGUI.BeginChangeCheck();
						intArg=(int)(KeyCode)EditorGUI.EnumPopup(rects[i++],s_EmptyLabel,(KeyCode)(property.FindPropertyRelative("intArg").intValue));
					if(EditorGUI.EndChangeCheck()) {
						property.FindPropertyRelative("intArg").intValue=intArg;
					}
				break;
				case Type.ControllerInput:
					if(numFields<3) return;
					EditorGUI.PropertyField(rects[i++],property.FindPropertyRelative("stringArg"),s_EmptyLabel);
					EditorGUI.BeginChangeCheck();
						string s=property.FindPropertyRelative("stringArg").stringValue.ToLower();
						if(s.StartsWith("left")||s.StartsWith("right")) {
							intArg=(int)(DaydreamButton)EditorGUI.EnumPopup(rects[i++],s_EmptyLabel,(DaydreamButton)(property.FindPropertyRelative("intArg").intValue));
						}else{
							intArg=(int)(ControllerButton)EditorGUI.EnumPopup(rects[i++],s_EmptyLabel,(ControllerButton)(property.FindPropertyRelative("intArg").intValue));
						}
					if(EditorGUI.EndChangeCheck()) {
						property.FindPropertyRelative("intArg").intValue=intArg;
					}
				break;
				default:
				break;
			}
		}
	}
	
#endif

	#endregion Nested Types

	#region Fields

	[SerializeField]protected PersistentListener[] m_PersistentListeners=new PersistentListener[0];
	[System.NonSerialized]protected bool m_IsInited=false;

	#endregion Fields

	#region Constructors

	public ButtonEvent() {
	}

	public ButtonEvent(params PersistentListener[] listeners) {
		m_PersistentListeners=listeners;
	}

	#endregion Constructors

	#region Methods

	protected void EnsureInitialized() {
		if(!m_IsInited) {
			m_IsInited=true;
			for(int i=0,imax=m_PersistentListeners.Length;i<imax;++i) {
				m_PersistentListeners[i].Initialize(this);
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public bool Get() {
		EnsureInitialized();
		for(int i=0,imax=m_PersistentListeners.Length;i<imax;++i) {
			if(!m_PersistentListeners[i].Get()) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	public bool GetAny() {
		EnsureInitialized();
		for(int i=0,imax=m_PersistentListeners.Length;i<imax;++i) {
			if(m_PersistentListeners[i].Get()) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	public bool GetAnyDown() {
		EnsureInitialized();
		for(int i=0,imax=m_PersistentListeners.Length;i<imax;++i) {
			if(m_PersistentListeners[i].GetDown()) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	public bool GetAnyUp() {
		EnsureInitialized();
		for(int i=0,imax=m_PersistentListeners.Length;i<imax;++i) {
			if(m_PersistentListeners[i].GetUp()) {
				return true;
			}
		}
		return false;
	}

	#endregion Methods

}
