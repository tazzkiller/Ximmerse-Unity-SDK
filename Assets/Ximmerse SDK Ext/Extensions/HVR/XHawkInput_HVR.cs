using System.Runtime.InteropServices;
using UnityEngine;
using Ximmerse.VR;
using Ximmerse.InputSystem;

public class XHawkInput_HVR:XHawkInput {

	#region Consts

	public const int 
		 NODE_LEFT_HAND   = 0
		,NODE_RIGHT_HAND  = 1
		,NODE_HEAD        = 2
		,NODE_LEFT_DOCK   = 0
		,NODE_RIGHT_DOCK  = 2
	;

	#endregion Consts

	#region Nested Types
	
	[StructLayout(LayoutKind.Sequential)]
	public struct BoundaryTestResult{
		// Returns true if the queried test would violate and/or trigger the tested boundary types.
		public bool isTriggering;
		//
		public int cornerId0;
		public int cornerId1;
		// Returns the distance between the queried test object and the closest tested boundary type.
		public float distance;
		// Returns the closest point to the queried test object.
		public Vector3 point;
		// Returns the normal of the closest point to the queried test object.
		public Vector3 normal;
	};

	internal static class NativeMethods {
		const string pluginName=XDevicePlugin.LIB_XDEVICE;

		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern System.IntPtr Boundary_Alloc(int handedness,int numCorners,float minHeight,float maxHeight);
		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern void Boundary_Free(System.IntPtr boundary);

		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern int Boundary_GetHandedness(System.IntPtr boundary);
		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern void Boundary_SetHandedness(System.IntPtr boundary,int handedness);

		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern int Boundary_GetNumCorners(System.IntPtr boundary);
		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern void Boundary_SetNumCorners(System.IntPtr boundary,int num);

		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern int Boundary_GetCorner(System.IntPtr boundary,int cornerId,ref float x,ref float z);
		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern int Boundary_SetCorner(System.IntPtr boundary,int cornerId,float x,float z);

		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern int Boundary_GetHeight(System.IntPtr boundary,ref float minHeight,ref float maxHeight);
		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern int Boundary_SetHeight(System.IntPtr boundary,float minHeight,float maxHeight);

		[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool Boundary_HitTest(System.IntPtr boundary,float x,float y,float z,out BoundaryTestResult result);
	}

	#endregion Nested Types

	#region Fields
	
	public float playAreaOffsetY=0.1f;
	public float warningDistance=0.5f;

	public Transform dockTransform;
	public Vector3 dockPosition;
	public float docksMaxDistance=2.0f;
	/// <summary>
	/// Dock control points
	/// </summary>
	[Tooltip("[0]:Left point\n[1]:Right point\n[2]:Dock anchor")]
	public GameObject[] docks=new GameObject[3];
	
	/// <summary>
	/// The final forward vector (on tracking space) for tracking system.
	/// </summary>
	protected Vector3 m_WorldForward=Vector3.forward;
	
	/// <summary>
	/// Vector (on tracking space) from left dock point to right one.
	/// </summary>
	protected Vector3 m_DockVector=Vector3.right;

	/// <summary>
	/// Tracking results for dock control points.
	/// </summary>
	protected bool[] m_DocksExisted=new bool[3];

	/// <summary>
	/// Position array on tracker space.
	/// </summary>
	protected Vector3[] m_DocksPosition=new Vector3[3];

	/// <summary>
	/// 
	/// </summary>
	protected Vector3 m_TrackerCachedPosition;

	/// <summary>
	/// 
	/// </summary>
	protected Transform m_PlayArea,m_PlayAreaGround,m_PlayAreaWall;
	protected PlayAreaRenderer m_PlayAreaRenderer;
	protected System.IntPtr m_BoundaryPtr=System.IntPtr.Zero;

	#endregion Fields

	#region Overrides

	protected override void OnDestroy() {
		if(m_BoundaryPtr!=System.IntPtr.Zero) {
			NativeMethods.Boundary_Free(m_BoundaryPtr);
			m_BoundaryPtr=System.IntPtr.Zero;
		}
		base.OnDestroy();
	}

