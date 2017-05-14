//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

#define XDEVICE_RELEASE
using System.Collections.Generic;
using UnityEngine;
using Ximmerse.VR;

namespace Ximmerse.InputSystem {

	public class XHawkInput:TrackerInput{

		#region Nested Types

		[System.Serializable]public class StringIntPair:UKeyValuePair<string,int>{}
#if UNITY_EDITOR
		[UnityEditor.CustomPropertyDrawer(typeof(StringIntPair))]public class StringIntPairDrawer:UKeyValuePairDrawer<string,int>{}
#endif

		#endregion Nested Types

		#region Fields

		[Header("X-Hawk(DK3)")]
		public bool modifyHierarchy=false;
		public Vector3 anchorPosition;
		public Vector3 anchorRotation;
		[Header("Outside-In(DK4)")]
		public GameObject anchorPrefab;
		public bool overridePlugin=true;
		
		[Header("Shared")]
		public ButtonEvent buttonRecenter=new ButtonEvent(
			new ButtonEvent.PersistentListener{
				type=ButtonEvent.Type.ControllerInput,
				stringArg="LeftController",
				intArg=(int)(DaydreamButton.Home)
			},
			new ButtonEvent.PersistentListener{
				type=ButtonEvent.Type.ControllerInput,
				stringArg="RightController",
				intArg=(int)(DaydreamButton.Home)
			}
		);

		public StringIntPair[] controllers=new StringIntPair[3]{
			new StringIntPair{key="XCobra-0",value=0},
			new StringIntPair{key="XCobra-1",value=1},
			new StringIntPair{key="VRDevice",value=2}
		};

		[System.NonSerialized]public XCobraInput[] inputs;
		[System.NonSerialized]protected bool m_IsRequestVR,m_IsLaunched;
		[System.NonSerialized]protected TrackerFrustum m_TrackerFrustum;
		[System.NonSerialized]protected int m_HmdHandle;
		[System.NonSerialized]protected int m_WaitFramesForHmdRecentering=0;
		[System.NonSerialized]protected ControllerInput m_HmdInput;
		[System.NonSerialized]protected Quaternion m_HmdRotationPrev=Quaternion.identity,m_HmdRotationNow=Quaternion.identity;
		[System.NonSerialized]protected List<Matrix4x4> m_AnchorMatrixQueue;
		[System.NonSerialized]protected int m_M2pFrameIndex=-1;

		[System.NonSerialized]protected bool m_AsDaydreamEmulator=false;
		[System.NonSerialized]protected Vector3[] m_HandPosition=new Vector3[2];
		[System.NonSerialized]protected float[] m_HandLength=new float[2];

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			int count=controllers.Length,i=4;
		}

		protected override void Start() {
			// Rewrite the controllers in ControllerInputManager.
			//if() {
			//}

			if(!m_IsLaunched) {
				Launch(false);
			}
			//StartCoroutine("UpdateEof");
		}

		//protected override void Update() {
			//base.Update();
		//}

		protected virtual System.Collections.IEnumerator UpdateEof() {
			var wait=new WaitForEndOfFrame();
			while(true) {
				yield return wait;
				base.Update();
			}
		}

		protected override void OnDestroy() {
			//
			//if(m_Timer!=null) {
			//	m_Timer.Stop();
			//	XHawkExitVirtualFps(m_Timer.onTick);
			//	m_Timer=null;
			//}
			//
			base.OnDestroy();
		}

		#endregion Unity Messages

		#region Methods

