//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ximmerse.InputSystem {

	/// <summary>
	/// The ControllerInputManager is a helper class for managing ControllerInputs.
	/// </summary>
	public partial class ControllerInputManager:MonoBehaviour {

		#region Static

		protected static ControllerInputManager s_Instance;
		protected static bool s_InstanceCached=false;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <returns> ControllerInputManager Singleton</returns>
        public static ControllerInputManager instance {
			get {
				if(!s_InstanceCached) {//
					s_InstanceCached=true;
					if(s_Instance==null) {
						//
						ControllerInputManager newInstance=FindObjectOfType<ControllerInputManager>();
						//
						if(newInstance==null) {
							string prefabName="Default ControllerInput Manager";
							newInstance=(Instantiate(Resources.Load("Input/"+prefabName))
								as GameObject).GetComponent<ControllerInputManager>();
							newInstance.name=prefabName;
						}
						if(newInstance!=null) {
							newInstance.Awake();
							s_Instance=newInstance;
						}
					}
				}
				return s_Instance;
			}
		}

		/// <summary>
		/// The controller info in csv database.
		/// </summary>
		public class ControllerInfo {
			static readonly string[] SPLIT_NEW_LINE=new string[3]{"\r\n","\r","\n"};
			static readonly char[] SPLIT_CSV=new char[1]{','};
			public static Dictionary<string,ControllerInfo> ParseFromText(Dictionary<string,ControllerInfo> dest,string text) {
				string[] lines=text.Split(SPLIT_NEW_LINE,System.StringSplitOptions.RemoveEmptyEntries),csv;
				int imax=lines.Length;
				if(dest==null) {
					dest=new Dictionary<string, ControllerInfo>(imax);
				}
				//
				ControllerInfo info=null;
				for(int i=0,j;i<imax;++i) {
					if(string.IsNullOrEmpty(lines[i])||lines[i].StartsWith(";")) {
						continue;
					}
					//
					csv=lines[i].Split(SPLIT_CSV);
					j=0;
					csv[j]=csv[j].ToLower();
					if(dest.ContainsKey(csv[j])) {
						info=dest[csv[j]];
					}else {
						dest.Add(csv[j],info=new ControllerInfo());
					}
					//
					info.name=csv[j];++j;
					info.type=(ControllerType)System.Enum.Parse(typeof(ControllerType),csv[j]);++j;
					info.priority=int.Parse(csv[j]);++j;
				}
				//
				return dest;
			}

			//
			public string name;
			public ControllerType type;
			public int priority;
		}

		public static Dictionary<string,ControllerInfo> s_ControllerDatabase;

        /// <summary>
        /// Convert deviceName to ControllerType.
        /// </summary>
        /// <param name="s">The deviceName in X-Device SDK.</param>
        /// <returns>The ControllerType associated with deviceName</returns>
        
        public static ControllerType ParseControllerType(string s) {
			if(instance==null) {
				return ControllerType.None;
			}
			//
			ControllerType ret=ControllerType.None;
			s=s.ToLower();
			if(s_ControllerDatabase!=null) {
				ControllerInfo info;
				if(!s_ControllerDatabase.TryGetValue(s,out info)){
					s_ControllerDatabase.TryGetValue("*",out info);
				}
				if(info!=null) {
					ret=info.type;
					return ret;
				}
			}
			if(s=="vrdevice") {
				ret=ControllerType.Hmd;
			}else if(s.EndsWith("0")) {
				ret=ControllerType.LeftController;
			}else if(s.EndsWith("1")) {
				ret=ControllerType.RightController;
			}else{
				ret=ControllerType.Gamepad;
			}
			return ret;
		}

		/// <summary>
		/// Convert deviceName to ControllerPriority.
		/// </summary>
		/// <param name="s">The deviceName in X-Device SDK.</param>
		/// <returns>The ControllerPriority associated with deviceName</returns>
		public static int ParseControllerPriority(string s) {
			if(instance==null) {
				return -1;
			}
			//
			int ret=0;
			if(s_ControllerDatabase!=null) {
				s=s.ToLower();
				ControllerInfo info;
				if(!s_ControllerDatabase.TryGetValue(s,out info)){
					s_ControllerDatabase.TryGetValue("*",out info);
				}
				if(info!=null) {
					ret=info.priority;
				}
			}
			return ret;
		}
		
		protected static readonly ControllerInput s_EmptyInput=new ControllerInput(-1,"");
		
		public static ControllerInput GetInput(string name) {
			return instance.GetControllerInput(name);
		}

		public static ControllerInput GetInput(ControllerType type) {
			return instance.GetControllerInput(type);
		}

		#endregion Static

		#region Fields

		public bool dontDestroyOnLoad=true;
		[SerializeField]protected TextAsset m_ControllerDataTxt;
		public Dictionary<string,ControllerInput> controllerInputs;

		[Header("Events")]
		[SerializeField]protected UnityEvent m_OnInitialized;
		[System.NonSerialized]public System.Action onUpdate,onDestroy;
			
		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			//
			if(s_Instance==null) {
				Log.i("ControllerInputManager","Initialize successfully.");
				s_Instance=this;
			}else if(s_Instance!=this){
				Log.e("ControllerInputManager","Only one instance can be run!!!");
			}else {
				//Log.i("ControllerInputManager","Initialize again.");
				return;
			}
			//
			if(dontDestroyOnLoad) {
				transform.SetParent(null,false);
				DontDestroyOnLoad(this);
			}
			//
			SendMessage("OnStart",SendMessageOptions.DontRequireReceiver);
			//
			InitAllControllerInputs();
		}

		protected virtual void Update() {
			//
			if(onUpdate!=null) {
				onUpdate();
			}
		}

		protected virtual void OnDestroy() {
			s_InstanceCached=false;
			//
			if(onDestroy!=null) {
				onDestroy();
			}
			//
			XDevicePlugin.Exit();
		}
		
		#endregion Unity Messages

		#region Methods

		#region Controller Management

		/// <summary>
		/// Initialize all the input controller .
		/// </summary>
		public virtual void InitAllControllerInputs() {
			//
			int ret=XDevicePlugin.Init(); 
			if(ret!=0) {
				/*...*/
			}
			//
			if(m_ControllerDataTxt!=null) {
				s_ControllerDatabase=ControllerInfo.ParseFromText(
					s_ControllerDatabase,m_ControllerDataTxt.text);
			}
			//
#if XDEVICE_EXT
			InitExternalControllerDevices();
#endif
			//
			int whichBufferSize=XDevicePlugin.GetInputDeviceCount();
			int[] whichBuffer=new int[whichBufferSize];
			string name;
			int handle,i=0,imax=XDevicePlugin.GetInputDevices(1,whichBuffer,whichBufferSize);
			//
			if(controllerInputs==null) {
				controllerInputs=new Dictionary<string,ControllerInput>(imax);
			}else {
				controllerInputs.Clear();
			}
			System.Text.StringBuilder sb=new System.Text.StringBuilder();
			sb.AppendLine("ControllerInput List:");
			for(;i<imax;++i) {
				handle=whichBuffer[i];
				name=XDevicePlugin.GetInputDeviceName(handle);
				AddControllerInput(name,new ControllerInput(handle,name));
				//
				sb.AppendLine("\t{ControllerInput name=\""+name+"\" handle=\""+handle+"\"}");
			}
			Log.i("ControllerInputManager",sb.ToString());
			//
			m_OnInitialized.Invoke();
		}

		/// <summary>
		/// Add a controller Input.
		/// </summary>
		public virtual void AddControllerInput(string name,ControllerInput controllerInput){
			//
			if(controllerInput!=null) {
				if(controllerInput.type==ControllerType.None) {
					controllerInput.type=ParseControllerType(name);
				}
				if(controllerInput.priority<=0) {
					controllerInput.priority=ParseControllerPriority(name);
				}
			}
			//
			if(controllerInputs.ContainsKey(name)) {
				//Log.i("ControllerInputManager","ControllerInput@"+name+" : "+controllerInputs[name]+" -> "+controllerInput);
				controllerInputs[name]=controllerInput;
			}else {
				controllerInputs.Add(name,controllerInput);
			}
		}

        /// <summary>
        /// Get a Controller Input with a name string.
        /// </summary>
        /// <param name="name">The deviceName in X-Device SDK.</param>
        /// <returns>ControllerInput instance if device name matches. Return null if nothing found.</returns>
        /// <example> 
        /// This sample shows how to call the <see cref="GetControllerInput"/> method.
        /// <code>
        /// using UnityEngine;
        /// using Ximmerse;
        /// using Ximmerse.InputSystem;
        /// 
        /// class TestClass : MonoBehaviour
        /// {
        ///     void Start() 
        ///     {
        ///        ControllerInput leftController=ControllerInputManager.instance.GetControllerInput("LeftController");
        ///        ControllerInput rightController=ControllerInputManager.instance.GetControllerInput("RightController");
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual ControllerInput GetControllerInput(string name) {
			if(controllerInputs.ContainsKey(name)) {
				return controllerInputs[name];
			}else {
				try {
					return GetControllerInput((ControllerType)System.Enum.Parse(typeof(ControllerType),name,true));
				}catch(System.Exception e) {
					return null;
				}
			}
		}

		protected Dictionary<ControllerType,ControllerInput> m_GetInputByTypeCaches=
			new Dictionary<ControllerType, ControllerInput>();

        /// <summary>
        /// Get a ControllerInput with a type.
        /// </summary>
        /// <param name="type">a controller type</param>
        /// <returns>ControllerInput instance if a type matches. Return null if nothing found.</returns>
        /// <example> 
        /// This sample shows how to call the <see cref="GetControllerInput"/> method.
        /// <code>
        /// using UnityEngine;
        /// using Ximmerse;
        /// using Ximmerse.InputSystem;
        /// 
        /// class TestClass : MonoBehaviour
        /// {
        ///     void Start() 
        ///     {
        ///        ControllerInput leftController=ControllerInputManager.instance.GetControllerInput(ControllerType.LeftController);
        ///        ControllerInput rightController=ControllerInputManager.instance.GetControllerInput(ControllerType.RightController);
        ///     }
        /// }
        /// </code>
        /// </example>
        public virtual ControllerInput GetControllerInput(ControllerType type) {
			ControllerInput ret=null;
			// Get the result faster.
			if(m_GetInputByTypeCaches.TryGetValue(type,out ret)){
			}else {
				int priority=int.MinValue;
				foreach(ControllerInput input in controllerInputs.Values) {
					if((input.type&type)!=0) {
						if(input.priority>priority) {
							ret=input;
							priority=input.priority;
						}
					}
				}
				// Cache the result.
				m_GetInputByTypeCaches.Add(type,ret);
			}
			return ret;
		}

		#endregion Controller Management

		#endregion Methods

	}

}