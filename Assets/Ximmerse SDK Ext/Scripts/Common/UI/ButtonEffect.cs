//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;

public class ButtonEffect:ButtonTweener {

	#region Fields
	
	[Header("Focus")]
	public int focusRotAxis=2;
	public float focusRotScale=1.0f;
	public AnimationCurve focusRotCurve=AnimationCurve.Linear(0,0,1,0);
	public float normalScale=1.0f,focusScale=1.0f;
	
	[Header("Click")]
	public AnimationCurve clickCurve=AnimationCurve.Linear(0,0,1,0);

	[System.NonSerialized]protected float m_InitScale;
	[System.NonSerialized]protected Vector3 m_InitPosition;

	#endregion Fields

	#region Unity Messages

	protected virtual void Start() {
		m_InitScale=m_Transform.localScale.x;
		m_InitPosition=m_Transform.localPosition;
	}

	protected override void Update() {
		base.Update();
		//
	}

	#endregion Unity Messages

	#region Methods

	public override void OnFocus(float time) {
		Vector3 euler=Vector3.zero;
		euler[focusRotAxis]=focusRotCurve.Evaluate(time)*focusRotScale;
		m_Transform.localRotation=Quaternion.Euler(euler);
	}

	public override void OnFocusValue(float value) {
		m_Transform.localScale=Vector3.one*(m_InitScale*Mathf.Lerp(normalScale,focusScale,value));
	}

	public override void OnClickValue(float value) {
		m_Transform.localPosition=m_InitPosition+Vector3.up*(clickCurve.Evaluate(value));
	}

	#endregion Methods
}