	public override void Launch(bool checkOthers) {
		//
		XDevicePlugin.Init();
		XDevicePlugin.SetInt(-1,XDevicePlugin.kField_CtxDeviceVersion,0x3000);
		//
		base.Launch(checkOthers);
		// Create control points in runtime.
		Transform trackingSpace=VRContext.GetAnchor(VRNode.TrackingSpace);
		for(int i=0;i<3;++i) {
			if(docks[i]!=null) {
				GameObject go=Instantiate(docks[i]);
				Transform t=docks[i].transform;
				Transform newT=go.transform;
				//
				go.name=docks[i].name;

				newT.SetParent(trackingSpace);
				newT.localPosition=t.localPosition;
				newT.localRotation=t.localRotation;
				newT.localScale=t.localScale;
				//
				docks[i]=go;
			}
		}
		// Initialize play area.
		m_PlayAreaRenderer=docks[2].GetComponentInChildren<PlayAreaRenderer>();
		if(m_PlayAreaRenderer!=null) {
			m_PlayArea=m_PlayAreaRenderer.transform;
			m_BoundaryPtr=NativeMethods.Boundary_Alloc(-1,4,-2.0f,2.0f);
			for(int i=0,imax=m_PlayAreaRenderer.corners.Length;i<imax;++i) {
				NativeMethods.Boundary_SetCorner(m_BoundaryPtr,i,
					m_PlayAreaRenderer.corners[i].x,-m_PlayAreaRenderer.corners[i].z
				);
			}
		}
		//
		Transform head=VRContext.GetAnchor(VRNode.Head);
		if(head!=null) {
			TrackedHead trackedHead=head.GetComponent<TrackedHead>();
			if(trackedHead!=null) {
				trackedHead.markTransform.localPosition=
					head.InverseTransformPoint(anchor.position);
			}
		}
	}

	//public override bool EnsureAnchor() {
	//	return base.EnsureAnchor();
	//}

	protected override void UpdateState() {
		if(Time.frameCount!=m_PrevFrameCount){
			base.UpdateState();
			m_PrevFrameCount=Time.frameCount;
			//
			UpdateDocks();
		}
	}

	public override bool Exists(int node) {
		switch(node) {
			case NODE_LEFT_HAND:
			return false;
			case NODE_RIGHT_HAND:
			return base.Exists(node);
			case NODE_HEAD:
			return m_DocksExisted[2];
			default:
			return base.Exists(node);
		}
	}

	public override void Recenter() {
		//base.Recenter();
	}

	public override Vector3 GetPosition(int node) {
		switch(node) {
			case NODE_LEFT_HAND:
			case NODE_RIGHT_HAND:
			return base.GetPosition(node);
			case NODE_HEAD:
			return GetTrackerPosition();
		}
		return base.GetPosition(node);
	}

	#endregion Overrides

	#region Methods

