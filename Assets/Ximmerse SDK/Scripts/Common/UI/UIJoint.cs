using System.Collections.Generic;
using UnityEngine;
using Ximmerse.VR;

namespace Ximmerse.UI{

	public class UIJoint:MonoBehaviour {

		#region Fields

		public static Dictionary<Transform,Transform> s_ParentMap=new Dictionary<Transform,Transform>();

		public static Transform GetUIParent(Transform parent) {
			Transform ret;
			if(s_ParentMap.TryGetValue(parent,out ret)) {
			}else {
				ret=parent.Find("UIRoot-VR") as Transform;
				if(ret==null) {
					ret=new GameObject("UIRoot-VR",typeof(Canvas)).transform;
					//
					ret.SetParent(parent);
					ret.localPosition=Vector3.zero;
					ret.localRotation=Quaternion.identity;
					ret.localScale=Vector3.one;
					//
					Canvas canvas=ret.GetComponent<Canvas>();
					canvas.renderMode=RenderMode.WorldSpace;
				}
			}
			return ret;
		}

		public VRNode parentNode=VRNode.CenterEye;
		public Transform parent;
		[System.NonSerialized]protected Transform m_Transform;

		[Header("Transform")]
		public Vector3 position=Vector3.zero;
		[SerializeField]protected Vector3 m_Rotation=Vector3.zero;
		[System.NonSerialized]public Quaternion rotation;
		public Vector3 scale=Vector3.one;

		[System.NonSerialized]protected GameObject m_SelfGo,m_ParentGo;

		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			//
			m_Transform=transform;
			rotation=Quaternion.Euler(m_Rotation);
			//
			if(true) {
			} else {
				Transform c=m_Transform.FindChild("_VisibleObject");
				if(c!=null) {
					m_SelfGo =c.gameObject;
				}
			}
		}

		protected virtual void Start() {
			//
			if(parent==null) {
				parent=VRContext.GetAnchor(parentNode);
			}
			if(parent!=null) {
				if(true){
					//
					Transform c=parent.FindChild("_VisibleObject");
					if(c!=null) {
						parent=c;
					}
					parent=GetUIParent(parent);
					//
					m_Transform.SetParent(parent);
					m_Transform.localPosition=position;
					m_Transform.localRotation=rotation;
					m_Transform.localScale=scale;
				}else{
					Transform c=parent.FindChild("_VisibleObject");
					if(c!=null) {
						m_ParentGo=c.gameObject;
					}else {
						m_ParentGo=parent.gameObject;
					}
				}
			}
		}

		protected virtual void Update() {
			UpdateTransform();
		}

		#endregion Unity Messages

		#region Methods

		public virtual void UpdateTransform() {
			if(true) {
			}else {
				if(parent!=null) {
					// GameObject.active
					if(m_SelfGo!=null) {
						m_SelfGo.SetActive(m_ParentGo.activeInHierarchy);
					}
					//
					m_Transform.position=parent.TransformPoint(position);
					m_Transform.rotation=parent.rotation*rotation;
					m_Transform.localScale=scale;
				}
			}
		}

		#endregion Methods

	}
}
