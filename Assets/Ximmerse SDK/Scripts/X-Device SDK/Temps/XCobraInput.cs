//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

namespace Ximmerse.InputSystem{

	/// <summary>
	/// 
	/// </summary>
	public class XCobraInput:ControllerInput {

		public TrackerInput trackerInput;
		public XHawkInput xHawkInput;
		public int node;

		/*/ <!-- TODO : Fix Inside-out Shake.
		[System.NonSerialized]protected bool m_FixInsideOutShake;
		[System.NonSerialized]protected int m_FixedFrameCount;

		[System.NonSerialized]protected float m_HmdSleepVelocity=0.0010f;
		[System.NonSerialized]protected float m_ControllerSleepVelocity=0.1f;
		[System.NonSerialized]protected float m_LerpFactor=0.25f;

		[System.NonSerialized]protected float m_FadeMaxTime=0.125f;
		[System.NonSerialized]protected float m_FadeTime=0.0f;

		[System.NonSerialized]protected TrackingResult m_PrevTrackingResult;
		[System.NonSerialized]protected Vector3 m_PrevPosition;
		*/// TODO : Fix Inside-out Shake. -->

		public XCobraInput(TrackerInput trackingInput,string name,int defaultNode):base(name) {
			this.trackerInput=trackingInput;
			this.node=defaultNode;
			if(this.trackerInput is XHawkInput) {
				xHawkInput=this.trackerInput as XHawkInput;
				/*/ <!-- TODO : Fix Inside-out Shake.
				m_FixInsideOutShake=(XDevicePlugin.GetInt(-1,XDevicePlugin.kField_CtxDeviceVersion,0)&0xF000)==0x3000&&
					PlayerPrefsEx.GetBool("XimmerseTracker[Inside-out].fixShake",false);
				*/// TODO : Fix Inside-out Shake. -->
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void UpdateState() {
			if(Time.frameCount!=m_PrevFrameCount){
				base.UpdateState();
				// TODO : Assign blob id dynamically.
				if(m_State.state_mask!=0) {
					node=(m_State.state_mask&0xff)-1;
				}
			}
		}

		#region TODO : State is HACKED

		public override DeviceConnectionState connectionState {
			get {
				if(trackerInput!=null) {
					if(trackerInput.Exists(node)) {
						return DeviceConnectionState.Connected;
					}
				}
				return base.connectionState;
			}
		}


		public override TrackingResult trackingResult {
			get {
				if(trackerInput!=null) {
					if(trackerInput.Exists(node)) {
						return base.trackingResult|(TrackingResult.PositionTracked);
					}else {
						return base.trackingResult&(~TrackingResult.PositionTracked);
					}
				}
				return base.trackingResult;
			}
		}

		#endregion TODO : State is HACKED

		public override Vector3 GetPosition() {
			/*/ <!-- TODO : Fix Inside-out Shake.
			if(m_FixInsideOutShake) {
				// Return cached result.
				if(m_FixedFrameCount==Time.frameCount) {
					return m_PrevPosition;
				}
				//
				m_FixedFrameCount=Time.frameCount;
				//Log.d("XCobraInput",xHawkInput.GetHmdRotationOffset().magnitude.ToString("0.0000"));
				  // NOTE : Break from smooth sampling.
				if((m_PrevTrackingResult&TrackingResult.PositionTracked)==0||
					GetGyroscope().sqrMagnitude>m_ControllerSleepVelocity*m_ControllerSleepVelocity) {
					m_PrevPosition=trackerInput.GetPosition(node);
					m_FadeTime=0.0f;// Don't fade.
				  // NOTE : Smooth sampling.
				}else if(xHawkInput.GetHmdRotationOffset().sqrMagnitude>m_HmdSleepVelocity*m_HmdSleepVelocity&&
					GetGyroscope().sqrMagnitude<=m_ControllerSleepVelocity*m_ControllerSleepVelocity) {
					m_PrevPosition=Vector3.Lerp(m_PrevPosition,trackerInput.GetPosition(node),m_LerpFactor);
					m_FadeTime=m_FadeMaxTime;
				  // NOTE : Fade(from Smooth to Default) sampling.
				}else if(m_FadeTime>0.0f){
					m_FadeTime=Mathf.Clamp01(m_FadeTime-Time.deltaTime);
					m_PrevPosition=Vector3.Lerp(m_PrevPosition,trackerInput.GetPosition(node),m_LerpFactor+
						(1.0f-m_LerpFactor)*(1.0f-m_FadeTime/m_FadeMaxTime));
				  // NOTE : Default sampling.
				}else{
					m_PrevPosition=trackerInput.GetPosition(node);
				}
				m_PrevTrackingResult=trackingResult;
				return m_PrevPosition;
			}
			*/// TODO : Fix Inside-out Shake. -->
			// Lost tracking...
			if((trackingResult&TrackingResult.PositionTracked)==0) {
				return Vector3.zero;
			}
			if(trackerInput!=null) {
				return trackerInput.GetPosition(node);
			}
			//
			return Vector3.zero;
		}

		public override Quaternion GetRotation() {
			return base.GetRotation();
		}

		public virtual void OnVRContextRecenter() {
			/*/ <!-- TODO : Fix Inside-out Shake.
			if(m_FixInsideOutShake) {
				m_FadeTime=0.0f;// Don't fade.
			}
			*/// TODO : Fix Inside-out Shake. -->
		}
	}
}