		/// <summary>
		/// This function will be called by ControllerInputManager or Start().
		/// XCobra controller's position will be overwritten by this class.
		/// </summary>
		public virtual void Launch(bool checkOthers) {
			if(m_IsLaunched) {
				return;
			}
			m_IsLaunched=true;
			//
			deviceName="XHawk-0";
			if(checkOthers) {
				XHawkInput other=FindAll(deviceName).Find((x)=>(x!=this)) as XHawkInput;
				if(other!=null) {
					// This will be not found by TrackingInput.Find(string).
					gameObject.SetActive(false);
					other.Launch(false);
					//Log.i("XHawkInput","Use the default TrackingInput in scene.");
					return;
				}
			}
			//
			XDevicePlugin.Init();
			if(m_Handle==-1) {
				m_Handle=XDevicePlugin.GetInputDeviceHandle(deviceName);
			}
			m_HmdHandle=XDevicePlugin.GetInputDeviceHandle("VRDevice");
			XDevicePlugin.SetInt(m_Handle,XDevicePlugin.kField_TrackingOrigin,(int)VRContext.trackingOrigin);
			  // Set values to the plugin
			if(overridePlugin) {//&&m_Handle>0) {
				if((XDevicePlugin.GetInt(-1,XDevicePlugin.kField_CtxDeviceVersion,0x4000)&0xF000)==0x4000) {
					Vector3 trackerPos=PlayerPrefsEx.GetVector3("XimmerseTracker[Outside-in].position",new Vector3(0.0f,1.675f,1.5f));
					Vector3 trackerRot=PlayerPrefsEx.GetVector3("XimmerseTracker[Outside-in].rotation",new Vector3(15.0f,180.0f,0.0f));
					XDevicePlugin.SetTrackerPose(m_Handle,trackerPos.y,trackerPos.z,-trackerRot.x);
				}
			}
			// TODO :
			XDevicePlugin.SendMessage(m_Handle,XDevicePlugin.kMessage_RecenterSensor,0,0);
			//
#if XDEVICE_RELEASE
			//if((XDevicePlugin.GetInt(-1,XDevicePlugin.kField_CtxDeviceVersion,0x4000)&0xF000)!=0x4000)
#endif
			{
				int i=0,imax=controllers.Length;
				inputs=new XCobraInput[imax];
				//
				ControllerInputManager mgr=ControllerInputManager.instance;
				ControllerInput ci;
				if(mgr!=null) {
					for(;i<imax;++i) {
						ci=mgr.GetControllerInput(controllers[i].key);
						if(ci is XCobraInput) {
							inputs[i]=ci as XCobraInput;
						}else {
							inputs[i]=new XCobraInput(this,controllers[i].key,controllers[i].value);
							mgr.AddControllerInput(inputs[i].name,inputs[i]);
						}
						//
					}
				}
			}
			m_HmdInput=ControllerInputManager.GetInput(ControllerType.Hmd);
			// VRContext must have a CenterEyeAnchor at least.
			m_IsRequestVR=(VRContext.GetAnchor(VRNode.CenterEye)!=null);
			EnsureAnchor();
			//
			if(XDevicePlugin.GetInt(-1,XDevicePlugin.kField_CtxDeviceVersion,0)==0x1010) {
				XDevicePlugin.SetInt(m_Handle,XDevicePlugin.kField_ConnectionState,(int)DeviceConnectionState.Connected);
				m_AsDaydreamEmulator=true;
				LoadHandConfigs();
			}
			//
			Log.i("XHawkInput","Initialize successfully.");
		}

		/// <summary>
		/// We will lose the VR context,when reloading level.
		/// Calling this function per frame can ensure that the anchor is alive.
		/// </summary>
		public virtual bool EnsureAnchor() {
			// <!-- TODO: VR Legacy Mode. -->
			// If the X-Hawk isn't connected,the game will run as legacy VR mode(Gets input events with GearVR touchpad).
			if(XDevicePlugin.GetInt(m_Handle,XDevicePlugin.kField_ConnectionState,0)!=(int)DeviceConnectionState.Connected) {
				XDevicePlugin.SetInt(m_HmdHandle,XDevicePlugin.kField_ConnectionState,(int)DeviceConnectionState.Disconnected);
				return false;
			}
			//
			if(trackingSpace==null) {
				trackingSpace=VRContext.GetAnchor(VRNode.TrackingSpace);
			}
			//
			if(anchor==null) {
				Transform centerEyeAnchor=VRContext.GetAnchor(VRNode.CenterEye);
				if(m_IsRequestVR&&centerEyeAnchor==null) {
					return false;
				}else {
					//
					switch((XDevicePlugin.GetInt(-1,XDevicePlugin.kField_CtxDeviceVersion,0x4000)&0xF000)) {
						case 0x3000:
							anchor=new GameObject("XHawkAnchor").transform;
							anchor.SetParent(centerEyeAnchor);
							anchor.localPosition=anchorPosition;
							anchor.localRotation=Quaternion.Euler(anchorRotation);
							//
							m_UseAnchorProjection=!PlayerPrefsEx.GetBool("XimmerseTracker[Inside-out].fixShake",false);//!modifyHierarchy;
							VRContext.SetAnchor(VRNode.TrackerDefault,anchor);
							// TODO :
							/*if(modifyHierarchy) {
								for(int i=0;i<2;++i) {
									Transform hand=VRContext.GetAnchor(VRNode.LeftHand+i);
									if(hand!=null) {
										hand.SetParent(anchor,true);
									}
								}
							}*/
							if(PlayerPrefsEx.GetBool("XimmerseTracker[Inside-out].fixShake",false)){
								m_AnchorMatrixQueue=new List<Matrix4x4>(8); 
								m_M2pFrameIndex=Mathf.CeilToInt(PlayerPrefsEx.GetFloat("XimmerseTracker[Inside-out].m2pLatency",0.0f)*VRContext.refreshRate);
								Log.i("XHawkInput","m_M2pFrameIndex="+m_M2pFrameIndex);
							}else {
								m_AnchorMatrixQueue=null;
							}
						break;
						case 0x4000:
							//
							float trackerHeight,trackerDepth,trackerPitch;
							XDevicePlugin.GetTrackerPose(m_Handle,out trackerHeight,out trackerDepth,out trackerPitch);
							//
							anchor=(anchorPrefab==null||!m_IsRequestVR?// in 2D mode.
								new GameObject():
								Object.Instantiate(anchorPrefab)
							).transform;
							anchor.name="TrackerAnchor(X-Hawk DK4)";
							anchor.SetParent(trackingSpace);
							//
#if XDEVICE_RELEASE
							if(true){
							UpdateAnchorFromPlugin();
							}else
#endif
							{
							anchor.localPosition=new Vector3(0f,VRContext.trackingOrigin==TrackingOrigin.FloorLevel?trackerHeight:0.0f,trackerDepth);
							anchor.localRotation=Quaternion.Euler(-trackerPitch,180f,0f);
							}
							//
							m_TrackerFrustum=anchor.GetComponentInChildren<TrackerFrustum>();
							//
//#if XDEVICE_RELEASE
//							m_UseAnchorProjection=false;
//#else
							m_UseAnchorProjection=true;
//#endif
							VRContext.SetAnchor(VRNode.TrackerDefault,anchor);
							m_AnchorMatrixQueue=null;
						break;
						default:
							anchor=new GameObject("Dummy Anchor").transform;
							anchor.SetParent(trackingSpace,false);
						break;
					}
					// Override anchor info.
					if(m_AnchorInfoCached) {
						anchor.localPosition=m_AnchorPosition;
						anchor.localRotation=m_AnchorRotation;
					}
					//
					if(m_IsRequestVR) {
						VRContext.main.onRecenter-=RecenterOnVRContextRecenter;
						VRContext.main.onRecenter+=RecenterOnVRContextRecenter;
					}
				}
			}
			return true;
		}

