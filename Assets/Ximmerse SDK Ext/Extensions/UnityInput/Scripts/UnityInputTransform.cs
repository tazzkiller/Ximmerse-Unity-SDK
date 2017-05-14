//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using Ximmerse.InputSystem;
using UnityEngine.VR;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ximmerse.ThirdParty {

	public class UnityInputTransform:InputTransform{

		#region Nested Types

		[System.Serializable]
		public class Axis {
			public int joyNum=0;
			public int axis=0;
			public bool invert=false;
			
			[System.NonSerialized]protected bool m_NameCached=false;
			[System.NonSerialized]protected string m_Name=string.Empty;

			public string name {
				get {
					if(!m_NameCached){
						m_NameCached=true;
						//
						m_Name=string.Format("Joystick{0}Axis{1}{2}",
							joyNum>0?joyNum.ToString():"",
							axis,
							invert?"-Inv":""
						);
					}
					return m_Name;
				}
			}
		}

#if UNITY_EDITOR

		[CustomPropertyDrawer(typeof(Axis))]
		public class AxisDrawer:SimplePropertyDrawer {
			[System.NonSerialized]protected string[] m_JoyNumName;
			[System.NonSerialized]protected string[] m_AxisName;

			public override void OnGUI(Rect position,SerializedProperty property,GUIContent label) {
				if(m_JoyNumName==null) {
					int i=9;
					m_JoyNumName=new string[i];
					while(i-->0) {
						m_JoyNumName[i]="Joystick"+((i==0)?"":i.ToString());
					}
				}
				if(m_AxisName==null) {
					int i=20;
					m_AxisName=new string[i];
					while(i-->0) {
						m_AxisName[i]="Axis"+i.ToString();
					}
				}
				//
				numFields=3;
				base.OnGUI(position,property,label);
			}
			protected override void OnDraw(SerializedProperty property,Rect[] rects) {
				SerializedProperty field;
				int intValue;
				//
				EditorGUI.LabelField(rects[0],property.name);
				rects[1]=MathUtil.MergeRects(rects[1],rects[2]);
				rects[1].width-=(12+40+4);
				rects[1].width*=.5f;
				//
				GUI.changed=false;
					intValue=EditorGUI.Popup(
						rects[1],
						(field=property.FindPropertyRelative("joyNum")).intValue,m_JoyNumName
					);
				if(GUI.changed){field.intValue=intValue;}
				//
				rects[1].x+=rects[1].width+4;
				//
				GUI.changed=false;
					intValue=EditorGUI.Popup(
						rects[1],
						(field=property.FindPropertyRelative("axis")).intValue,m_AxisName
					);
				if(GUI.changed){field.intValue=intValue;}
				//
				EditorGUI.LabelField(
					new Rect(rects[2].position+new Vector2(rects[2].width-12-40,0),new Vector2(40,rects[2].height)),
					"Invert"
				);
				EditorGUI.PropertyField(
					new Rect(rects[2].position+new Vector2(rects[2].width-12,0),new Vector2(12,rects[2].height)),
					property.FindPropertyRelative("invert"),s_EmptyLabel
				);
			}
		}

#endif

		#endregion Nested Types

		#region Fields

		public static readonly float AXIS_AS_BUTTON_THRESHOLD = 0.5f;
		
		[Header("Unity")]
		public ControllerType type=ControllerType.Gamepad;
		public int priority;
		public VRNode node=VRNode.Head;
		[Header("Axes")]
		public Axis LeftThumbstickX  = new Axis{axis= 0,           };
		public Axis LeftThumbstickY  = new Axis{axis= 1,invert=true};
		public Axis RightThumbstickX = new Axis{axis= 2,           };
		public Axis RightThumbstickY = new Axis{axis= 3,invert=true};
		public Axis LeftTrigger      = new Axis{axis=12,           };
		public Axis RightTrigger     = new Axis{axis=11,           };
		[Header("Dpad")]
		public Axis DpadX            = new Axis{axis= 4,           };
		public Axis DpadY            = new Axis{axis= 5,invert=true};
		
		//
		public string DpadEnumType="Ximmerse.InputSystem.ControllerButton";
		public string[] DpadEnumKeys=new string[4] {
			"DpadUp",
			"DpadDown",
			"DpadLeft",
			"DpadRight"
		};
		
		[System.NonSerialized]protected bool m_TempMembersCached=false;
		[System.NonSerialized]protected KeyCode[] m_KeyCodes;
		[System.NonSerialized]protected int[] m_DpadMasks;

		#endregion Fields

		#region Methods

		protected virtual void CacheTempMembers() {
			//
			if(m_TempMembersCached) {
				return;
			}
			m_TempMembersCached=true;
			//
			if(m_KeyCodes==null) {
				int i=0,imax=buttonMasksFrom.Length;
				m_KeyCodes=new KeyCode[imax];
				for(;i<imax;++i) {
					m_KeyCodes[i]=(KeyCode)buttonMasksFrom[i];
				}
			}
			//
			if(m_DpadMasks==null) {
				int i=0,imax=4;
				m_DpadMasks=new int[imax];
				System.Type enumType=System.Type.GetType(DpadEnumType);
				if(enumType!=null) {
					for(;i<imax;++i) {
						m_DpadMasks[i]=(int)System.Enum.Parse(enumType,DpadEnumKeys[i]);
					}
				}
			}
		}

		public virtual uint GetButtons(){
			//
			if(!m_TempMembersCached) {
				CacheTempMembers();
			}
			//
			uint buttons=0;
			int i=0,imax=m_KeyCodes.Length;
			for(;i<imax;++i) {
				if(Input.GetKey(m_KeyCodes[i])){
					buttons|=(uint)buttonMasksTo[i];
				}
			}
			// Dpad
			float x=Input.GetAxis(DpadX.name);
			float y=Input.GetAxis(DpadY.name);
			i=0;
			// DpadUp
			if(y>= AXIS_AS_BUTTON_THRESHOLD){
				buttons|=(uint)m_DpadMasks[i];
			}++i;
			// DpadDown
			if(y<=-AXIS_AS_BUTTON_THRESHOLD){
				buttons|=(uint)m_DpadMasks[i];
			}++i;
			// DpadLeft
			if(x<=-AXIS_AS_BUTTON_THRESHOLD){
				buttons|=(uint)m_DpadMasks[i];
			}++i;
			// DpadRight
			if(x>= AXIS_AS_BUTTON_THRESHOLD){
				buttons|=(uint)m_DpadMasks[i];
			}++i;
			return buttons;
		}

		public virtual void TransformState(ref XDevicePlugin.ControllerState state) {
			//
			state.buttons=GetButtons();
			//
			int i=0;
			state.axes[i++]=Input.GetAxis(LeftTrigger.name);
			state.axes[i++]=Input.GetAxis(RightTrigger.name);
			state.axes[i++]=Input.GetAxis(LeftThumbstickX.name);
			state.axes[i++]=Input.GetAxis(LeftThumbstickY.name);
			state.axes[i++]=Input.GetAxis(RightThumbstickX.name);
			state.axes[i++]=Input.GetAxis(RightThumbstickY.name);
			//
			Vector3 pos=InputTracking.GetLocalPosition(node);
			Quaternion rot=InputTracking.GetLocalRotation(node);
			//
			TrackingResult trackingResult=TrackingResult.NotTracked;
			if(pos!=Vector3.zero) {
				trackingResult|=TrackingResult.PositionTracked;
			}
			if(rot!=Quaternion.identity) {
				trackingResult|=TrackingResult.RotationTracked;
			}
			XDevicePlugin.SetInt(state.handle,XDevicePlugin.kField_TrackingResult,(int)trackingResult);
			//
			i=0;
			state.position[i]= pos[i];++i;
			state.position[i]= pos[i];++i;
			state.position[i]=-pos[i];++i;
			i=0;
			state.rotation[i]=-rot[i];++i;
			state.rotation[i]=-rot[i];++i;
			state.rotation[i]= rot[i];++i;
			state.rotation[i]= rot[i];++i;
		}
		
		#endregion Methods

	}
}