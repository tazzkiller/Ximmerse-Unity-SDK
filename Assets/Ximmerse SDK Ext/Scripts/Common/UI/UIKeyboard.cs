//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;

namespace Ximmerse.UI {

	/// <summary>
	/// Interface to display a virtual keyboard in UGUI,without platform limitations.
	/// You can use it to type in VR,but it's a lot of works to do for implementation of IME.
	/// </summary>
	public class UIKeyboard:MonoBehaviour {

		#region Static
	
		public static readonly string[] SPLIT_NEW_LINE=new string[3]{"\r","\n","\r\n"};
		public static readonly char[] SPLIT_CSV=new char[1]{','};
	
		public static UIKeyboard s_Instance;

		public static UIKeyboard GetInstanceAt(UIInputField inputField,Transform parent,InputField.ContentType contentType) {
			if(s_Instance!=null) {
				if(parent==null) {
					parent=s_Instance.m_DefaultParent;
				}
				Transform t=s_Instance.transform;
				t.SetParent(parent,false);
				t.localPosition=Vector3.zero;
				t.localRotation=Quaternion.identity;
				s_Instance.gameObject.SetActive(true);
				//
				s_Instance.m_InputField=inputField;
				s_Instance.SetPage(0);
				inputField.onEndEdit.AddListener(s_Instance.OnEndEdit);
				return s_Instance;
			}
			return null;
		}

		#endregion Static

		#region Nested Types

		/// <summary>
		/// Representation of keys or buttons on UIKeyboard.
		/// </summary>
		public class KeyEntity{

			protected UIKeyboard m_Context;
			protected Button m_Button;
			protected Text m_Text;
			public KeyCommand cmd;
			public int type,page;
			public string text;
			public float x,y;
			public string value,up,shift,capslock;

			public KeyEntity(UIKeyboard context,string[] csv) {
				m_Context=context;
				ParseCsv(csv);
				//
				GameObject go=m_Context.prefabs[type];
				GameObject clone=Object.Instantiate(go);
				Transform cloneT=clone.transform,goT=go.transform;
				//
				if(text.StartsWith("@")) {
					Transform t=cloneT.FindChild(text);
					if(t!=null) {
						t.gameObject.SetActive(true);
					}
					Text txt=clone.GetComponentInChildren<Text>();
					if(txt!=null) {
						txt.gameObject.SetActive(false);
					}
				}else {
					m_Text=clone.GetComponentInChildren<Text>();
					UpdateText();
				}
				SetButton(clone.GetComponent<Button>());
				//
				clone.name=go.name;
				cloneT.SetParent(m_Context.pages[page]);
				cloneT.localPosition=m_Context.start+
					Vector3.Scale(m_Context.offset,new Vector3(x,y));
				cloneT.localRotation=goT.localRotation;
				cloneT.localScale=goT.localScale;
			}

			public virtual void ParseCsv(string[] csv) {
				int i=0;
				type=int.Parse(csv[i++]);
				text=csv[i++];
				page=int.Parse(csv[i++]);
				x=float.Parse(csv[i++]);
				y=float.Parse(csv[i++]);
				value=ParseString(csv[i++]);
				up=ParseString(csv[i++]);
				shift=ParseString(csv[i++]);
				capslock=ParseString(csv[i++]);
				//
				if(value.StartsWith("@")){
					cmd=(KeyCommand)System.Enum.Parse(typeof(KeyCommand),value.Substring(1));
				}else{
					cmd=KeyCommand.Text;
					if(string.IsNullOrEmpty(shift)) {
						shift=value.ToUpper();
					}
					if(string.IsNullOrEmpty(capslock)) {
						capslock=value.ToUpper();
					}
				}
			}

			public static string ParseString(string s) {
				if(!string.IsNullOrEmpty(s)&&s.StartsWith("&#")) {
					return ((char)(int.Parse(s.Substring(2,s.Length-3)))).ToString();
				}
				return s;
			}

			public virtual void SetButton(Button button) {
				if(button!=null) {
					m_Button=button;
					m_Button.onClick.AddListener(this.OnClick);
				}
			}

			public virtual void UpdateText() { 
				if(m_Text!=null) {
					//
					switch(cmd) {
						case KeyCommand.Text:
							m_Text.text=(string.IsNullOrEmpty(up)?"":up+"\n")+Text;
						break;
						default:
							m_Text.text=text;
						break;
					}
				}
			}

			public virtual void OnClick() {
				if(m_Context!=null) {
					m_Context.OnClick(this);
				}
			}

