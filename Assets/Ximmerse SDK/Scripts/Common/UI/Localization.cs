//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using System.Collections.Generic;

namespace Ximmerse.UI {

	public class Localization:MonoBehaviour{

		#region Fields

		protected static readonly string[] SPLIT_NEW_LINE=new string[3]{"\r\n","\r","\n"};
		protected static readonly string[] SPLIT_CSV=new string[1]{","};

		public TextAsset textAsset;
		[System.NonSerialized]protected Dictionary<SystemLanguage,Dictionary<string,string>> m_StringTable=null;
		[System.NonSerialized]protected bool m_IsInited=false;

		#endregion Fields

		#region Methods

		public virtual void Init() {
			if(m_IsInited) {
				return;
			}
			m_IsInited=true;
			//
			m_StringTable=new Dictionary<SystemLanguage, Dictionary<string, string>>();
			if(textAsset==null){return;}
			//
			Dictionary<string,string>[] maps=null;
			string[] lines=textAsset.text.Split(SPLIT_NEW_LINE,System.StringSplitOptions.RemoveEmptyEntries);
			string[] csv;
			string str;
			int j,jmax;
			for(int i=0,imax=lines.Length;i<imax;++i) {
				if(string.IsNullOrEmpty(lines[i])||lines[i].StartsWith(";")) {
					continue;
				}
				//
				csv=lines[i].Split(SPLIT_CSV,System.StringSplitOptions.None);
				if(maps==null) {
					j=0;jmax=csv.Length-1;
					maps=new Dictionary<string, string>[jmax];
					for(;j<jmax;++j) {
						maps[j]=new Dictionary<string, string>();
						m_StringTable.Add(
							(SystemLanguage)System.Enum.Parse(typeof(SystemLanguage),csv[1+j]),
							maps[j]
						);
					}
				}else {
					j=0;jmax=csv.Length-1;
					for(;j<jmax;++j) {
						str=csv[1+j];
						str=str.Replace("&#44;",",").Replace("\\n","\n");
						//
						maps[j].Add(csv[0],str);
					}
				}
			}
			//
			m_StringTable.Add(SystemLanguage.Unknown,m_StringTable[SystemLanguage.English]);
		}

		public virtual string GetText(SystemLanguage language,string key) {
			if(!m_IsInited) {
				Init();
			}
			//
			Dictionary<string,string> map;
			if(m_StringTable.TryGetValue(language,out map)) {
				if(map.ContainsKey(key)) {
					return map[key];
				}else if(language!=SystemLanguage.Unknown){
					return GetText(SystemLanguage.Unknown,key);
				}
			}else if(language!=SystemLanguage.Unknown){
				return GetText(SystemLanguage.Unknown,key);
			}
			return "";
		}

		#endregion Methods

	}
}