		protected override void UpdateState() {
			if(Time.frameCount!=m_PrevFrameCount){
			if(EnsureAnchor()) {
				//
				base.UpdateState();
				// TODO : 
				if(m_AnchorMatrixQueue!=null&&m_HmdInput!=null) {
					Matrix4x4 matrix=Matrix4x4.TRS(anchor.position,m_HmdInput.GetRotation()*anchor.localRotation,Vector3.one);
					if(trackingSpace!=null) {
						matrix=trackingSpace.worldToLocalMatrix*matrix;
					}
					if(m_AnchorMatrixQueue.Count>=8) {
						m_AnchorMatrixQueue.RemoveAt(0);
					}
					m_AnchorMatrixQueue.Add(matrix);
				}
				/*if(m_HmdInput!=null) {
					if(m_WaitFramesForHmdRecentering-->0) {
						m_HmdRotationPrev=m_HmdRotationNow=m_HmdInput.GetRotation();
					}else {
						m_HmdRotationPrev=m_HmdRotationNow;
						m_HmdRotationNow=m_HmdInput.GetRotation();
					}
				}*/
			}}
		}

		public override bool Exists(int node) {
			if(m_AsDaydreamEmulator){
				return GetRotation(node)!=Quaternion.identity;
			}else {
				return base.Exists(node);
			}
		}

		public override Vector3 GetPosition(int node) {
			if(m_AsDaydreamEmulator){
				return EmulatePosition(VRNode.LeftHand+node);
			}else if(m_AnchorMatrixQueue!=null){
				Vector3 position=base.GetPosition(node);
				position=m_AnchorMatrixQueue[m_AnchorMatrixQueue.Count-(m_AnchorMatrixQueue.Count<m_M2pFrameIndex?1:m_M2pFrameIndex)].MultiplyPoint3x4(position);
				return position;
			}else{
				return base.GetPosition(node);
			}
		}

		public override Quaternion GetRotation(int node) {
			UpdateState();
			//
			if(inputs==null) {
				return Quaternion.identity;
			}
			//
			ControllerInput input=null;
			for(int i=0,imax=inputs.Length;i<imax;++i) {
				if(inputs[i]!=null){if(inputs[i].node==node) {
					input=inputs[i];
					break;
				}}
			}
			//
			if(input==null) {
				return Quaternion.identity;
			}else {
				return input.GetRotation();
			}
		}