			public virtual string Text {
				get {
					string str=null;
					if(m_Context.isShift) {
						str=shift;
					}else if(m_Context.isCapsLock){
						str=capslock;
					}else{
						str=value;
					}
					return str;
				}
			}
		}

		public enum KeyCommand{
			Text,
			CapsLock,
			Shift,
			Backspace,
			Delete,
			Enter,
			GotoPage
		}


		#endregion Nested Types

		#region Fields

		public TextAsset csvFile=null;
		public Transform[] pages=new Transform[1];
		[System.NonSerialized]protected GameObject[] m_PageGos;
		public GameObject[] prefabs=new GameObject[1];
		public Vector3 start,offset=new Vector3(100f,-100f,0f);
	
		[System.NonSerialized]protected Transform m_DefaultParent;
		[System.NonSerialized]protected UIInputField m_InputField;
		[System.NonSerialized]protected KeyEntity[] m_KeyEntities;
		[System.NonSerialized]public bool isShift,isCapsLock;
		[System.NonSerialized]protected Event m_KeyEvent=new Event();
	
		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			if(s_Instance==null) {
				s_Instance=this;
			}else if(s_Instance!=this){
				Log.e("UIKeyboard","Only one instance can be run!!!");
				return;
			}
			m_DefaultParent=transform.parent;
			gameObject.SetActive(false);
			SetPage(0);
		}

		protected virtual void Start() {
			if(csvFile!=null) {
				LoadLines(csvFile.text.Split(SPLIT_NEW_LINE,System.StringSplitOptions.RemoveEmptyEntries));
			}
		}

		#endregion Unity Messages

		#region Methods

		public virtual void LoadLines(string[] lines) {
			int i=0,imax=lines.Length,k=0;
			m_KeyEntities=new KeyEntity[imax];
			for(;i<imax;++i) {
				if(string.IsNullOrEmpty(lines[i])||lines[i].StartsWith(";")) {
					continue;
				}
				m_KeyEntities[k++]=new KeyEntity(this,lines[i].Split(SPLIT_CSV));
			}
		}

		public virtual void OnEndEdit(string s) {
			if(m_InputField!=null) {
				m_InputField.onEndEdit.RemoveListener(OnEndEdit);
				//
				gameObject.SetActive(false);
				m_InputField.OnDeselect(null);
				m_InputField=null;
			}
		}

		public virtual void OnClick(KeyEntity key) {
			bool isEvent=false,isUpdateKeys=false;
			switch(key.cmd) {
				//
				case KeyCommand.Text:
					AppendText(key.Text);
					if(isShift) {
						isShift=false;
						isUpdateKeys=true;
					}
				break;
				//
				case KeyCommand.Backspace:
					m_KeyEvent.keyCode=KeyCode.Backspace;
					isEvent=true;
				break;
				case KeyCommand.Delete:
					m_KeyEvent.keyCode=KeyCode.Delete;
					isEvent=true;
				break;
				case KeyCommand.Enter:
					m_KeyEvent.keyCode=KeyCode.Return;
					isEvent=true;
				break;
				//
				case KeyCommand.CapsLock:
					isCapsLock=!isCapsLock;
					isEvent=false;
					isUpdateKeys=true;
				break;
				case KeyCommand.Shift:
					isShift=!isShift;
					isEvent=true;
					isUpdateKeys=true;
				break;
				case KeyCommand.GotoPage:
					SetPage(int.Parse(key.shift));
				break;
			}
			if(isEvent) {
				if(m_InputField!=null) {
					m_InputField.Select();
					m_InputField.ProcessEvent(m_KeyEvent);
					//if(m_KeyEvent.keyCode==KeyCode.Return) {
					//	gameObject.SetActive(false);
					//	m_InputField.OnDeselect(null);
					//}
				}
			}
			if(isUpdateKeys) {
				int i=m_KeyEntities.Length;
				while(i-->0) {
					if(m_KeyEntities[i]!=null) {
						m_KeyEntities[i].UpdateText();
					}
				}
			}
		}

		public virtual void SetPage(int page) {
			int i=0,imax=pages.Length;
			if(m_PageGos==null) {
				m_PageGos=new GameObject[imax];
				for(i=0;i<imax;++i) {
					m_PageGos[i]=pages[i]==null?null:pages[i].gameObject;
				}
			}
			for(i=0;i<imax;++i) {
				if(m_PageGos[i]!=null) m_PageGos[i].SetActive(false);
			}
			if(m_PageGos[page]!=null) m_PageGos[page].SetActive(true);
		}

		public virtual void AppendText(string input) {
			if(m_InputField!=null) {
				m_InputField.Select();
				m_InputField.AppendText(input);
			}
		}

		#endregion Methods

	}

}