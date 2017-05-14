//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using Ximmerse.InputSystem;

namespace Ximmerse.ThirdParty {

	public class XboxController:ExternalControllerDevice{
		public XboxControllerManager manager;
		public int dwUserIndex;

		public InputTransform inputTransform;
		public XInputPlugin.XINPUT_STATE xinputState;
		public XInputPlugin.XINPUT_VIBRATION xinputVibration;
		public XInputPlugin.XINPUT_BATTERY_INFORMATION xinputBatteryInformation;

		public XboxController(XboxControllerManager manager,string name,int dwUserIndex):base(name) {
			this.manager=manager;
			this.dwUserIndex=dwUserIndex;

			xinputState=new XInputPlugin.XINPUT_STATE();
			xinputVibration=new XInputPlugin.XINPUT_VIBRATION();
			xinputBatteryInformation=new XInputPlugin.XINPUT_BATTERY_INFORMATION();
			//
			if(this.manager.inputTransforms!=null&&this.manager.inputTransforms.Length>1) {
				inputTransform=this.manager.inputTransforms[0];
			}
		}

		public override int GetInputState(ref XDevicePlugin.ControllerState state) {
			int ret=manager.impl.XInputGetState(dwUserIndex,ref xinputState);
			if(ret==0){
				state.handle=this.handle;
				++state.timestamp;
				ResetStatePose(ref state);
				//
				if(manager.impl.XInputGetBatteryInformation!=null) {
					manager.impl.XInputGetBatteryInformation(dwUserIndex,0,ref xinputBatteryInformation);
					XDevicePlugin.SetInt(handle,XDevicePlugin.kField_BatteryLevel,
						(xinputBatteryInformation.BatteryType==XInputPlugin.XINPUT_BATTERY_TYPE.BATTERY_TYPE_DISCONNECTED
						||xinputBatteryInformation.BatteryType==XInputPlugin.XINPUT_BATTERY_TYPE.BATTERY_TYPE_UNKNOWN)?
						-1:
						(int)(100f*(float)xinputBatteryInformation.BatteryLevel/(float)XInputPlugin.XINPUT_BATTERY_LEVEL.BATTERY_LEVEL_FULL)
					);
				}
				//
				if(inputTransform==null) {
					state.buttons=(uint)xinputState.Gamepad.wButtons;
				}else {
					state.buttons=inputTransform.TransformButtons((uint)xinputState.Gamepad.wButtons);
				}
				//
				state.axes[(int)ControllerRawAxis.LeftTrigger]=Mathf.Lerp(0f,1f,MathUtil.Normalize(0,255,xinputState.Gamepad.bLeftTrigger));
				state.axes[(int)ControllerRawAxis.RightTrigger]=Mathf.Lerp(0f,1f,MathUtil.Normalize(0,255,xinputState.Gamepad.bRightTrigger));
				state.axes[(int)ControllerRawAxis.LeftThumbX]=Mathf.Lerp(-1f,1f,MathUtil.Normalize(-32767,32767,xinputState.Gamepad.sThumbLX));
				state.axes[(int)ControllerRawAxis.LeftThumbY]=Mathf.Lerp(-1f,1f,MathUtil.Normalize(-32767,32767,xinputState.Gamepad.sThumbLY));
				state.axes[(int)ControllerRawAxis.RightThumbX]=Mathf.Lerp(-1f,1f,MathUtil.Normalize(-32767,32767,xinputState.Gamepad.sThumbRX));
				state.axes[(int)ControllerRawAxis.RightThumbY]=Mathf.Lerp(-1f,1f,MathUtil.Normalize(-32767,32767,xinputState.Gamepad.sThumbRY));
			}
			return ret;
		}


		public override int SendMessage(int Msg,int wParam,int lParam) {
			switch(Msg) {
				case XDevicePlugin.kMessage_TriggerVibration:
					xinputVibration.wLeftMotorSpeed=
					xinputVibration.wRightMotorSpeed=
					(ushort)((wParam+1)*(65535/(manager.vibrationNumLevels)));
					int ret=manager.impl.XInputSetState(dwUserIndex,ref xinputVibration);
				return 0;
			}
			return base.SendMessage(Msg,wParam,lParam);
		}
	}

	public class XboxControllerManager:ControllerDeviceManager{

		public XInputPlugin.Version pluginVersion=XInputPlugin.Version.Xinput1_3;
		[Tooltip("Start with Level 0")]
		public int vibrationNumLevels=20;
		public XInputPlugin.Impl impl;

		public override bool InitAllControllers() {
			// It only works in windows.
			if(Application.platform!=RuntimePlatform.WindowsEditor &&
			   Application.platform!=RuntimePlatform.WindowsPlayer
			){
				return false;
			}
			//
			impl=new XInputPlugin.Impl(pluginVersion);
			//
			bool ret=base.InitAllControllers();
			if(ret) {
				if(!IsConnected(0)) {
					return false;
				}
				controllers=new XboxController[1] {
					new XboxController(this,"XboxController",0)
				};
			}
			return ret;
		}

		public override bool DeinitAllControllers() {
			if(impl!=null) {
				impl.Dispose();
				impl=null;
			}
			return base.DeinitAllControllers();
		}

		public virtual bool IsConnected(int dwUserIndex) {
			if(impl==null||impl.XInputGetState==null) return false;
			XInputPlugin.XINPUT_STATE state=new XInputPlugin.XINPUT_STATE();
			return impl.XInputGetState(dwUserIndex,ref state)==0;
		}
	}
}
