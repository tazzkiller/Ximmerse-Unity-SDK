//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

#if STEAM_VR_SDK
using Valve.VR;
#endif
using UnityEngine;
using Ximmerse.InputSystem;

namespace Ximmerse.ThirdParty {

	public class SteamVRController:ExternalControllerDevice {

#if STEAM_VR_SDK
		public static uint TransformButton(InputTransform transform,ulong ulButtonPressed,ulong ulButtonTouched) {
			uint ret=0;
			for(int i=0;i<transform.maxButtons;++i) {
				switch((EVRButtonId)transform.buttonMasksFrom[i]) {
					case EVRButtonId.k_EButton_SteamVR_Touchpad:
					case EVRButtonId.k_EButton_SteamVR_Trigger:
						if((ulButtonTouched&(1u<<transform.buttonMasksFrom[i]))!=0) {
							ret|=(uint)transform.buttonMasksTo[i];
						}
					break;
					default:
						if((ulButtonPressed&(1u<<transform.buttonMasksFrom[i]))!=0) {
							ret|=(uint)transform.buttonMasksTo[i];
						}
					break;
				}
			}
			return ret;
		}

		protected uint m_Index;
		protected InputTransform m_InputTransform;
		protected TrackedDevicePose_t m_Pose;
		protected VRControllerState_t m_State;

		public SteamVRController(string name,int index,InputTransform inputTransform):base(name) {
			m_Index=(uint)index;
			m_InputTransform=inputTransform;
			// Write the controller's properties
			XDevicePlugin.SetBool(handle,XDevicePlugin.kField_IsAbsRotation,true);
		}
		
		public override int GetInputState(ref XDevicePlugin.ControllerState state){
			CVRSystem system=OpenVR.System;
			if(system!=null){
				int i;
				if(/*valid=*/system.GetControllerStateWithPose(SteamVR_Render.instance.trackingSpace, m_Index,ref m_State,ref m_Pose)) {
					//
					i=0;
					state.axes[i++]=m_State.rAxis1.x; // trigger
					++i;
					state.axes[i++]=m_State.rAxis0.x;
					state.axes[i++]=m_State.rAxis0.y;
					++i;
					++i;

					if(m_InputTransform!=null) {
						state.buttons=TransformButton(m_InputTransform,m_State.ulButtonPressed,m_State.ulButtonTouched);
					}else {
						// You needs an InputTransform to convert ulong buttons to uint buttons.
						state.buttons=0;
					}
					//
					SteamVR_Utils.RigidTransform t=new SteamVR_Utils.RigidTransform(m_Pose.mDeviceToAbsoluteTracking);
					for(i=0;i<3;++i) {
						state.position[i]=t.pos[i];
					}
					state.position[i]=1.0f;
					for(i=0;i<4;++i) {
						state.rotation[i]=t.rot[i];
					}
					return 0;
				}
			}
			return -1;
		}
#endif

	}

	public class SteamVRControllerManager:ControllerDeviceManager{

		public string controllerFormat="SteamVRController-{0}";

		public override bool InitAllControllers() {
			// SteamVR only works in windows.
			if(Application.platform!=RuntimePlatform.WindowsEditor &&
			   Application.platform!=RuntimePlatform.WindowsPlayer
			){
				return false;
			}
#if STEAM_VR_SDK
			//
			bool ret=base.InitAllControllers();
			if(ret) {
				if(!OpenVRInterop.IsRuntimeInstalled()||
				   !OpenVRInterop.IsHmdPresent()) {
					ret=false;
				}
				if(ret){
					if(inputTransforms.Length==0) {
						inputTransforms=new InputTransform[1]{null};
					}
					controllers=new SteamVRController[maxControllers];
					for(int i=0;i<maxControllers;++i) {
						controllers[i]=new SteamVRController(
							string.Format(controllerFormat,i),i,
							inputTransforms.Length==1?inputTransforms[0]:inputTransforms[i]
						);
					}
				}
			}
			return ret;
#else
			Log.d("SteamVRControllerManager","Not Support!!!If you want to enable this feature,please follow these steps:\n"+
			"    1) You need HTC Vive or other devices which can be developed by OpenVR,and remember SteamVR only works in pc.\n"+
			"    2) Import \"SteamVR Plugin.unitypackage\"(you can download it from \"https://www.assetstore.unity3d.com/en/#!/content/32647\").\n"+
			"    3) Add \"STEAM_VR_SDK\" to [Scripting Define Symbols] in [Player Settings](menu:Edit>Project Settings>Player).\n");
			return false;
#endif
		}
	}

}