	/// <summary>
	/// 
	/// </summary>
	public virtual void UpdateDocks() {
		int i=0;
		//
		for(;i<2;++i) {
			m_DocksExisted[i]=(XDevicePlugin.GetNodePosition(m_Handle,0,
				(i==0)?NODE_LEFT_DOCK:NODE_RIGHT_DOCK,
				ref m_DocksPosition[i]
			)&TrackingResult.PositionTracked)!=0;
		}
		if(m_DocksExisted[0]&&m_DocksExisted[1]) {
			m_DocksExisted[i]=true;
			m_DocksPosition[i]=Vector3.Lerp(m_DocksPosition[0],m_DocksPosition[1],.5f);
			//
			Vector3 vector=m_DockVector;
			m_DockVector=TransformPoint(m_DocksPosition[1])-TransformPoint(m_DocksPosition[0]);
			  // Revert
			if(m_DockVector.sqrMagnitude>docksMaxDistance*docksMaxDistance) {
				print("m_DockVector.magnitude("+m_DockVector.magnitude+")>docksMaxDistance in UpdateDocks");
				m_DockVector=vector;
			}
			// TODO : 
			m_WorldForward=m_DockVector;
			m_WorldForward.y=0.0f;
			m_WorldForward=Quaternion.Euler(new Vector3(0,-90.0f,0))*m_WorldForward;
			m_WorldForward.Normalize();
		} else if(m_DocksExisted[0]) {
			m_DocksExisted[i]=true;
			m_DocksPosition[i]=m_AnchorMatrix.inverse.MultiplyPoint3x4(
				TransformPoint(m_DocksPosition[0])+0.5f*m_DockVector);
		} else if(m_DocksExisted[1]) {
			m_DocksExisted[i]=true;
			m_DocksPosition[i]=m_AnchorMatrix.inverse.MultiplyPoint3x4(
				TransformPoint(m_DocksPosition[1])-0.5f*m_DockVector);
		} else {
			m_DocksExisted[i]=false;
			m_DocksPosition[i]=Vector3.zero;
		}
		//
		i=0;
		Quaternion rotation=Quaternion.LookRotation(m_WorldForward,Vector3.up);
		for(;i<3;++i) {
			if(docks[i]!=null) {
				docks[i].SetActive(m_DocksExisted[i]);
				Transform t=docks[i].transform;
				t.localPosition=TransformPoint(m_DocksPosition[i]);
				t.localRotation=rotation;
			}
		}
		// Update play area.
		if(m_PlayArea!=null) {
			docks[2].SetActive(true);
			docks[2].transform.localRotation=Quaternion.identity;
			UpdatePlayArea();
		}
	}

	public virtual Vector3 GetTrackerPosition() {
		if(!m_DocksExisted[2]) {
			return m_TrackerCachedPosition;
		}
		//
		Matrix4x4 anchorMatrix=anchor.localToWorldMatrix;
		if(trackingSpace!=null) {
			anchorMatrix=trackingSpace.worldToLocalMatrix*anchorMatrix;
		}
		Quaternion trackerRotation=Quaternion.LookRotation(
			anchorMatrix.MultiplyVector(Vector3.forward),
			anchorMatrix.MultiplyVector(Vector3.up)
		);
		return m_TrackerCachedPosition=dockPosition-trackerRotation*m_DocksPosition[2];
	}

	protected static int[] s_TrackedNodes=new int[] {
		NODE_RIGHT_HAND,
		NODE_HEAD
	};

	public virtual void UpdatePlayArea() {
		// TODO : 
		if(m_PlayAreaGround==null) {
			m_PlayAreaGround=m_PlayArea.FindChild("_Ground");
		}
		if(m_PlayAreaWall==null) {
			m_PlayAreaWall=m_PlayArea.FindChild("_Walls");
		}
		//
		Vector3 position=Vector3.zero;
		if(m_PlayAreaGround!=null) {
			position=m_PlayAreaGround.position;
			position.y=0.0f;
			m_PlayAreaGround.position=position;
		}
		if(m_PlayAreaWall!=null) {
			position=m_PlayAreaWall.localPosition;
			position.y=playAreaOffsetY-m_PlayAreaRenderer.height;
			m_PlayAreaWall.localPosition=position;
		}
		//
		BoundaryTestResult hitInfo=new BoundaryTestResult();
		float minDistance=float.MaxValue;
		for(int i=0,imax=s_TrackedNodes.Length;i<imax;++i) {
			if(Exists(s_TrackedNodes[i])) {
				//
				position=GetPosition(s_TrackedNodes[i]);
				position=m_PlayArea.InverseTransformPoint(trackingSpace.TransformPoint(position));
				//
				if(NativeMethods.Boundary_HitTest(m_BoundaryPtr,
					 position[0],
					 position[1],
					-position[2],
					out hitInfo
				)) {
					// TODO : 
					//hitInfo.distance=-1.0f;
				}else {
					if(hitInfo.distance>0.0f) {
						hitInfo.distance*=-1;
					}
				}
				if(hitInfo.distance<minDistance) {
					minDistance=hitInfo.distance;
				}
			}
		}
		if(m_PlayAreaRenderer!=null) {
			float alpha=1.0f-Mathf.Clamp01(minDistance/warningDistance);
			//
			m_PlayAreaRenderer.groundAlpha=1.0f;
			m_PlayAreaRenderer.wallAlpha=alpha*1.0f;
		}
	}

	#endregion Methods

}
