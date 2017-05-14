//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

using Gvr.Internal;
using Ximmerse.InputSystem;

namespace Ximmerse.ThirdParty {

	class XDeviceControllerProvider:IControllerProvider{
		
		protected ControllerInput[] m_ControllerInputs;
		protected ControllerInput m_ControllerInput=null;

		internal XDeviceControllerProvider() {
			m_ControllerInputs=new ControllerInput[2]{
				ControllerInputManager.GetInput(ControllerType.LeftController),
				ControllerInputManager.GetInput(ControllerType.RightController),
			};
		}

		public virtual void OnPause(){}
		public virtual void OnResume(){}

		public virtual void ReadState(ControllerState outState) {
			//
			if(m_ControllerInput!=null) {
				if(m_ControllerInput.connectionState!=DeviceConnectionState.Connected){// If not connected.
					m_ControllerInput=null;
				}
			}
			if(m_ControllerInput==null) {
				// Search available controller...
				for(int i=0,imax=m_ControllerInputs.Length;i<imax;++i) {
					if(m_ControllerInputs[i]!=null||m_ControllerInputs[i].connectionState==DeviceConnectionState.Connected) {
						m_ControllerInput=m_ControllerInputs[i];
						break;
					}
				}
				if(m_ControllerInput==null) {
					outState.connectionState=GvrConnectionState.Disconnected;
					return;
				}
			}
			//
			outState.connectionState=GvrConnectionState.Connected;//(GvrConnectionState)m_ControllerInput.connectionState;
			// Touchpad
			outState.touchPos=m_ControllerInput.GetTouchPos();
			outState.isTouching=m_ControllerInput.GetButton(DaydreamButton.Touch);
			outState.touchDown=m_ControllerInput.GetButtonDown(DaydreamButton.Touch);
			outState.touchUp=m_ControllerInput.GetButtonUp(DaydreamButton.Touch);
			// Click Button
			outState.clickButtonState=m_ControllerInput.GetButton(DaydreamButton.Click);
			outState.clickButtonDown=m_ControllerInput.GetButtonDown(DaydreamButton.Click);
			outState.clickButtonUp=m_ControllerInput.GetButtonUp(DaydreamButton.Click);
			// App Button
			outState.appButtonState=m_ControllerInput.GetButton(DaydreamButton.App);
			outState.appButtonDown=m_ControllerInput.GetButtonDown(DaydreamButton.App);
			outState.appButtonUp=m_ControllerInput.GetButtonUp(DaydreamButton.App);
			// Home Button
			outState.recentering=m_ControllerInput.GetButton(DaydreamButton.Home);
			outState.recentered=m_ControllerInput.GetButtonUp(DaydreamButton.Home);
			  // Fix recenter api.
			if(m_ControllerInput.GetButtonUp(DaydreamButton.Home)) {
				m_ControllerInput.Recenter();
			}
			// Motions
			outState.orientation=m_ControllerInput.GetRotation();
			outState.accel=m_ControllerInput.GetAccelerometer();
			outState.gyro=m_ControllerInput.GetGyroscope();
		}
	}
}

#endif