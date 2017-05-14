//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ximmerse.UI {

	/// <summary>
	/// An InputField using third party input.
	/// </summary>
	[AddComponentMenu("UI/VR Input Field",0x1f)]
	public class UIInputField:InputField{

		#region Fields

		[SerializeField]protected Transform m_KeyboardAnchor;
		protected UIKeyboard m_UIKeyboard;

		#endregion Fields

		#region Hacked Methods

		public new bool shouldHideMobileInput {
			get {
				return true;
			}
			set {
				base.shouldHideMobileInput=value;
			}
		}

		public override void OnSelect(BaseEventData eventData){
			base.OnSelect(eventData);
			if(m_UIKeyboard==null) {
				m_UIKeyboard=UIKeyboard.GetInstanceAt(this,m_KeyboardAnchor,contentType);
			}
		}
	
		public override void OnDeselect(BaseEventData eventData){
			if(eventData==null) {
				m_UIKeyboard=null;
			}
			//
			if(m_UIKeyboard!=null) {
				return;
			}else{
				base.OnDeselect(eventData);
			}
		}

		public virtual void AppendChar(char input) {
			/*if(!this.m_ReadOnly&&this.InPlaceEditing()) */{
				if(this.onValidateInput!=null) {
					input=this.onValidateInput(this.text,this.caretPositionInternal,input);
				} else if(this.characterValidation!=CharacterValidation.None) {
					input=this.Validate(this.text,this.caretPositionInternal,input);
				}
				if(input!='\0') {
					this.InsertChar(input);
				}
			}
		}

		public virtual void AppendText(string input) {
			/*if(!this.m_ReadOnly&&this.InPlaceEditing()) */{
				int num = 0;
				int length = input.Length;
				while(num<length) {
					char ch = input[num];
					if(((ch>=' ')||(ch=='\t'))||(((ch=='\r')||(ch=='\n'))||(ch=='\n'))) {
						this.AppendChar(ch);
					}
					num++;
				}
			}
		}

		protected System.Reflection.MethodInfo m_InsertCharImpl;
		public virtual void InsertChar(char c) {
			if(m_InsertCharImpl==null) {
				System.Type clazz=typeof(InputField);
				m_InsertCharImpl=clazz.GetMethod(
					"Insert",ReflectUtil.sBindingFlags,null,
					new System.Type[1]{typeof(char)},null
				);
			}
			m_InsertCharImpl.Invoke(this,new object[1]{c});
#if UNITY_EDITOR
			OnValidate();
#endif
		}
	
		#endregion Hacked Methods

	}

}