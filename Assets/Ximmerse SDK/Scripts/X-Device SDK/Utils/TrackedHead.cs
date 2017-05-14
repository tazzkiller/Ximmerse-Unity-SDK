//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using Ximmerse.VR;
using Ximmerse.InputSystem;

/// <summary>
/// For controlling in-game "head" with inside-out tracking or outside-in tracking.
/// </summary>
public class TrackedHead:TrackedObject {

	#region Nested Types

	public enum TrackingType {
		Unknown,
		/// <summary>
		/// Inside Out solution: Xhawk is mounted on HMD. 
		/// </summary>
		Inside_Out,
		/// <summary>
		/// Outside In Solution: Xhawk is mounted on a wall, tripod or something similar. Device setup wise, it is similar to a HTC Vive. 
		/// </summary>
		Outside_In,
		Custom,
	}

	#endregion Nested Types

	#region Fields

	[Header("Head")]

	/// <summary>
	/// TODO : VR
	/// </summary>
	public Transform eyeContainer;
	
	/// <summary>
	/// Which tracking type applied to control the "head".
	/// </summary>
	public TrackingType trackingType=TrackingType.Unknown;
    
	/// <summary>
	/// Only used in TrackingType.Inside_Out,and it must be child of this object's tracking space.
	/// </summary>
	public Transform trackingOrigin;
	
	/// <summary>
	/// The transform of the mark object.
	/// </summary>
	public Transform markTransform;

	[SerializeField]protected GameObject[] m_Gizmos=new GameObject[0];

	[Header("EXT")]
	public string prefsKey="TrackedHead";
	public GameObject uiFixMark;

	#endregion Fields

	#region Unity Messages

    protected override void Awake() {
//#if !UNITY_EDITOR
		SetGizmosActive(false);
//#endif
		//
		source=ControllerType.Hmd;
		trackRotation=false;
		// TODO : Load from runtime.
		//Vector3 offset=markTransform.localPosition;
		//offset.x=PlayerPrefs.GetFloat(prefsKey+".markOffset.x",offset.x);
		//offset.y=PlayerPrefs.GetFloat(prefsKey+".markOffset.y",offset.y);
		//offset.z=PlayerPrefs.GetFloat(prefsKey+".markOffset.z",offset.z);
		//markTransform.localPosition=offset;
		//
		base.Awake();
	}

	protected override void Start() {
		base.Start();
		//
		if(uiFixMark!=null) {
			Transform uiRoot=VRContext.uiRootVR;
			if(uiRoot!=null) {
				GameObject go=Instantiate(uiFixMark);
					Transform t=go.transform,raw=uiFixMark.transform;
					t.SetParent(uiRoot);
					t.localPosition=raw.localPosition;
					t.localRotation=raw.localRotation;
					t.localScale=raw.localScale;
				uiFixMark=go;
				uiFixMark.SendMessage("OnCreate",this,SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	protected virtual void OnVRContextInited(VRContext context) {
		if(eyeContainer==null) {
			eyeContainer=new GameObject("Rotate-Pivot").transform;
			eyeContainer.SetParent(transform);
			//
			eyeContainer.localPosition=Vector3.zero;
			eyeContainer.localRotation=Quaternion.identity;
			eyeContainer.localScale=Vector3.one;
			//
			if(context.vrDevice!=null&&context.vrDevice.family!="Dummy") {
				markTransform.localPosition=markTransform.localPosition+context.vrDevice.neckOffset;
				eyeContainer.localPosition=context.vrDevice.neckOffset;
#if UNITY_EDITOR
				// Editor features.
				if(m_Gizmos.Length>0&&m_Gizmos[0]!=null){
					m_Gizmos[0].transform.localPosition=m_Gizmos[0].transform.localPosition+context.vrDevice.neckOffset;
				}
#endif

			}
		}
		//
		for(int i=0;i<3;++i) {
			Transform eye=context.GetAnchor(VRNode.LeftEye+i,null);
			if(eye!=null) {
				eye.SetParent(eyeContainer,false);
			}
		}
		//
		switch(PlayerPrefsEx.GetInt("XimmerseDevice.type",0)) {
			// No head tracking.
			case 0x1010:
				source=ControllerType.None;
				m_ControllerInput=null;
			break;
		}
	}

		
#if UNITY_EDITOR

	protected virtual void UpdateGizmos() {
		GameObject[] gos=UnityEditor.Selection.gameObjects;
		if(gos.Length>=1) {
			GameObject go=gos[0];
			for(int i=0,imax=m_Gizmos.Length;i<imax;++i) {
				if(m_Gizmos[i]==go) {
					SetGizmosActive(true);
					return;
				}
			}
			SetGizmosActive(go==gameObject);
		}else {
			SetGizmosActive(false);
		}
	}

	protected override void Update() {
		if(Application.isPlaying) {
			if(Camera.current!=null
#if UNITY_5&&!UNITY_5_0&&!UNITY_5_1
				&&Camera.current.cameraType==CameraType.SceneView
#endif
			) {
				UpdateGizmos();
			}else {
				//SetGizmosActive(false);
			}
		}
		base.Update();
	}

	protected virtual void OnDrawGizmos() {
		if(!Application.isPlaying) {
			UpdateGizmos();
		}
	}

	protected virtual void OnDrawGizmosSelected() {
		if(!Application.isPlaying) {
			SetGizmosActive(true);
		}
	}

#endif

	#endregion Unity Messages

	#region Methods

	public virtual void SetGizmosActive(bool value) {
		for(int i=0,imax=m_Gizmos.Length;i<imax;++i) {
			if(m_Gizmos[i]!=null&&m_Gizmos[i].activeSelf!=value) {
				m_Gizmos[i].SetActive(value);
			}
		}
	}

	public override void UpdateTransform(float factor) {
		if(m_ControllerInput==null) {
			return;
		}
		//
		if((m_ControllerInput.trackingResult&TrackingResult.PositionTracked)==0) {
			return;
		}
		//
		Vector3 position=m_ControllerInput.GetPosition();
		//
		Vector3 markOffset=markTransform.localPosition;
		Transform centerEyeAnchor;
		switch(trackingType) {
			case TrackingType.Inside_Out:
				// Solve order : trackingOrigin->[TrackingInput.anchor]->MarkAnchor->HeadAnchor.
				centerEyeAnchor=VRContext.GetAnchor(VRNode.CenterEye);
				if(centerEyeAnchor!=null) {
					position=trackingOrigin.localPosition-centerEyeAnchor.localRotation*(position+markOffset);
				}
			break;
			case TrackingType.Outside_In:
				// Solve order : TrackingInput.GetPosition()->MarkAnchor->HeadAnchor.
				centerEyeAnchor=VRContext.GetAnchor(VRNode.CenterEye);
				if(centerEyeAnchor!=null) {
					position+=centerEyeAnchor.localRotation*(-markOffset);
				}
			break;
		}
		if(factor==1.0f) {
			target.localPosition=position;
		}else {
			target.localPosition=Vector3.Lerp(target.localPosition,position,factor);
		}
    }
	
	#endregion Methods

}
