//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Runtime.InteropServices;
using Ximmerse.VR;
#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;
#endif

namespace Ximmerse.InputSystem {

	/// <summary>
	/// C# wrapper for X-Device SDK.
	/// </summary>
	public partial class XDevicePlugin {

		#region Const
     
		/// <summary>
        /// Dll name
        /// </summary>
		public const string LIB_XDEVICE=
#if UNITY_IPHONE //|| UNITY_XBOX360
			// On iOS and Xbox 360 plugins are statically linked into the executable,
			// so we have to use __Internal as the library name.
			"__Internal"
#else
			"xdevice"
#endif
		;

		#endregion Const

		#region Nested Types
		
		public delegate void VoidDelegate(int which);
		public delegate void AxesDelegate(int which, int axis,float value);
		public delegate void KeyDelegate(int which, int code, int action);
		public delegate void Vector3Delegate(int which, float x,float y,float z);
		public delegate void Vector4Delegate(int which, float x,float y,float z,float w);
		public delegate int ControllerStateDelegate(int which, ref ControllerState state);
		public delegate int SendMessageDelegate(int which, int Msg, int wParam, int lParam);

        /// <summary>
        /// A struct contains Cobra hand controller info, including button mask, axes value, positions, rotations, etc
        /// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct ControllerState {
            
			// Common
			public int handle;
			public int timestamp;
			public int state_mask;
            
			// Gamepad
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]
			public float[] axes;
			public uint buttons;
            
			// Motion
            [MarshalAs(UnmanagedType.ByValArray,SizeConst=3)]
			public float[] position;
			[MarshalAs(UnmanagedType.ByValArray,SizeConst=4)]
			public float[] rotation;
			[MarshalAs(UnmanagedType.ByValArray,SizeConst=3)]
			public float[] accelerometer;
			[MarshalAs(UnmanagedType.ByValArray,SizeConst=3)]
			public float[] gyroscope;

			public static ControllerState Obtain(){
				return new ControllerState(-1);
			}

