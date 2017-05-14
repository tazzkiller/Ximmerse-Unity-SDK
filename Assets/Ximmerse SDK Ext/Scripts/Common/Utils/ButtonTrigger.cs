//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

/// <summary>
/// A utility which implements listening input to call the function without scripting.
/// </summary>
public class ButtonTrigger:MonoBehaviour {

	[System.Serializable]
	public class Entry {
		public ButtonEvent buttonEvent;
		public UnityEngine.Events.UnityEvent onTrigger;
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
				entries[i].onTrigger.Invoke();
			}
		}
	}
}

