//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

/// <summary>
/// A utility which implements listening input to call the toggle function without scripting.
/// </summary>
public class ButtonToggle:MonoBehaviour {

	[System.Serializable]
	public class Entry {
		public ButtonEvent buttonEvent;
		public bool value=true;
		public UnityEngine.Events.UnityEvent onTrue,onFalse;

		public void Toggle() {
			value=!value;
			if(value) {
				onTrue.Invoke();
			} else {
				onFalse.Invoke();
			}
		}
	}

	public Entry[] entries=new Entry[1];
	protected int m_NumEntries=-1;


	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start() {
		m_NumEntries=entries.Length;
	}

	/// <summary>
	/// 
	/// </summary>
	protected virtual void Update() {
		for(int i=0;i<m_NumEntries;++i) {
			if(entries[i].buttonEvent.GetAnyDown()) {
				entries[i].Toggle();
			}
		}
	}
}