			public ControllerState(int myHandle){
            
				// Common
				handle=myHandle;
				timestamp=0;
				state_mask=0;
            
				// Gamepad
				axes=new float[6] ;
				buttons=0u;
            
				// Motion
				position=new float[3] ;
				rotation=new float[4] ;
				accelerometer=new float[3] ;
				gyroscope=new float[3] ;
			}
		}

        /// <summary>
        /// A struct that contains raw camera tracker data. 
        /// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct TrackerState {

			public const int POINT_DATA_SIZE=3;

			public int handle;
			public int timestamp;
			public int frameCount;
			public int capacity;
			public int count;
			public System.IntPtr id;
			public System.IntPtr data;

		}

		#endregion Nested Types

		#region Natives

		internal static class NativeMethods {
			private const string pluginName=LIB_XDEVICE;
			
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceInit();
#if UNITY_EDITOR_OSX||UNITY_STANDALONE_OSX
			public static int XDeviceExit(){return 0;}
#else
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceExit();
#endif
		
#elif UNITY_ANDROID

			public static AndroidJavaClass s_XDeviceApi;
		
			public static int XDeviceInit(){
				if(s_XDeviceApi==null) {
					s_XDeviceApi=new AndroidJavaClass("com.ximmerse.sdk.XDeviceApi");
				}
#if !XDEVICE_DO_NOT_USE_SERVICES
				int ret=s_XDeviceApi.CallStatic<int>("initInUnity");
#else
				int ret=-1;
				using(AndroidJavaClass jc=new AndroidJavaClass("com.unity3d.player.UnityPlayer")){
				using(AndroidJavaObject currentActivity=jc.GetStatic<AndroidJavaObject>("currentActivity")){
					ret=s_XDeviceApi.CallStatic<int>("init",currentActivity,0x4000,1);
					SetBool(-1,kField_CtxCanProcessInputEvent,false);
				}}
#endif
				return ret;
			}

			public static int XDeviceExit(){
				int ret=0;
				if(s_XDeviceApi!=null) {
					ret=s_XDeviceApi.CallStatic<int>("exit");
					s_XDeviceApi.Dispose();
					s_XDeviceApi=null;
				}
				return ret;
			}

#endif

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern System.IntPtr XDeviceGetContext(bool autoCreate);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern System.IntPtr XDeviceGetInputDevice(int which);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetInputDeviceHandle(System.IntPtr name);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern System.IntPtr XDeviceGetInputDeviceName(int which);

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetInputDeviceCount();
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetInputDevices(int type,int[] whichBuffer,int whichBufferSize);

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern System.IntPtr XDeviceRemoveInputDeviceAt(int which,bool dispose);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceAddExternalControllerDevice(System.IntPtr name,ControllerStateDelegate converter,SendMessageDelegate sender);

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetInputState(int which,System.IntPtr state);
#if (UNITY_EDITOR_OSX||UNITY_STANDALONE_OSX||UNITY_IOS)
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int ControllerStateFromPtr(ref ControllerState state,System.IntPtr ptr);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int TrackerStateFromPtr(ref TrackerState state,System.IntPtr ptr);
#else
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetInputState(int which,ref ControllerState state);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetInputState(int which,ref TrackerState state);
#endif
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceSendMessage(int which,int Msg,int wParam,int lParam);

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceUpdateInputState(int which);
	
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern bool XDeviceGetBool(int which,int fieldID,bool defaultValue);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern void XDeviceSetBool(int which,int fieldID,bool value);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetInt(int which,int fieldID,int defaultValue);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern void XDeviceSetInt(int which,int fieldID,int value);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern float XDeviceGetFloat(int which,int fieldID,float defaultValue);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern void XDeviceSetFloat(int which,int fieldID,float value);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern void XDeviceGetObject(int which,int fieldID,System.IntPtr buffer,int offset,int count);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern void XDeviceSetObject(int which,int fieldID,System.IntPtr buffer,int offset,int count);

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetNodePosition(int which,int history,int node,float[] position);

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceUpdateNodePose(int which,int node,float[] position,float[] rotation);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetTrackerPose(int which,out float height,out float depth,out float pitch);
			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceSetTrackerPose(int which,float height,float depth,float pitch);

			[DllImport(pluginName,CallingConvention=CallingConvention.Cdecl)]
			public static extern int XDeviceGetTickCount();

		}
		
		#endregion Natives

		#region Methods

		protected static bool s_IsInited=false;
			
		/// <summary>
		/// Initialize the X-Device SDK library.
		/// </summary>
		public static int Init(){
			// Initialization Lock.
			if(s_IsInited) return 0;
			s_IsInited=true;
			//
			NativeMethods.XDeviceExit();
			//
			NativeMethods.XDeviceGetContext(true);
#if UNITY_EDITOR_WIN||UNITY_STANDALONE_WIN
			//
			string pluginPath=UnityEngine.Application.dataPath+"\\Plugins\\"
#if UNITY_EDITOR_WIN
				+(GetInt(-1,/*FieldID::*/kField_CtxPlatformVersion,0)%2==0?"x86":"x86_64");
#elif UNITY_STANDALONE_WIN
				;
#endif
			string path=System.Environment.GetEnvironmentVariable("PATH");
			if(path.IndexOf(pluginPath)==-1) {
				System.Environment.SetEnvironmentVariable("PATH",path+";"+pluginPath);
			}
#endif
			NativeMethods.XDeviceSetInt(-1,kField_CtxDeviceVersion,PlayerPrefsEx.GetInt("XimmerseDevice.type",0x4000));
			int ret=NativeMethods.XDeviceInit();
			// Add Unity objects into X-Device plugin.
			AddExternalControllerDevice("VRDevice",VRContext.s_OnHmdUpdate,VRContext.s_OnHmdMessage);
			return ret;
		}
			
		/// <summary>
		/// Finalize the X-Device SDK library.
		/// </summary>
		public static int Exit(){
			// Initialization Lock.
			if(!s_IsInited) return 0;
			s_IsInited=false;
			//
			int ret=NativeMethods.XDeviceExit();
			return ret;
		}
		
		/// <summary>
        /// Get the handle of input device identified by name.
        /// </summary>
        /// <param name="name">Available parameters: 
        ///     XCobra-0 : represents left controller;
        ///     XCobra-1 : represents right controller;
        ///     XHawk-0 : represents Hawk tracking camera;
        /// </param>
        /// <returns>A handle to the device, which is an int.</returns>
        /// <example> 
        /// This example shows how to use the <see cref="GetInputDeviceHandle"/> method.
        /// <code>
        /// using UnityEngine;
        /// using Ximmerse;
        /// using Ximmerse.InputSystem;
        /// 
        /// class TestClass : MonoBehaviour
        /// {
        ///     private int m_leftControllerHandle;
        ///     private int m_hawkHandle;
        ///     
        ///     private XDevicePlugin.ControllerState m_leftControllerState;
        ///     private XDevicePlugin.TrackingStateEx m_hawkState;
        ///     
        ///     void Awake() 
        ///     {
        ///         XDevicePlugin.Init();
        ///         m_leftControllerHandle = XDevicePlugin.GetInputDeviceHandle("XCobra-0");
        ///         m_hawkHandle = XDevicePlugin.GetInputDeviceHandle("XHawk-0");
        ///     }
        ///     void Update()
        ///     {
        ///         // if this is larger than 0, it means it is valid input device;
        ///         if (m_leftControllerHandle >= 0)
        ///         {
        ///             UpdateLeftController();
        ///         }
        ///         if(m_hawkHandle>=0)
        ///         {
        ///             UpdateHawk();
        ///         }
        ///     }
        ///     
        ///     private void UpdateLeftController()
        ///     {
        ///         // You have to update the state manually. 
        ///         XDevicePlugin.UpdateInputState(m_leftControllerHandle);
        ///         XDevicePlugin.GetInputState(m_leftControllerHandle, ref m_leftControllerState);
        ///         var trigger = m_leftControllerState.axes[(int)ControllerRawAxis.LeftTrigger];
        ///         var xAxis = m_leftControllerState.axes[(int)ControllerRawAxis.LeftThumbX];
        ///         var yAxis = m_leftControllerState.axes[(int)ControllerRawAxis.LeftThumbY];
        ///         var orientation = new Quaternion(
        ///                 -m_leftControllerState.rotation[0],
        ///                 -m_leftControllerState.rotation[1],
        ///                  m_leftControllerState.rotation[2],
        ///                  m_leftControllerState.rotation[3]
        ///             );
        ///     }
        ///
        ///     private void UpdateHawk()
        ///     {
        ///         // You have to update the state manually. 
        ///         XDevicePlugin.UpdateInputState(m_hawkHandle);
        ///         XDevicePlugin.GetInputState(m_hawkHandle, ref m_hawkState);
        ///         // 0 = left controller, 1 = right controller 
        ///         int floatOffset = m_hawkState.OffsetOf(0);
        ///         // if this is -1, the position can not be found
        ///         // return to 0 if point is valid
        ///         if (floatOffset >= 0)
        ///         {
        ///             float[] rawPostionData = m_hawkState.GetData();
        ///             // convert the position to 1:1 scale movement. 
        ///             float movementScale = 0.001f;
        ///             Vector3 position = new Vector3(
        ///                          m_hawkState.data[floatOffset++] * movementScale,
        ///                          m_hawkState.data[floatOffset++] * movementScale,
        ///                         -m_hawkState.data[floatOffset++] * movementScale
        ///                     );
        ///             // todo : this position needs to be transformed into different space depending on what user is trying to do.
        ///             // Check out TrackingInput.cs line 148-160 for reference.
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
		public static int GetInputDeviceHandle(string name) {
			System.IntPtr ptr=Marshal.StringToHGlobalAnsi(name);
				int ret=NativeMethods.XDeviceGetInputDeviceHandle(ptr);
			Marshal.FreeHGlobal(ptr);
			return ret;
		}
		/// <summary>
		/// Get the name of input device identified by handle.
		/// </summary>
		public static string GetInputDeviceName(int which) {
			return Marshal.PtrToStringAnsi(NativeMethods.XDeviceGetInputDeviceName(which));
		}

		/// <summary>
		/// Get count of input devices in X-Device SDK.
		/// </summary>
		public static int GetInputDeviceCount() {
			return NativeMethods.XDeviceGetInputDeviceCount();
		}

		/// <summary>
		/// Get handles of all input devices identified by type.
		/// </summary>
		public static int GetInputDevices(int type,int[] whichBuffer,int whichBufferSize) {
			return NativeMethods.XDeviceGetInputDevices(type,whichBuffer,whichBufferSize);
		}
		/// <summary>
		/// Add an external controller device to X-Device SDK.
		/// </summary>
		public static int AddExternalControllerDevice(string name,ControllerStateDelegate converter,SendMessageDelegate sender) {
			System.IntPtr ptr=Marshal.StringToHGlobalAnsi(name);
				int ret=NativeMethods.XDeviceAddExternalControllerDevice(ptr,converter,sender);
			Marshal.FreeHGlobal(ptr);
			return ret;
		}

		/// <summary>
		/// Remove an input device identified by handle from X-Device SDK.
		/// </summary>
		public static System.IntPtr RemoveInputDeviceAt(int which) {
			return NativeMethods.XDeviceRemoveInputDeviceAt(which,true);
		}

        // I/O

        /// <summary>
        /// Get the input state of input device identified by handle.
        /// </summary>
        /// <param name="which">Device handle, which is grabbed by using XDevicePlugin.GetInputDeviceHandle().
        /// </param>
        /// <param name="state">An empty controller state, which is filled out with data inside of this function.
        /// </param>
        /// <example> 
        /// This example shows how to use the <see cref="GetInputState"/> method.
        /// <code>
        /// using UnityEngine;
        /// using Ximmerse;
        /// using Ximmerse.InputSystem;
        /// 
        /// class TestClass : MonoBehaviour
        /// {
        ///     private int m_leftControllerHandle;
        ///     private int m_hawkHandle;
        ///     
        ///     private XDevicePlugin.ControllerState m_leftControllerState;
        ///     private XDevicePlugin.TrackingStateEx m_hawkState;
        ///     
        ///     void Awake() 
        ///     {
        ///         XDevicePlugin.Init();
        ///         m_leftControllerHandle = XDevicePlugin.GetInputDeviceHandle("XCobra-0");
        ///         m_hawkHandle = XDevicePlugin.GetInputDeviceHandle("XHawk-0");
        ///     }
        ///     void Update()
        ///     {
        ///         // if this is larger than 0, it means it is valid input device;
        ///         if (m_leftControllerHandle >= 0)
        ///         {
        ///             UpdateLeftController();
        ///         }
        ///         if(m_hawkHandle>=0)
        ///         {
        ///             UpdateHawk();
        ///         }
        ///     }
        ///     
        ///     private void UpdateLeftController()
        ///     {
        ///         // You have to update the state manually. 
        ///         XDevicePlugin.UpdateInputState(m_leftControllerHandle);
        ///         XDevicePlugin.GetInputState(m_leftControllerHandle, ref m_leftControllerState);
        ///         var trigger = m_leftControllerState.axes[(int)ControllerRawAxis.LeftTrigger];
        ///         var xAxis = m_leftControllerState.axes[(int)ControllerRawAxis.LeftThumbX];
        ///         var yAxis = m_leftControllerState.axes[(int)ControllerRawAxis.LeftThumbY];
        ///         var orientation = new Quaternion(
        ///                 -m_leftControllerState.rotation[0],
        ///                 -m_leftControllerState.rotation[1],
        ///                  m_leftControllerState.rotation[2],
        ///                  m_leftControllerState.rotation[3]
        ///             );
        ///     }
        ///
        ///     private void UpdateHawk()
        ///     {
        ///         // You have to update the state manually. 
        ///         XDevicePlugin.UpdateInputState(m_hawkHandle);
        ///         XDevicePlugin.GetInputState(m_hawkHandle, ref m_hawkState);
        ///         // 0 = left controller, 1 = right controller 
        ///         int floatOffset = m_hawkState.OffsetOf(0);
        ///         // if this is -1, the position can not be found
        ///         // return to 0 if point is valid
        ///         if (floatOffset >= 0)
        ///         {
        ///             float[] rawPostionData = m_hawkState.GetData();
        ///             // convert the position to 1:1 scale movement. 
        ///             float movementScale = 0.001f;
        ///             Vector3 position = new Vector3(
        ///                          m_hawkState.data[floatOffset++] * movementScale,
        ///                          m_hawkState.data[floatOffset++] * movementScale,
        ///                         -m_hawkState.data[floatOffset++] * movementScale
        ///                     );
        ///             // todo : this position needs to be transformed into different space depending on what user is trying to do.
        ///             // Check out TrackingInput.cs line 148-160 for reference.
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
#if (UNITY_EDITOR_OSX||UNITY_STANDALONE_OSX||UNITY_IOS)
		public static int GetInputState(int which,ref ControllerState state){
			System.IntPtr ptr=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ControllerState)));
				int ret=NativeMethods.XDeviceGetInputState(which,ptr);
				if(ret==0) NativeMethods.ControllerStateFromPtr(ref state,ptr);
			Marshal.FreeHGlobal(ptr);
			return ret;
		}

		/// <summary>
		/// Get the input state of input device identified by handle.
		/// </summary>
		public static int GetInputState(int which,ref TrackerState state){
			System.IntPtr ptr=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrackerState)));
				int ret=NativeMethods.XDeviceGetInputState(which,ptr);
				if(ret==0) NativeMethods.TrackerStateFromPtr(ref state,ptr);
			Marshal.FreeHGlobal(ptr);
			return ret;
		}
