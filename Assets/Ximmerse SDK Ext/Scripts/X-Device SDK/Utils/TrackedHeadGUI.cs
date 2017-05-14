using UnityEngine;
using Ximmerse.VR;
using Ximmerse.InputSystem;
using Ximmerse.UI;

public class TrackedHeadGUI:MonoBehaviour {

	#region Fields

	public GameObject uiRoot;
	public UnityEngine.UI.Text textFixMark;
	public UIVector3Field uiHmdRotation;
	public string[] tipsFixMark=new string[5]{
		"Please keep your neck's position,and look to your forward...",
		"Please keep your neck's position,and look to your leftward...",
		"Please keep your neck's position,and look to your rightward...",
		"Please keep your neck's position,and look to your downward...",
		"Done!"
	};

	public ButtonEvent[] buttonStartFixMark=new ButtonEvent[2] {
		new ButtonEvent(
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
		),
		new ButtonEvent(
			new ButtonEvent.PersistentListener{
				type=ButtonEvent.Type.ControllerInput,
				stringArg="LeftController",
				intArg=(int)(DaydreamButton.GripL)
			},
			new ButtonEvent.PersistentListener{
				type=ButtonEvent.Type.ControllerInput,
				stringArg="LeftController",
				intArg=(int)(DaydreamButton.GripR)
			},
			new ButtonEvent.PersistentListener{
				type=ButtonEvent.Type.ControllerInput,
				stringArg="RightController",
				intArg=(int)(DaydreamButton.GripL)
			},
			new ButtonEvent.PersistentListener{
				type=ButtonEvent.Type.ControllerInput,
				stringArg="RightController",
				intArg=(int)(DaydreamButton.GripR)
			}
		)
	};

	public ButtonEvent buttonCancelFixMark=new ButtonEvent(
		new ButtonEvent.PersistentListener{
			type=ButtonEvent.Type.ControllerInput,
			stringArg="LeftController",
			intArg=(int)(DaydreamButton.Home)
		},
		new ButtonEvent.PersistentListener{
			type=ButtonEvent.Type.ControllerInput,
			stringArg="RightController",
			intArg=(int)(DaydreamButton.Home)
		},
		new ButtonEvent.PersistentListener{
			type=ButtonEvent.Type.Input_Key,
			intArg=(int)KeyCode.Escape,
		}
	);

	public ButtonEvent buttonNextFixMark=new ButtonEvent(
		new ButtonEvent.PersistentListener{
			type=ButtonEvent.Type.ControllerInput,
			stringArg="LeftController",
			intArg=(int)(DaydreamButton.Click)
		},
		new ButtonEvent.PersistentListener{
			type=ButtonEvent.Type.ControllerInput,
			stringArg="RightController",
			intArg=(int)(DaydreamButton.Click)
		},
		new ButtonEvent.PersistentListener{
			type=ButtonEvent.Type.Input_Button,
			stringArg="Fire1",
		}
	);

	[System.NonSerialized]protected TrackedHead m_TrackedHead;
	[System.NonSerialized]protected Vector3[] m_CalPoints_DK4=new Vector3[4];
	[System.NonSerialized]protected int m_CalPointIndex_DK4=-1;

	#endregion Fields

	#region Unity Messages

	protected virtual void OnCreate(Object createBy) {
		m_TrackedHead=(TrackedHead)createBy;
		if(uiRoot==null) {
			uiRoot=transform.GetChild(0).gameObject;
		}
		if(textFixMark==null) {
			textFixMark=uiRoot.GetComponentInChildren<UnityEngine.UI.Text>();
		}
		//
		if(uiRoot!=null) {
			uiRoot.SetActive(false);
		}
	}

	protected virtual void Update() {
		if(m_TrackedHead!=null) {
			TryFixMarkOffset();
		}
		if(uiRoot.activeSelf) {
			//
			if(uiHmdRotation!=null) {
				uiHmdRotation.value=VRContext.GetAnchor(VRNode.CenterEye).localRotation.eulerAngles;
			}
			//
		}
	}

	#endregion Unity Messages

	#region Methods

	public virtual void TryFixMarkOffset() {
		switch(m_TrackedHead.trackingType) {
			case TrackedHead.TrackingType.Outside_In:
				// TODO : 
				//
				if(m_CalPointIndex_DK4<0) {
					bool isAllDown=buttonStartFixMark.Length!=0;
					for(int i=0,imax=buttonStartFixMark.Length;i<imax;++i) {
						if(!buttonStartFixMark[i].GetAny()) {
							isAllDown=false;
							break;
						}
					}
					if(isAllDown) {
						m_CalPointIndex_DK4=0;
						//
						if(uiRoot!=null) {
							uiRoot.SetActive(true);
						}
						if(textFixMark!=null) {
							textFixMark.text=tipsFixMark[m_CalPointIndex_DK4];
						}
					}
				}else if(m_CalPointIndex_DK4<4){
					if(buttonNextFixMark.GetAnyDown()) {
						m_CalPoints_DK4[m_CalPointIndex_DK4++]=m_TrackedHead.controllerInput.GetPosition();
						//
						if(textFixMark!=null) {
							textFixMark.text=tipsFixMark[m_CalPointIndex_DK4];
						}
					}
					if(buttonCancelFixMark.GetAnyDown()) {
						m_CalPointIndex_DK4=5;
					}
				}else if(m_CalPointIndex_DK4==4){
					if(buttonNextFixMark.GetAnyDown()) {
						m_CalPointIndex_DK4=5;
					}
				}
				// Quit GUI
				if(m_CalPointIndex_DK4==5) {
					FixMarkOffset_DK4(m_CalPoints_DK4);
					m_CalPointIndex_DK4=-1;
					//
					if(textFixMark!=null) {
						textFixMark.text="";
					}
					if(uiRoot!=null) {
						uiRoot.SetActive(false);
					}
				}
			break;
		}
	}

	public virtual void FixMarkOffset_DK4(Vector3[] points) {
		int i;
		Vector3 offset=Vector3.zero;
		Vector3 v=Vector3.zero;
		float r;
		//
		i=0;
		if(!Ximmerse.MathUtil.CalculateSphere(
			points[i++],
			points[i++],
			points[i++],
			points[i++],
			out v,
			out r
		)){
			return;
		}
		//
		Vector3[] points2=new Vector3[3];
		Quaternion q=Quaternion.identity;// TODO
		for(i=0;i<3;++i) {
			//
			points2[i]=q*points[i];
			//
			points2[i].y=points2[i].z;
			points2[i].z=0.0f;
		}
		i=0;
		Vector2 vXZ;
		if(!Ximmerse.MathUtil.CalculateCircle(
			points2[i++],
			points2[i++],
			points2[i++],
			out vXZ,
			out offset.z
		)){
			return;
		}
		offset.y=Mathf.Sqrt(r*r-offset.z*offset.z);
		print("Center:"+v.ToString("0.000000")+"\nRadius:"+r+"\nOffset Y:"+offset.y+"\nOffset Z:"+offset.z);
		//
		m_TrackedHead.markTransform.localPosition=offset;
		PlayerPrefs.SetFloat(m_TrackedHead.prefsKey+".markOffset.x",offset.x);
		PlayerPrefs.SetFloat(m_TrackedHead.prefsKey+".markOffset.y",offset.y);
		PlayerPrefs.SetFloat(m_TrackedHead.prefsKey+".markOffset.z",offset.z);
	}

	#endregion Methods

}