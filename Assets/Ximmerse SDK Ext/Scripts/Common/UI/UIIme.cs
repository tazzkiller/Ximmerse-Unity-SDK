using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keys=System.Windows.Forms.Keys;

namespace Ximmerse.UI {

	/// <summary>
	/// 
	/// </summary>
	public class UIIme:MonoBehaviour {

		#region Nested Types

		public class CandidateItem{
			//
			[System.NonSerialized]public UIIme context;
			public int index;
			public Keys key;

			// Unity Context.
			public Transform root;
			public GameObject gameObject;
			public Transform transform;

			// Unity UI Context.
			[System.NonSerialized]protected Button m_Button;
			[System.NonSerialized]protected Text m_IndexText;
			[System.NonSerialized]protected Text m_ValueText;

			public CandidateItem(UIIme context,int index,Transform root,GameObject gameObject) {
				this.context=context;
				this.index=index;
				this.root=root;
				//
				key=Keys.None;
				if(index>=0&&index<10) {
					key=(Keys)System.Enum.Parse(typeof(Keys),
						"D"+((index==9)?0:index+1));
				}
				//
				this.gameObject=Object.Instantiate(gameObject);
				this.gameObject.name=gameObject.name+"-"+key;
				this.transform=this.gameObject.transform;
				//
				this.transform.SetParent(this.root);
				Transform t=gameObject.transform;
				this.transform.localPosition=t.localPosition;
				this.transform.localRotation=t.localRotation;
				this.transform.localScale=t.localScale;
				//
				this.m_Button=this.gameObject.GetComponent<Button>();
				if(this.m_Button!=null) {
					this.m_Button.onClick.AddListener(OnButtonClicked);
				}
				//
				t=this.transform.FindChild("Text Index");
				if(t!=null) {
					this.m_IndexText=t.GetComponent<Text>();
				}
				t=this.transform.FindChild("Text Value");
				if(t!=null) {
					this.m_ValueText=t.GetComponent<Text>();
				}

				if(this.m_IndexText!=null) {
					string text=m_IndexText.text;
					//
					m_IndexText.text=(string.IsNullOrEmpty(text))?
						(key-Keys.D0).ToString():string.Format(text,(key-Keys.D0));
				}
			}

			public virtual void OnButtonClicked() {
				if(key!=Keys.None) {
					ImeApiPlugin.ime_simulate_key((int)key);
				}
			}

			public virtual void SetText(string value) {
				if(this.m_ValueText!=null) {
					//
					m_ValueText.text=value;
				}
			}

			public virtual void SetActive(bool active) {
				gameObject.SetActive(active);
			}
		}

		#endregion Nested Types

		#region Fields

		[SerializeField]protected Text m_CompositionText;
		[SerializeField]protected Transform m_CandidateRoot;
		[SerializeField]protected GameObject m_CandidatePrefab;

		[System.NonSerialized]protected List<CandidateItem> m_CandidateList;

		[System.NonSerialized]protected GameObject m_Go;
		[System.NonSerialized]protected bool m_SelfActive;

		#endregion Fields

		#region Unity Messages

		protected virtual void Start() {
			if(!CheckPlatform()) {
				Destroy(gameObject);
				return;
			}
			//
			ImeApiPlugin.ime_init();
			m_Go=gameObject;
			m_SelfActive=m_Go.activeSelf;
			Juggler.Main.UpdateCall(CheckImeOpened,0.0f,float.MaxValue);

			InitImeUI();
		}

		protected virtual void OnDestroy() {
			if(!CheckPlatform()) {
				return;
			}
			//
			ImeApiPlugin.ime_exit();
		}

		protected virtual void Update() {
			if(ImeApiPlugin.ime_is_open()) {
				UpdateImeUI();
			}
		}

		protected virtual void  OnEnable() {

		}

		protected virtual void  OnDisable() {

		}

		#endregion Unity Messages

		#region Methods

		public virtual bool CheckPlatform() {
			switch(Application.platform) {
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
				return true;
				default:
				Log.e("UIIme","Ime Api can only works on Windows.");
				return false;
			}
		}

		public virtual void CheckImeOpened() {
			bool isOpened = ImeApiPlugin.ime_is_open();
			if(isOpened!=m_SelfActive) {
				m_Go.SetActive(m_SelfActive=isOpened);
			}
		}

		public virtual void InitImeUI(){
			m_CandidateList=new List<CandidateItem>(10);
		}

		public virtual void UpdateImeUI(){
			ClearImeUI();
			string str;
			//
			str=ImeApiPlugin.ime_get_composition_string();
			if(m_CompositionText!=null) {
				m_CompositionText.text=str;
			}
			//
			int i=0,imax=ImeApiPlugin.ime_get_candidate_list_count();
			for(;i<imax;++i) {
				str=ImeApiPlugin.ime_get_candidate_list_item(i);
				AddCandidate(i,str);
			}
		}

		public virtual void ClearImeUI(){
			if(m_CompositionText!=null) {
				m_CompositionText.text="";
			}
			//
			CandidateItem item;for(int i=0,imax=m_CandidateList.Count;i<imax;++i) {item=m_CandidateList[i];
				if(item!=null) {
					item.SetText("");
					item.SetActive(false);
				}
			}
		}

		public virtual void AddCandidate(int index,string value) {
			//
			if(m_CandidatePrefab==null) {
				return;
			}
			//
			CandidateItem item=null;
			if(index>=m_CandidateList.Count) {
				item=new CandidateItem(this,index,m_CandidateRoot,m_CandidatePrefab);
				m_CandidateList.Add(item);
			}else {
				item=m_CandidateList[index];
			}
			//
			item.SetText(value);
			item.SetActive(true);
		}
		
		#endregion Methods

	}

}
