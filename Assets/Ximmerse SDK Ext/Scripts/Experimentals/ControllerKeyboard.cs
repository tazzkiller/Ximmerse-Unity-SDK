using System.Collections.Generic;
using UnityEngine;
using Ximmerse.InputSystem;
using Ximmerse.UI;

using Keys=System.Windows.Forms.Keys;

public class ControllerKeyboard:ControllerMonitor {

	#region Static

	public static readonly string[] SPLIT_NEW_LINE=new string[3]{"\r","\n","\r\n"};
	public static readonly char[] SPLIT_CSV=new char[1]{','};

	public static string ParseString(string s) {
		if(!string.IsNullOrEmpty(s)&&s.StartsWith("&#")) {
			return ((char)(int.Parse(s.Substring(2,s.Length-3)))).ToString();
		}
		return s;
	}

	#endregion Static

	#region Fields

	[Header("UI")]
	[SerializeField]protected TextAsset m_KeyNamesTxt;
	[System.NonSerialized]protected Dictionary<int,string> m_KeyNames;
	[SerializeField]protected UITextable[] m_UIKeys=new UITextable[4];

	[Header("Keyboard")]
	[SerializeField]protected ControllerButton m_ButtonShiftL;
	[SerializeField]protected ControllerButton m_ButtonShiftR;
	[SerializeField]protected InputTransform[] m_InputMappings=new InputTransform[0];
	[SerializeField]protected InputTransform[] m_ShiftInputMappings=new InputTransform[0];

	[System.NonSerialized]protected bool m_IsShiftPrev;

	#endregion Fields

	#region Unity Messages

	protected override void Start() {
		base.Start();
		//
		if(m_KeyNamesTxt!=null) {
			string[] csvData,lines=m_KeyNamesTxt.text.Split(SPLIT_NEW_LINE,System.StringSplitOptions.RemoveEmptyEntries);
			int i=0,imax=lines.Length;
			m_KeyNames=new Dictionary<int, string>(imax);
			for(;i<imax;++i) {
				if(string.IsNullOrEmpty(lines[i])||lines[i].StartsWith(";")) {
					continue;
				}
				//
				csvData=lines[i].Split(SPLIT_CSV);
				m_KeyNames.Add(
					(int)System.Enum.Parse(typeof(Keys),csvData[0]),
					ParseString(csvData[1])
				);
			}
		}
	}

	protected override void Update() {
		base.Update();
		//
		bool isShift=IsShiftMode();
		if(m_IsShiftPrev!=isShift) {
			m_IsShiftPrev=isShift;
			SetSubKeyboardActive(m_CurrentStateId,true,isShift);
		}
	}

	#endregion Unity Messages

	#region Methods

	/// <summary>
	/// 
	/// </summary>
	public override bool IsShiftMode() {
		if(m_ControllerInput!=null) {
			return m_ControllerInput.GetButton((m_ButtonShiftL|m_ButtonShiftR));
		}
		return false;
	} 

	public override void OnStateChanged(int oldState,int newState) {
		SetSubKeyboardActive(oldState,false);
		SetSubKeyboardActive(newState,true,IsShiftMode());
	}
	

	public override void OnInputUpdated(uint prev_buttons,uint buttons,bool isShift=false) {
		if(m_CurrentStateId==-1) {
			return;
		}
		//
		InputTransform inputMapping=isShift?m_ShiftInputMappings[m_CurrentStateId]:m_InputMappings[m_CurrentStateId];
		if(inputMapping!=null) {
			int buttonMask;
			for(int i=0,imax=inputMapping.buttonMasksFrom.Length;i<imax;++i) {
				buttonMask=inputMapping.buttonMasksFrom[i];
				//
				if((prev_buttons&buttonMask)==0&&(buttons&buttonMask)!=0) {// Down
					//
					ImeApiPlugin.ime_simulate_key(inputMapping.buttonMasksTo[i]);
				}else if((prev_buttons&buttonMask)!=0&&(buttons&buttonMask)==0) {// Up
					//
				}
			}
		}
	}

	public virtual void SetSubKeyboardActive(int id,bool value,bool isShift=false) {
		if(id==-1){
		} else {
			InputTransform inputMapping=isShift?m_ShiftInputMappings[id]:m_InputMappings[id];
			if(inputMapping!=null) {
				string text=null;
				for(int i=0,imax=m_UIKeys.Length;i<imax;++i) {
					if(m_UIKeys[i]!=null) {
						if(m_KeyNames.TryGetValue(inputMapping.buttonMasksTo[i],out text)) {
							m_UIKeys[i].Display(text);
						}else {
							m_UIKeys[i].Display(((Keys)inputMapping.buttonMasksTo[i]).ToString());
						}
					}
				}
				return;
			}
		}
		// Deactivate...
		for(int i=0,imax=m_UIKeys.Length;i<imax;++i) {
			if(m_UIKeys[i]!=null) {
				m_UIKeys[i].Display(null);
			}
		}
	}

	#endregion Methods

}