		public virtual void RecenterOnVRContextRecenter() {
			//
			if(m_HmdInput!=null) {
				m_WaitFramesForHmdRecentering=2;
			}
			for(int i=0,imax=inputs.Length;i<imax;++i) {
				inputs[i].OnVRContextRecenter();
			}
			//
			if(buttonRecenter.Get()){
				Recenter();
			}
		}

		[ContextMenu("Recenter Tracker")]
		public override void Recenter() {
			switch((XDevicePlugin.GetInt(-1,XDevicePlugin.kField_CtxDeviceVersion,0x4000)&0xF000)) {
				case 0x3000:
				break;
				case 0x4000:
					Recenter_DK4();
				break;
			}
			//
			if(onRecenter!=null) {
				onRecenter.Invoke();
			}
		}

		[System.NonSerialized]protected bool m_AnchorInfoCached=false;
		[System.NonSerialized]protected Vector3 m_AnchorPosition;
		[System.NonSerialized]protected Quaternion m_AnchorRotation;
		public virtual void Recenter_DK4() {
			XDevicePlugin.SendMessage(m_Handle,XDevicePlugin.kMessage_RecenterSensor,1,0);
			UpdateAnchorFromPlugin();
			m_AnchorInfoCached=false;
			//
			if(m_TrackerFrustum!=null) {
				m_TrackerFrustum.OnTrackerRecenter();
			}
		}

		
		[System.NonSerialized]protected float[] m_UpdateAnchorFromPluginTRS=new float[3+4+3];
		public virtual void UpdateAnchorFromPlugin(){
			XDevicePlugin.GetObject(m_Handle,XDevicePlugin.kField_TRS,m_UpdateAnchorFromPluginTRS,0);
			if(true) {
				anchor.localPosition=new Vector3(
					 m_UpdateAnchorFromPluginTRS[0],
					 m_UpdateAnchorFromPluginTRS[1],
					-m_UpdateAnchorFromPluginTRS[2]
				);
				anchor.localRotation=new Quaternion(
					-m_UpdateAnchorFromPluginTRS[3],
					-m_UpdateAnchorFromPluginTRS[4],
					 m_UpdateAnchorFromPluginTRS[5],
					 m_UpdateAnchorFromPluginTRS[6]
				);
			}
		}

		public virtual void LoadHandConfigs(){
			int i=0;
			//
			m_HandPosition[i]=PlayerPrefsEx.GetVector3("Daydream.leftHand.position",new Vector3(-0.2f,-0.5f,0.0f));
			m_HandLength[i]=PlayerPrefsEx.GetFloat("Daydream.leftHand.length",0.0f);
			++i;
			m_HandPosition[i]=PlayerPrefsEx.GetVector3("Daydream.rightHand.position",new Vector3( 0.2f,-0.5f,0.0f));
			m_HandLength[i]=PlayerPrefsEx.GetFloat("Daydream.rightHand.length",0.0f);
			++i;
		}

		public virtual Vector3 EmulatePosition(VRNode node) {
			switch(node) {
				case VRNode.LeftHand:
				case VRNode.RightHand:
					Vector3 pos=m_HandPosition[(int)(node-VRNode.LeftHand)];
					Transform centerEyeAnchor=VRContext.GetAnchor(VRNode.CenterEye);
					if(centerEyeAnchor!=null) {
						Transform trackingSpace=VRContext.GetAnchor(VRNode.TrackingSpace);
						Quaternion bodyRotation=Quaternion.Euler(new Vector3(0.0f,centerEyeAnchor.eulerAngles.y,0.0f));
						pos=centerEyeAnchor.position+bodyRotation*pos+
							((trackingSpace==null?Quaternion.identity:trackingSpace.rotation)*GetRotation((int)(node-VRNode.LeftHand)))*
							(Vector3.forward*m_HandLength[(int)(node-VRNode.LeftHand)]);
						// TODO : 
						if(trackingSpace!=null) {
							pos=trackingSpace.InverseTransformPoint(pos);
						}
					}else {
						pos=pos+GetRotation((int)(node-VRNode.LeftHand))*(Vector3.forward*m_HandLength[(int)(node-VRNode.LeftHand)]);
					}
					return pos;
				break;
				case VRNode.LeftHand+2:
					return Vector3.up*(VRContext.trackingOrigin==TrackingOrigin.EyeLevel?0:// Keep zero.
						XDevicePlugin.GetFloat(m_Handle,XDevicePlugin.kField_TrackerHeight,0.0f));// From ground.
				break;
				default:
					return Vector3.zero;
				break;
			}
		}

		public virtual Vector3 GetHmdRotationOffset() {
			return m_HmdRotationNow*new Vector3(0,0.075f,0.0805f)-m_HmdRotationPrev*new Vector3(0,0.075f,0.0805f);
		}

		#endregion Methods

	}
}
