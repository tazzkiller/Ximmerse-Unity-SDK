//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ximmerse.UI {

	public class UILocalize:MonoBehaviour {

		#region Nested Types
		
		[System.Serializable]
		public class SystemLanguageGameObjectPair:UKeyValuePair<SystemLanguage,GameObject>{}

#if UNITY_EDITOR
		[UnityEditor.CustomPropertyDrawer(typeof(SystemLanguageGameObjectPair))]
		public class SystemLanguageGameObjectPairDrawer:UKeyValuePairDrawer<SystemLanguage,GameObject>{}
#endif

		#endregion Nested Types

		#region Fields

		public Localization localization;
		public string key="Text";

		[SerializeField]protected SystemLanguageGameObjectPair[] m_Text=new SystemLanguageGameObjectPair[1]{
			 new SystemLanguageGameObjectPair{key=SystemLanguage.Unknown}
		};
		[System.NonSerialized]protected Dictionary<SystemLanguage,GameObject> m_TextDic;
		[System.NonSerialized]protected SystemLanguage m_SystemLanguage=(SystemLanguage)(-1);
		
		#endregion Fields

		#region Unity Messages

		protected virtual void Awake() {
			m_TextDic=new Dictionary<SystemLanguage, GameObject>();
			for(int i=0,imax=m_Text.Length;i<imax;++i) {
				if(m_Text[i].value!=null) {
					m_Text[i].value.SetActive(false);
					m_TextDic.Add(m_Text[i].key,m_Text[i].value);
				}
			}
			//
			//SetLanguage(Application.systemLanguage);
		}

		protected virtual void OnEnable() {
			SetLanguage(Application.systemLanguage);
		}

		#endregion Unity Messages
		
		public virtual bool SetLanguage(SystemLanguage language) {
			if(language==m_SystemLanguage) {
				return true;
			}
			//
			GameObject go;
			if(m_TextDic.TryGetValue(m_SystemLanguage,out go)) {
				go.SetActive(false);
			}
			//
			if(m_TextDic.TryGetValue(language,out go)) {
				go.SetActive(true);
				//
				if(localization!=null) {
					Text text=go.GetComponent<Text>();
					if(text!=null) {
						text.text=localization.GetText(Application.systemLanguage,key);
					}
				}
				//
				m_SystemLanguage=language;
				return true;
			}else if(language!=SystemLanguage.Unknown){
				return SetLanguage(SystemLanguage.Unknown);
			}else {
				return false;
			}
		}
	}
}