//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Collections.Generic;
using UnityEngine;
using Ximmerse.InputSystem;

namespace Ximmerse.ThirdParty {

	public class UnityController:ExternalControllerDevice {
		public UnityInputTransform transform;
		public UnityController(string name,UnityInputTransform transform):base(name) {
			this.transform=transform;
			// TODO : 
			if(ControllerInputManager.s_ControllerDatabase==null) {
				ControllerInputManager.s_ControllerDatabase=new Dictionary<string,ControllerInputManager.ControllerInfo>();
			}
			ControllerInputManager.ControllerInfo info;
			if(ControllerInputManager.s_ControllerDatabase.TryGetValue(name,out info)){
			}else {
				info=new ControllerInputManager.ControllerInfo();
				ControllerInputManager.s_ControllerDatabase.Add(name,info);
			}
			info.type=this.transform.type;
			info.priority=this.transform.priority;
		}
		public override int GetInputState(ref XDevicePlugin.ControllerState state) {
			state.handle=this.handle;
			ResetStatePose(ref state);
			transform.TransformState(ref state);
			return 0;
		}
	}

	public class UnityControllerManager:ControllerDeviceManager{

		public override bool InitAllControllers() {
			bool ret=base.InitAllControllers();
			//
			int i=0,imax=inputTransforms.Length;
#if (!UNITY_EDITOR && UNITY_ANDROID)
			// Only gamepad.
			imax=1;
			Log.i("UnityInput","Only gamepad can be enabled on Android.");
#elif (UNITY_5_4||UNITY_5_3||UNITY_5_2||UNITY_5_1)
			// Only gamepad.
			imax=1;
#elif UNITY_5&&UNITY_5_5
			// All controllers.
			// See https://unity3d.com/cn/unity/whats-new/unity-5.5.0
			System.Text.StringBuilder sb=new System.Text.StringBuilder();
			sb.AppendLine("Joysticks:");
			string[] joyNames=Input.GetJoystickNames();
			for(int j=0,jmax=joyNames.Length;j<jmax;++j) {
				sb.AppendLine("\t"+joyNames[j]);
			}
			Log.i("UnityInput",sb.ToString());
#endif
			controllers=new ExternalControllerDevice[imax];
			for(;i<imax;++i) {
				controllers[i]=new UnityController(inputTransforms[i].name,inputTransforms[i] as UnityInputTransform);
			}
			return ret;
		}
	}

}