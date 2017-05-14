//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

namespace Ximmerse.VR{

	public class VRDevice:MonoBehaviour {

		#region Fields

		[Header("VR")]
		public string family;
		public TrackingOrigin trackingOriginType=TrackingOrigin.EyeLevel;
		[Tooltip("Vector from the tracking origin to the neck pivot point.")]
		public Vector3 neckPosition;
		[Tooltip("Vector from the neck pivot point to the point between the eyes.")]
		public Vector3 neckOffset;
		public bool isCenterEye;

		public string methodIsPresent="[Type].UnityEngine.VR.VRDevice,UnityEngine.dll.[Property].isPresent";
		[System.NonSerialized]protected System.Func<bool> m_MethodIsPresent;
		public string methodRefreshRate="[Type].UnityEngine.VR.VRDevice,UnityEngine.dll.[Property].refreshRate";
		[System.NonSerialized]protected System.Func<float> m_MethodRefreshRate;
		public string methodRecenter="[Type].UnityEngine.VR.InputTracking,UnityEngine.dll.[Method].Recenter";
		[System.NonSerialized]protected System.Action m_MethodRecenter;
		[System.NonSerialized]protected bool m_MethodRecenterCached;

		[Header("Editor")]
		public string androidTargetName;
		public TextAsset androidManifest;
		public bool useUnityVR=false;
		
		[System.NonSerialized]protected Transform m_CenterEyeAnchor;

		#endregion Fields

		#region Methods

		/// <summary>
		/// Initialize this device.
		/// </summary>
		public virtual void InitDevice(VRContext context) {
			// Replace the center eye if needed.
			if(isCenterEye) {
				Transform t=transform;
				Transform trackingSpace=context.GetAnchor(VRNode.TrackingSpace,null);
				Transform centerEye=context.GetAnchor(VRNode.CenterEye,null);
				if(trackingSpace!=null) {
					t.SetParent(trackingSpace);
					t.localPosition=Vector3.zero;
					t.localRotation=Quaternion.identity;
					t.localScale=Vector3.one;
				}
				if(centerEye!=null) {
					// TODO : You may have a lot of work to do.
					foreach(Transform c in centerEye) {
						c.SetParent(t,false);
					}
					centerEye.gameObject.SetActive(false);
					//
					Log.d("VRDevice","Replace origin center eye.");
				}
				context.SetAnchor(VRNode.CenterEye,t,true);
			}
			//
			m_MethodIsPresent=ReflectUtil.ParseDelegate<System.Func<bool>>(methodIsPresent);
			m_MethodRefreshRate=ReflectUtil.ParseDelegate<System.Func<float>>(methodRefreshRate);
			if(m_MethodRefreshRate==null) {
				float f;
				if(float.TryParse(methodRefreshRate,out f)){
					m_MethodRefreshRate=()=>(f);
				}
			}
			if(useUnityVR) {// TODO : more method？
				Log.i("VRDevice","UnityVR loads a device(model = \""+UnityEngine.VR.VRDevice.model+"\").");
				switch(UnityEngine.VR.VRDevice.model) {
					case "Oculus Rift DK2":
						m_MethodRefreshRate=()=>(75.0f);
					break;
#if !UNITY_EDITOR && UNITY_ANDROID
					default:// Cardboard,Gear VR and Daydream.
						m_MethodRefreshRate=()=>(60.0f);
					break;
#endif
				}
			}
		}

		public virtual void OnVRContextInited(VRContext context) {
			m_CenterEyeAnchor=context.GetAnchor(VRNode.CenterEye,null);
		}

		public virtual Quaternion GetRotation() {
			if(m_CenterEyeAnchor==null) {
				return Quaternion.identity;
			}else if(useUnityVR) {// TODO : more method？
				return UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
			}else {
				return m_CenterEyeAnchor.localRotation;
			}
		}

		/// <summary>
		/// Successfully detected a VR device in working order.
		/// </summary>
		public virtual bool isPresent {
			get{
				return m_MethodIsPresent==null?false:m_MethodIsPresent();
			}
		}

		/// <summary>
		/// Refresh rate of the display in Hertz.
		/// </summary>
		public virtual float refreshRate {
			get{
				return m_MethodRefreshRate==null?Application.targetFrameRate:m_MethodRefreshRate();
			}
		}

		/// <summary>
		/// Center tracking to the current position and orientation of the HMD.
		/// </summary>
		public virtual void Recenter() {
			if(!m_MethodRecenterCached) {
				m_MethodRecenterCached=true;
				//
				if(m_MethodRecenter==null) {
					m_MethodRecenter=ReflectUtil.ParseDelegate<System.Action>(methodRecenter);
				}
			}
			if(m_MethodRecenter!=null) {
				m_MethodRecenter();
			}
		}

		#endregion Methods

	}

}