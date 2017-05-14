//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;
using Ximmerse.VR;
using Ximmerse.InputSystem;
using Ximmerse.UI;

public class GuidePlayer:MonoBehaviour {

	#region Nested Types
	
	[System.Serializable]
	public class FistsToStartSettings{
		[System.NonSerialized]public bool enabled;
		[System.NonSerialized]public System.Action onStart;
		public ButtonEvent buttonSkip=new ButtonEvent(
			new ButtonEvent.PersistentListener{type=ButtonEvent.Type.Input_Button,stringArg="Fire1"},
			new ButtonEvent.PersistentListener{type=ButtonEvent.Type.ControllerInput,stringArg="LeftController",intArg=(int)DaydreamButton.Touch},
			new ButtonEvent.PersistentListener{type=ButtonEvent.Type.ControllerInput,stringArg="RightController",intArg=(int)DaydreamButton.Touch}
		);

		public GameObject displayObject;
		//public YawCopier yawObject;
		public float childSize=0.025f;
		public string childReadyMsg;
		public Transform[] children=new Transform[2];

		[System.NonSerialized]protected bool[] m_ChildIsReady=new bool[2];
		[System.NonSerialized]protected GameObject[] m_ChildReadyObjects=new GameObject[2];
		[System.NonSerialized]protected Transform[] m_Fists=new Transform[2];

		public virtual void StartListen() {
			enabled=true;
			//
			if(displayObject!=null) displayObject.SetActive(true);
			for(int i=0,imax=children.Length;i<imax;++i) {
				Transform t=children[i].transform.Find("IsReady");
				if(t!=null) {
					m_ChildReadyObjects[i]=t.gameObject;
				}
			}

			TrackedObject[] objs=FindObjectsOfType<TrackedObject>();
			for(int i=0,imax=objs.Length;i<imax;++i) {
				if(true) {
					switch(objs[i].source) {
						case ControllerType.LeftController:
							if(m_Fists[0]==null) m_Fists[0]=objs[i].target;
						break;
						case ControllerType.RightController:
							if(m_Fists[1]==null) m_Fists[1]=objs[i].target;
						break;
					}
				}
			}
			//if(yawObject!=null) {
			//	/*
			//	yawObject.source=m_Fists[0];
			//	yawObject.subSource=m_Fists[1];
			//	*/
			//	Destroy(yawObject);
			//}
		}

		public virtual void StopListen() {
			enabled=false;
			//
			if(displayObject!=null) displayObject.SetActive(false);
		}

		public virtual void Update() {
			int num=0;
			for(int i=0,imax=m_Fists.Length;i<imax;++i) {
				if(children[i]!=null&&m_Fists[i]!=null) {
					if((m_Fists[i].position-children[i].position).sqrMagnitude<=childSize*childSize) {
						++num;
						// Invoke events.
						if(!m_ChildIsReady[i]) {
							// Trigger a vibration to notify users this controller is ready.
							ControllerInput input=ControllerInputManager.instance.GetControllerInput(ControllerType.LeftController+i);
							if(input!=null) {
								input.StartVibration(0,0.25f);
							}
							/*...*/
							if(m_ChildReadyObjects[i]!=null) {
								if(string.IsNullOrEmpty(childReadyMsg)) {
									m_ChildReadyObjects[i].SetActive(true);
								}else {
									m_ChildReadyObjects[i].SendMessage(childReadyMsg,true,SendMessageOptions.DontRequireReceiver);
								}
							}
						}
						//
						m_ChildIsReady[i]=true;
					}else {
						// Invoke events.
						if(m_ChildIsReady[i]) {
							/*...*/
							if(m_ChildReadyObjects[i]!=null) {
								if(string.IsNullOrEmpty(childReadyMsg)) {
									m_ChildReadyObjects[i].SetActive(false);
								}else {
									m_ChildReadyObjects[i].SendMessage(childReadyMsg,false,SendMessageOptions.DontRequireReceiver);
								}
							}
						}
						//
						m_ChildIsReady[i]=false;
					}
				}
			}
			if(num==m_Fists.Length||buttonSkip.GetAny()) {
				// TODO:
				TrackerInput trackerInput=TrackerInput.Find(null);
				if(trackerInput!=null) {
					trackerInput.Recenter();
				}
				//
				StopListen();
				if(onStart!=null) {
					onStart();
				}
			}
		}
	}

	#endregion Nested Types

	#region Fields

	[Header("Page Setting")]
	public Mask maskHiddenOnAwake;
	public Transform pageRoot;
	public int pageIndex;
	public int numPages;
	public Vector3 pageSize;
	public Vector3 pageMoveScale=Vector3.left;

	[Header("Screen Fade")]
	public bool useFade=true;

	/// <summary>
	/// How long it takes to fade.
	/// </summary>
	public float fadeTime=2.0f;

	/// <summary>
	/// The initial screen color.
	/// </summary>
	public Color fadeColor=new Color(0.01f,0.01f,0.01f,1.0f);

	[Header("Fists To Start")]
	public FistsToStartSettings fistsToStartSettings;
	public bool playFistsToStart{
		set {
			if(value){
				fistsToStartSettings.StartListen();
			} else{
				fistsToStartSettings.StopListen();
			}
		}
	}
	public UIFade tipRecenter;
	public UIFade tipFinished;

