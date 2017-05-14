//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

namespace Ximmerse.InputSystem {
	
	/// <summary>
	/// An implement of the ExternalControllerDevice management.
	/// </summary>
	public class ControllerDeviceManager:MonoBehaviour{

		#region Fields

		public bool updateBySelf=false;//true;
		public int maxControllers=1;
		public ExternalControllerDevice[] controllers;
		public InputTransform[] inputTransforms=new InputTransform[1];

		[System.NonSerialized]protected bool m_IsInited=false;
		[System.NonSerialized]protected int m_Result=-1;

		#endregion Fields

		#region Unity Messages
		
		/// <summary>
		/// 
		/// </summary>
		protected virtual void Awake() {
			if(updateBySelf) {
				InitAllControllers();
			}
		} 
		
		/// <summary>
		/// 
		/// </summary>
		protected virtual void OnDestroy() {
			if(updateBySelf) {
				DeinitAllControllers();
			}
		}

		#endregion Unity Messages

		#region Methods
		
		/// <summary>
		/// 
		/// </summary>
		public virtual bool InitAllControllers() {
			return XDevicePlugin.Init()==0;
		}
		
		/// <summary>
		/// TODO : UpdateState() will called in ControllerInput class.
		/// </summary>
		public virtual bool UpdateAllControllers() {
			return true;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public virtual bool DeinitAllControllers() {
			return XDevicePlugin.Exit()==0;
		}

		#endregion Methods

	}

	public partial class ControllerInputManager {

		#region Fields

		public ControllerDeviceManager[] controllerDeviceManagers=new ControllerDeviceManager[0];

		#endregion Fields

		#region Unity Messages

		protected virtual void OnStart() {
			onUpdate+=UpdateControllerDeviceManagers;
			onDestroy+=DeinitControllerDeviceManagers;
			//
			InitControllerDeviceManagers();
		}

		#endregion Unity Messages

		#region Methods

		protected virtual void InitControllerDeviceManagers() {
			for(int i=0,imax=controllerDeviceManagers.Length;i<imax;++i) {
				if(controllerDeviceManagers[i]!=null) {
					controllerDeviceManagers[i].enabled=false;
					if(controllerDeviceManagers[i].InitAllControllers()) {
					}else {
						controllerDeviceManagers[i]=null;
					}
				}
			}
		}

		protected virtual void UpdateControllerDeviceManagers() {
			for(int i=0,imax=controllerDeviceManagers.Length;i<imax;++i) {
				if(controllerDeviceManagers[i]!=null) {
					controllerDeviceManagers[i].UpdateAllControllers();
				}
			}
		}

		protected virtual void DeinitControllerDeviceManagers() {
			for(int i=0,imax=controllerDeviceManagers.Length;i<imax;++i) {
				if(controllerDeviceManagers[i]!=null) {
					controllerDeviceManagers[i].DeinitAllControllers();
				}
			}
		}
		
		#endregion Methods

	}
}
