using UnityEngine;
using Ximmerse.InputSystem;

public class ControllerMonitor:MonoBehaviour {

	#region Nested Types
	
	public enum MonitorField {
		None,
		Yaw,
		Pitch,
		Roll,
	}

	[System.Serializable]
	public class Condition {
		public MonitorField field;
		public UnityEngine.Rendering.CompareFunction compareFunc;
		public float value;

		public bool Evaluate(float v) {
			switch(compareFunc) {
				case UnityEngine.Rendering.CompareFunction.Always:
				return true;
				case UnityEngine.Rendering.CompareFunction.Less:
				return v<value;
				case UnityEngine.Rendering.CompareFunction.LessEqual:
				return v<=value;
				case UnityEngine.Rendering.CompareFunction.Equal:
				return v==value;
				case UnityEngine.Rendering.CompareFunction.NotEqual:
				return v!=value;
				case UnityEngine.Rendering.CompareFunction.GreaterEqual:
				return v>=value;
				case UnityEngine.Rendering.CompareFunction.Greater:
				return v>value;
				case UnityEngine.Rendering.CompareFunction.Never:
				default:
				return false;
			}
		}

		public bool Evaluate(ref Vector3 position,ref Vector3 yawPitchRoll) {
			switch(field) {
				case MonitorField.Yaw:
				return Evaluate(yawPitchRoll.y);
				case MonitorField.Pitch:
				return Evaluate(yawPitchRoll.x);
				case MonitorField.Roll:
				return Evaluate(yawPitchRoll.z);
				default:
				return false;
			}
		}
	}

	[System.Serializable]
	public class State {
		public string name;
		public Condition[] conditions=new Condition[0];

		public bool Evaluate(ref Vector3 position,ref Vector3 yawPitchRoll) {
			for(int i=0,imax=conditions.Length;i<imax;++i) {
				if(!conditions[i].Evaluate(ref position,ref yawPitchRoll)) {
					return false;
				}
			}
			return true;
		}
	}

	#endregion Nested Types

	#region Static

	public static Vector3 GetYawPitchRoll(Quaternion q){
		Vector3 yawPitchRoll=q.eulerAngles;
		//
		if(yawPitchRoll.x>180.0f) {
			yawPitchRoll.x=yawPitchRoll.x-360.0f;
		}
		if(yawPitchRoll.z>180.0f) {
			yawPitchRoll.z=yawPitchRoll.z-360.0f;
		}
		//
		return yawPitchRoll;
	}
	
	#endregion Static

	#region Fields

	[SerializeField]protected ControllerType m_Controller=ControllerType.LeftController;
	[System.NonSerialized]protected ControllerInput m_ControllerInput;
	[System.NonSerialized]protected Vector3 m_Position;
	[System.NonSerialized]protected Vector3 m_YawPitchRoll; 
	[SerializeField]protected int m_CurrentStateId=-1;
	[SerializeField]protected State[] m_States=new State[0];

	#endregion Fields

	#region Unity Messages

	protected virtual void Start() {
		m_ControllerInput=ControllerInputManager.instance.GetControllerInput(m_Controller);
	}

	protected virtual void Update() {
		if(m_ControllerInput!=null) {
			OnPoseUpdated(
				m_ControllerInput.GetPosition(),
				GetYawPitchRoll(m_ControllerInput.GetRotation())
			);
			XDevicePlugin.ControllerState state=m_ControllerInput.GetState();
			XDevicePlugin.ControllerState prevState=m_ControllerInput.GetPrevState();
			OnInputUpdated(
				prevState.buttons,
				state.buttons,
				IsShiftMode()
			);
		}
	}

	#endregion Unity Messages

	#region Methods

	/// <summary>
	/// 
	/// </summary>
	public virtual bool IsShiftMode() {
		return false;
	} 

	/// <summary>
	/// 
	/// </summary>
	public virtual void OnPoseUpdated(Vector3 position,Vector3 yawPitchRoll) {
		m_Position=position;m_YawPitchRoll=yawPitchRoll;
		int stateId=-1;
		//
		for(int i=0,imax=m_States.Length;i<imax;++i) {
			if(m_States[i].Evaluate(ref position,ref yawPitchRoll)) {
				stateId=i;
				break;
			}
		}
		//
		if(stateId!=m_CurrentStateId) {
			OnStateChanged(m_CurrentStateId,stateId);
			//
			m_CurrentStateId=stateId;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void OnInputUpdated(uint prev_buttons,uint buttons,bool isShift=false) {
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void OnStateChanged(int oldState,int newState) {
	}

	#endregion Methods

}