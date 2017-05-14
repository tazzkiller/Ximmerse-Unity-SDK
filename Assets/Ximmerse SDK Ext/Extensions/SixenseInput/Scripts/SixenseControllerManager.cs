//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using Ximmerse.InputSystem;

namespace Ximmerse.ThirdParty {

	/// <summary>
	/// 
	/// </summary>
	public class SixenseController:ExternalControllerDevice{

		#region Fields

		public SixenseControllerManager manager;
		public int index;
		public InputTransform inputTransform;
		public SixensePluginLite.sixenseControllerData rawData;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		public SixenseController(SixenseControllerManager manager,string name,int index):base(name) {
			this.manager=manager;
			this.index=index;
			this.inputTransform=this.manager.inputTransforms.Length==1?this.manager.inputTransforms[0]:this.manager.inputTransforms[index];
			this.rawData=new SixensePluginLite.sixenseControllerData();
			// Write the controller's properties
			XDevicePlugin.SetBool(handle,XDevicePlugin.kField_IsAbsRotation,true);
		}

		#endregion Constructors

		#region Methods
		
		/// <summary>
		/// 
		/// </summary>
		public override int GetInputState(ref XDevicePlugin.ControllerState state) {
			int i;
			//
			int ret=SixensePluginLite.sixenseGetNewestData(index,ref rawData);
			if(ret==0) {
				// Axes
				i=0;
				state.axes[i++]=rawData.trigger;
				i++;
				state.axes[i++]=rawData.joystick_x;
				state.axes[i++]=rawData.joystick_y;
				// Buttons
				//
				if(inputTransform==null) {
					state.buttons=rawData.buttons;
				}else {
					state.buttons=inputTransform.TransformButtons(rawData.buttons);
				}
				// Position
				i=3;
				while(i-->0) {// i==3
					state.position[i]=rawData.pos[i]*manager.sensitivity[i];
				}
				// Rotation
				i=4;
				while(i-->0) {
					state.rotation[i]=rawData.rot_quat[i];
				}
			}
			return ret;
		}

		#endregion Methods

	}

	/// <summary>
	/// 
	/// </summary>
	public class SixenseControllerManager:ControllerDeviceManager{

		#region Fields
		
		public string controllerFormat="SixenseController-{0}";
		public Vector3 sensitivity=Vector3.one*.01f;
		public Vector3[] resetPoints=new Vector3[2]{new Vector3(),new Vector3()};
		public ButtonEvent buttonReset=new ButtonEvent(
			new ButtonEvent.PersistentListener{type=ButtonEvent.Type.ControllerInput,stringArg="LeftController",intArg=(int)ControllerButton.Start},
			new ButtonEvent.PersistentListener{type=ButtonEvent.Type.ControllerInput,stringArg="RightController",intArg=(int)ControllerButton.Start}
		);

		#endregion Fields
		
		#region Methods

		/// <summary>
		/// 
		/// </summary>
		public override bool InitAllControllers() {
			//
			if(m_IsInited) {
				return false;
			}
			m_IsInited=true;
			//
			int ret;
			ret=XDevicePlugin.Init();
			if(ret!=0) return false;
			ret=SixensePluginLite.sixenseInit();
			if(ret!=0) return false;
			//
			if(maxControllers<=0) {
				maxControllers=SixensePluginLite.sixenseGetMaxControllers();
			}
			if(controllers==null) {
				if(inputTransforms.Length==0) {
					inputTransforms=new InputTransform[1]{null};
				}
				controllers=new SixenseController[maxControllers];
				for(int i=0;i<maxControllers;++i) {
					controllers[i]=new SixenseController(
						this,
						string.Format(controllerFormat,i),i
					);
				}
			}
			//
			m_Result=0;
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool DeinitAllControllers() {
			if(!m_IsInited) {
				return false;
			}
			m_IsInited=false;
			//
			int ret;
			ret=SixensePluginLite.sixenseExit();
			if(ret!=0) return false;
			ret=XDevicePlugin.Exit();
			if(ret!=0) return false;
			//
			m_Result=0;
			return true;
		}
		
		#endregion Methods

	}
}