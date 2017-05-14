//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Graphic))]
public class ButtonTweener:MonoBehaviour,
	IPointerEnterHandler,
	IPointerExitHandler
{

	#region Nested Types

	public enum ButtonState{
		Normal,
		NormalToFocus,
		Focus,
		FocusToNormal,
	};

	#endregion Nested Types

	#region Fields
	
	public ButtonState state=ButtonState.Normal;
	public float fadeDuration=.1f;
	public float clickDuration=.1f;

	[System.NonSerialized]protected float m_NormalTime,m_FocusTime;
	[System.NonSerialized]protected float m_FocusValue=0.0f,m_PrevFocusValue;

	[System.NonSerialized]protected bool m_IsClicking=false;
	[System.NonSerialized]protected float m_ClickTime=0.0f;

	[System.NonSerialized]protected Transform m_Transform;
	[System.NonSerialized]protected Button m_Button;

	#endregion Fields

	#region Unity Messages

	protected virtual void Awake() {
		m_Transform=GetComponent<Transform>();
		m_Button=GetComponent<Button>();
		//
		if(m_Button!=null) {
			fadeDuration=m_Button.colors.fadeDuration;
			m_Button.onClick.AddListener(OnButtonClicked);
		}
	}

	protected virtual void Update() {
		//
		m_PrevFocusValue=m_FocusValue;
		//
		switch(state) {
			case ButtonState.Normal:
				OnNormal(m_NormalTime+=Time.deltaTime);
			break;
			case ButtonState.NormalToFocus:
				m_FocusValue+=Time.deltaTime/fadeDuration;
				//
				if(m_FocusValue>=1.0f) {
					m_FocusTime=0.0f;
					state=ButtonState.Focus;
				}
			break;
			case ButtonState.Focus:
				OnFocus(m_FocusTime+=Time.deltaTime);
			break;
			case ButtonState.FocusToNormal:
				m_FocusValue-=Time.deltaTime/fadeDuration;
				//
				if(m_FocusValue<=0.0f) {
					m_NormalTime=0.0f;
					state=ButtonState.Normal;
				}
			break;
		}
		//
		if(m_FocusValue!=m_PrevFocusValue) {
			OnFocusValue(m_FocusValue=Mathf.Clamp01(m_FocusValue));
		}
		//
		if(m_IsClicking) {
			m_ClickTime-=Time.deltaTime;
			OnClickValue(Mathf.Clamp01(clickDuration-m_ClickTime)/clickDuration);
			//
			if(m_ClickTime<=0f) {
				m_IsClicking=false;
			}
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData) {
		if(state==ButtonState.Focus) {
			return;
		}
		//
		OnNormal(0.0f);
		state=ButtonState.NormalToFocus;
	}

	public virtual void OnPointerExit(PointerEventData eventData) {
		if(state==ButtonState.Normal) {
			return;
		}
		//
		OnFocus(0.0f);
		state=ButtonState.FocusToNormal;
	}

	#endregion Unity Messages

	#region Methods

	public virtual void OnNormal(float time) {
	}

	public virtual void OnFocus(float time) {
	}

	public virtual void OnFocusValue(float value) {
	}

	public virtual void OnButtonClicked() {
		m_IsClicking=true;
		m_ClickTime=clickDuration;
	}
	public virtual void OnClickValue(float value) {
	}

	#endregion Methods

}