	[Header("Events")]
	public UnityEngine.Events.UnityEvent onRecenter=
		new UnityEngine.Events.UnityEvent();
	public UnityEngine.Events.UnityEvent onFinished=
		new UnityEngine.Events.UnityEvent();

	[System.NonSerialized]protected Vector3 m_PageOrigin;
	[System.NonSerialized]protected bool m_IsPlaying=false,m_IsFinished=false;
	[System.NonSerialized]protected int m_PageIndexDest;
	[System.NonSerialized]protected float m_Duration,m_Time;

	[System.NonSerialized]protected UIFade m_UIFade;

	#endregion Fields

	#region Unity Messages

	protected virtual void Awake() {
		//
		if(pageRoot==null) {
			pageRoot=transform;
		}
		m_PageOrigin=pageRoot.localPosition;
		//
		if(maskHiddenOnAwake!=null) {
			maskHiddenOnAwake.enabled=true;
		}
		fistsToStartSettings.onStart=()=>{
			if(m_UIFade!=null) {
				m_UIFade.onBecameVisible.AddListener(OnFinished);
				m_UIFade.FadeIn();
			}else {
				OnFinished();
			}
		};
		// TODO : 
		if(tipRecenter!=null) {
			tipRecenter.onBecameInvisible.AddListener(()=>{
				if(tipFinished!=null) {
					tipFinished.gameObject.SetActive(true);
				}
			});
		}
		if(tipFinished!=null) {
			tipFinished.onBecameVisible.AddListener(()=>{
				Juggler.Main.DelayCall(new DelayedCall.Call(fistsToStartSettings.onStart),.5f);
			});
		}
	}

	protected virtual void Start() {
		// Add an UIFade forcefully for a better user experience.
		if(useFade) {
			// Override user settings.
			if(VRContext.main!=null) {
				m_UIFade=VRContext.main.fadeFx;
				m_UIFade.durationIn=m_UIFade.durationIn=fadeTime;
				m_UIFade.GetComponent<Image>().color=fadeColor;
				//
				m_UIFade.alpha=1.0f;
				m_UIFade.FadeOut();
			}
		}
		// <!-- TODO: VR Legacy Mode. -->
		TrackerInput trackerInput=TrackerInput.Find(null);
		if(trackerInput==null||trackerInput.connectionState!=DeviceConnectionState.Connected) {
			OnFinished();
		//
		}else {
			trackerInput.onRecenter+=()=> {
				onRecenter.Invoke();
				if(tipRecenter!=null) {
					tipRecenter.FadeOut();
				}else {
					fistsToStartSettings.onStart();
				}
			};
		}
	}

	protected virtual void Update() {
		if(m_IsPlaying) {
			m_Duration+=Time.deltaTime;
			//
			float t=Mathf.Clamp01(m_Duration/m_Time);
			pageRoot.localPosition=m_PageOrigin+Vector3.Scale(pageSize,pageMoveScale)*Mathf.Lerp(pageIndex,m_PageIndexDest,t);
			//
			if(t>=1.0f) {
				SetPageIndex(m_PageIndexDest);// Stop animation here.
			}
		}
		//
		if(fistsToStartSettings.enabled) {
			fistsToStartSettings.Update();
		}
	}

	#endregion Unity Messages

	#region Methods

	public virtual void PrevPage(float time) {
		if(m_IsPlaying) {
			return;
		}
		if(pageIndex-1<0) {

			return;
		}
		//
		if(time==0.0f) {
			SetPageIndex(pageIndex-1);
		}else {
			SetPageIndex(pageIndex-1,time);
		}
	}

	public virtual void NextPage(float time) {
		if(m_IsPlaying) {
			return;
		}
		if(pageIndex+1>=numPages) {
			OnFinished();
			return;
		}
		//
		if(time==0.0f) {
			SetPageIndex(pageIndex+1);
		}else {
			SetPageIndex(pageIndex+1,time);
		}
	}

	public virtual void SetPageIndex(int index,float time) {
		if(index==m_PageIndexDest) {
			return;
		}
		m_PageIndexDest=index;
		//
		m_IsPlaying=true;
		m_Duration=0.0f;
		m_Time=time;
	}

	public virtual void SetPageIndex(int index) {
		if(index==pageIndex) {
			return;
		}
		pageIndex=index;
		//
		pageRoot.localPosition=m_PageOrigin+Vector3.Scale(pageSize,pageMoveScale)*pageIndex;
		//
		m_IsPlaying=false;
	}

	protected virtual void OnFinished() {
		if(m_IsFinished) {
			return;
		}
		m_IsFinished=true;
		//
		onFinished.Invoke();
	}

	// Misc

	public virtual void ReloadLevel() {
		CleanScene();
		Application.LoadLevel(Application.loadedLevel);
	}

	public virtual void LoadLevel(int index) {
		CleanScene();
		Application.LoadLevel(index);
	}

	public virtual void LoadLevel(string name) {
		CleanScene();
		Application.LoadLevel(name);
	}

	public virtual void Quit() {
		CleanScene();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying=false;
#else
		Application.Quit();
#endif
	}

	public virtual void CleanScene() {
		// We must stop all vibrations when loading a new level.
		int i=2;
		while(i-->0) {
			ControllerInput input=ControllerInputManager.instance.GetControllerInput(ControllerType.LeftController+i);
			if(input!=null) {
				input.StopVibration();
			}
		}
	}

	#endregion Methods

}