#else
		public static int GetInputState(int which,ref ControllerState state){
			return NativeMethods.XDeviceGetInputState(which,ref state);
		}

		/// <summary>
		/// Get the input state of input device identified by handle.
		/// </summary>
		public static int GetInputState(int which,ref TrackerState state){
			return NativeMethods.XDeviceGetInputState(which,ref state);
		}
#endif


		/// <summary>
		/// Send a message to input device identified by handle.
		/// </summary>
		public static int SendMessage(int which,int Msg,int wParam,int lParam) {
			return NativeMethods.XDeviceSendMessage(which,Msg,wParam,lParam);
		}

		/// <summary>
		/// Update input device identified by handle.
		/// </summary>
		public static int UpdateInputState(int which) {
			return NativeMethods.XDeviceUpdateInputState(which);
		}
		
		/// <summary>
		/// Update the input buffer of input device identified by handle.
		/// </summary>
		////public static int UpdateInputBuffer(int which,System.IntPtr buffer,int offset,int count);
		
		public static bool GetBool(int which,int fieldID,bool defaultValue) {return NativeMethods.XDeviceGetBool(which,fieldID,defaultValue);}
		public static void SetBool(int which,int fieldID,bool value) {NativeMethods.XDeviceSetBool(which,fieldID,value);}
		
		public static int GetInt(int which,int fieldID,int defaultValue) {return NativeMethods.XDeviceGetInt(which,fieldID,defaultValue);}
		public static void SetInt(int which,int fieldID,int value) {NativeMethods.XDeviceSetInt(which,fieldID,value);}
		
		public static float GetFloat(int which,int fieldID,float defaultValue) {return NativeMethods.XDeviceGetFloat(which,fieldID,defaultValue);}
		public static void SetFloat(int which,int fieldID,float value) {NativeMethods.XDeviceSetFloat(which,fieldID,value);}
		
		public static void GetObject(int which,int fieldID,float[] data,int offset) {
			int size=(data.Length-offset)*4;
			System.IntPtr ptr=Marshal.AllocHGlobal(size);
				NativeMethods.XDeviceGetObject(which,fieldID,ptr,0,size);
				Marshal.Copy(ptr,data,offset,data.Length-offset);
			Marshal.FreeHGlobal(ptr);
		}

		// Ext

		public static TrackingResult GetNodePosition(int which,int history,int node,float[] position) {
			return (TrackingResult)NativeMethods.XDeviceGetNodePosition(which,history,node,position);
		}

		protected static float[] GetNodePosition_floats=new float[3];

		/// <summary>
		/// Get the node position of input device identified by handle.
		/// </summary>
		public static TrackingResult GetNodePosition(int which,int history,int node,ref UnityEngine.Vector3 position) {
			lock(GetNodePosition_floats){
				int ret=NativeMethods.XDeviceGetNodePosition(which,history,node,GetNodePosition_floats);
				if(ret>0) {
					position.Set(
						 GetNodePosition_floats[0],
						 GetNodePosition_floats[1],
						-GetNodePosition_floats[2]
					);
				}else{
					position.Set(0,0,0);
					return TrackingResult.NotTracked;
				}
				return (TrackingResult)ret;
			}
		}

		protected static float[] UpdateNodeRotation_floats=new float[4];

		/// <summary>
		/// Update the node rotation of input device identified by handle.
		/// </summary>
		public static int UpdateNodeRotation(int which,int node,UnityEngine.Quaternion rotation) {
			lock(UpdateNodeRotation_floats) {
				int i=0;
				UpdateNodeRotation_floats[i]=-rotation[i];++i;
				UpdateNodeRotation_floats[i]=-rotation[i];++i;
				UpdateNodeRotation_floats[i]= rotation[i];++i;
				UpdateNodeRotation_floats[i]= rotation[i];++i;
				//
				int ret=NativeMethods.XDeviceUpdateNodePose(which,node,null,UpdateNodeRotation_floats);
				return ret;
			}
		}

		public static int GetTrackerPose(int which,out float height,out float depth,out float pitch) {
			return NativeMethods.XDeviceGetTrackerPose(which,out height,out depth,out pitch);
		}

		public static int SetTrackerPose(int which,float height,float depth,float pitch) {
			return NativeMethods.XDeviceSetTrackerPose(which,height,depth,pitch);
		}

		public static int GetTickCount() {
			return NativeMethods.XDeviceGetTickCount();
		}
		
		#endregion Methods

	}
}