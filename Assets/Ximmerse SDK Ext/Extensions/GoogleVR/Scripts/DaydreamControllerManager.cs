//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

#if GOOGLE_VR_SDK
using Gvr.Internal;
#endif
using UnityEngine;
using Ximmerse.InputSystem;

namespace Ximmerse.ThirdParty {

	class DaydreamController:ExternalControllerDevice {

#if GOOGLE_VR_SDK

		protected IControllerProvider m_ControllerProvider;
		protected ControllerState m_State;
		public DaydreamController(DaydreamControllerManager owner,string name) : base(name) {
			m_State=new ControllerState();
			//
#if UNITY_EDITOR||UNITY_STANDALONE
			// Use the Controller Emulator.
			m_ControllerProvider=new EmulatorControllerProvider((GvrController.EmulatorConnectionMode)owner.emulatorConnectionMode,owner.enableGyro,
			owner.enableAccel);
#elif UNITY_ANDROID
			// Use the GVR C API.
			m_ControllerProvider=new AndroidNativeControllerProvider(owner.enableGyro, owner.enableAccel);
#else
			// Platform not supported.
			Debug.LogWarning("No controller support on this platform.");
			m_ControllerProvider=new DummyControllerProvider();
#endif
		}

		public override int GetInputState(ref XDevicePlugin.ControllerState state) {
			if(m_ControllerProvider==null) {
				return -1;
			}
			m_ControllerProvider.ReadState(m_State);
			//
			state.axes[(int)ControllerAxis.PrimaryThumbX]=(m_State.touchPos.x-.5f)*2f;
			state.axes[(int)ControllerAxis.PrimaryThumbY]=(m_State.touchPos.y-.5f)*-2f;
			//
			state.buttons=0;
			if(m_State.isTouching) state.buttons|=(uint)DaydreamButton.Touch;
			if(m_State.clickButtonState) state.buttons|=(uint)DaydreamButton.Click;
			if(m_State.appButtonState) state.buttons|=(uint)DaydreamButton.App;
			if(m_State.recentering) state.buttons|=(uint)DaydreamButton.Home;
			//
			int i;
			i=4;while(i-->0){
				state.rotation[i]=m_State.orientation[i];
			}
			i=3;while(i-->0){
				state.accelerometer[i]=m_State.accel[i];
			}
			i=3;while(i-->0){
				state.gyroscope[i]=m_State.gyro[i];
			}
			//
			return 0;
		}

#endif

	}

	class DaydreamControllerManager:ControllerDeviceManager {

		public enum EmulatorConnectionMode {
			OFF,
			USB,
			WIFI,
		}

		/// If true, enable gyroscope on the controller.
		[Tooltip("If enabled, the controller will report gyroscope readings.")]
		public bool enableGyro = false;

		/// If true, enable accelerometer on the controller.
		[Tooltip("If enabled, the controller will report accelerometer readings.")]
		public bool enableAccel = false;
		
		/// Indicates how we connect to the controller emulator.
		[Tooltip("How to connect to the emulator: USB cable (recommended) or WIFI.")]
		public EmulatorConnectionMode emulatorConnectionMode=EmulatorConnectionMode.USB;

		public override bool InitAllControllers() {
#if GOOGLE_VR_SDK
			bool ret=base.InitAllControllers();
			if(ret) {
				controllers=new DaydreamController[1] {
					new DaydreamController(this,"DaydreamController-0")
				};
			}
			return ret;
#else
			XDevicePlugin.RemoveInputDeviceAt(XDevicePlugin.GetInputDeviceHandle("DaydreamController-0"));
			return false;
#endif
		}
	}
}