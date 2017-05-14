//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ximmerse.InputSystem {

#if UNITY_5 && !UNITY_5_0
	[CreateAssetMenu(fileName="New Input Transform",menuName="Ximmerse SDK/Input Transform",order=800)]
#endif
	public class InputTransform:ScriptableObject {

		public int maxButtons=0;

		public string enumTypeFrom;
		[HideInInspector]public int[] buttonMasksFrom=new int[0];
		public string enumTypeTo;
		[HideInInspector]public int[] buttonMasksTo=new int[0];

		public virtual uint TransformButtons(uint buttons) {
			return (uint)TransformButtons((int)buttons);
		}

		public virtual int TransformButtons(int buttons){
			int ret=0;
			for(int i=0,imax=maxButtons;i<imax;++i) {
				if((buttons&buttonMasksFrom[i])==buttonMasksFrom[i]) {
					ret|=buttonMasksTo[i];
				}
			}
			return ret;
		}
	}

#if UNITY_EDITOR

	public class EnumCollection {

		protected string mType;
		protected string[] mNames;
		protected int[] mValues;

		public EnumCollection(string typeName) {
			mType=typeName;
			try{
				System.Type type=System.Type.GetType(typeName,true,true);
				mNames=System.Enum.GetNames(type);
				mValues=(int[])System.Enum.GetValues(type);
			}catch(System.Exception e) {
				throw e;
			}
		}

		public int GetIndex(string name) {
			if(mNames!=null) {
				for(int i=0,imax=mNames.Length;i<imax;++i) {
					if(name==mNames[i]) {
						return i;
					}
				}
			}
			return -1;
		}

		public int GetIndex(int value) {
			if(mValues!=null) {
				for(int i=0,imax=mValues.Length;i<imax;++i) {
					if(value==mValues[i]) {
						return i;
					}
				}
			}
			return -1;
		}

		public string[] GetNames() {
			return mNames;
		}

		public string GetName(int index){
			if(index>=0&&index<mNames.Length) {
				return mNames[index];
			}else {
				return "";//-1;//
			}
		}

		public int GetValue(int index){
			if(index>=0&&index<mValues.Length) {
				return mValues[index];
			}else {
				return 0;//-1;//
			}
		}

		public int Parse(string s){
			return GetValue(GetIndex(s));
		}

		public string ToString(int value) {
			return GetName(GetIndex(value));
		}

		public void OnGUI(Object undoObj,ref int value,params GUILayoutOption[] options) {
			GUI.changed=false;
			value=this.GetValue(
				EditorGUILayout.Popup(
					this.GetIndex(value),
					this.GetNames(),
					options
				)
			);
			if(GUI.changed) {
				EditorUtility.SetDirty(undoObj);
			}
		}
	}

	[CustomEditor(typeof(InputTransform),true)]
	public class InputTransformEditor:Editor {
		
		protected EnumCollection m_EnumFrom,m_EnumTo;
		protected bool m_IsFoldout;
		protected GUIStyle m_HeaderStyle;

		protected virtual void OnEnable() {
			m_IsFoldout=EditorPrefs.GetBool("InputTransformEditor.m_IsFoldout",false);
		}

		protected virtual void OnDisable() {
			EditorPrefs.SetBool("InputTransformEditor.m_IsFoldout",m_IsFoldout);
		}

		protected virtual bool EnsureEnum(string en,ref EnumCollection ec) {
			if(ec==null) {
				try {
					ec=new EnumCollection(en);
					return true;
				}catch(System.Exception e) {
					ec=null;
					return false;
				}
			}else {
				return true;
			}
		}

		protected virtual void SaveAsText(string path,InputTransform transform){
			System.Text.StringBuilder sb=new System.Text.StringBuilder();
			for(int i=0,imax=transform.maxButtons;i<imax;++i) {
				sb.AppendLine(
					m_EnumFrom.ToString(transform.buttonMasksFrom[i])
					+","+
					m_EnumTo.ToString(transform.buttonMasksTo[i])
				);
			}
			File.WriteAllText(path,sb.ToString());
			AssetDatabase.Refresh(ImportAssetOptions.Default);
		}

		protected virtual void LoadAsText(string path,InputTransform transform){
			string[] lines=File.ReadAllLines(path),csvData;
			System.Collections.Generic.List<int> list0=new System.Collections.Generic.List<int>();
			System.Collections.Generic.List<int> list1=new System.Collections.Generic.List<int>();
			for(int i=0,imax=lines.Length;i<imax;++i) {
				if(!string.IsNullOrEmpty(lines[i])) {
					csvData=lines[i].Split(',');
					if(csvData.Length>=2) {
						list0.Add(m_EnumFrom.Parse(csvData[0]));
						list1.Add(m_EnumTo.Parse(csvData[1]));
					}
				}
			}
			transform.buttonMasksFrom=list0.ToArray();
			transform.buttonMasksTo=list1.ToArray();
		}
		
		/// <summary>
		/// 
		/// </summary>
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			InputTransform target_=target as InputTransform;

			if(target_.maxButtons>target_.buttonMasksFrom.Length) {
				if(target_.buttonMasksFrom==null) {
					target_.buttonMasksFrom=new int[target_.maxButtons];
					target_.buttonMasksTo=new int[target_.maxButtons];
				}else {
					System.Array.Resize(ref target_.buttonMasksFrom, target_.maxButtons);
					System.Array.Resize(ref target_.buttonMasksTo, target_.maxButtons);
				}
			}
			//
			if(!EnsureEnum(target_.enumTypeFrom,ref m_EnumFrom)) {
				return;
			}
			if(!EnsureEnum(target_.enumTypeTo,ref m_EnumTo)) {
				return;
			}
			EditorGUILayout.Space();
			//
			if(m_HeaderStyle==null) {
				m_HeaderStyle=new GUIStyle(GUI.skin.label);
				m_HeaderStyle.fontStyle=FontStyle.Bold;
				m_HeaderStyle.fontSize=12;
			}
			EditorGUILayout.LabelField("Buttons",m_HeaderStyle);
			GUILayout.BeginHorizontal();
				if(GUILayout.Button("Save")) {
					string path=EditorUtility.SaveFilePanel("Save","",target_.name,"txt");
					if(!string.IsNullOrEmpty(path)) {
						SaveAsText(path,target_);
					}
				}
				if(GUILayout.Button("Load")) {
					string path=EditorUtility.OpenFilePanel("Load","","txt");
					if(!string.IsNullOrEmpty(path)) {
						LoadAsText(path,target_);
					}
				}
			GUILayout.EndHorizontal();
			//
			if(m_IsFoldout=EditorGUILayout.Foldout(m_IsFoldout,"Transform")){
				GUILayoutOption myWidth=null;
				GUILayout.BeginHorizontal();
					GUILayout.BeginVertical();myWidth=GUILayout.MinWidth(100f);
						EditorGUILayout.LabelField("From("+target_.enumTypeFrom+")",myWidth);
						for(int i=0,imax=target_.maxButtons;i<imax;++i) {
							m_EnumFrom.OnGUI(target_,ref target_.buttonMasksFrom[i],myWidth);
						}
					GUILayout.EndVertical();
					GUILayout.BeginVertical();myWidth=GUILayout.Width(24f);
						EditorGUILayout.LabelField("",myWidth);
						for(int i=0,imax=target_.maxButtons;i<imax;++i) {
							EditorGUILayout.LabelField("=>",myWidth);
						}
					GUILayout.EndVertical();
				GUILayout.BeginHorizontal();
					GUILayout.BeginVertical();myWidth=GUILayout.MinWidth(100f);
						EditorGUILayout.LabelField("To("+target_.enumTypeTo+")",myWidth);
						for(int i=0,imax=target_.maxButtons;i<imax;++i) {
							m_EnumTo.OnGUI(target_,ref target_.buttonMasksTo[i],myWidth);
						}
					GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}
	}

#endif
}