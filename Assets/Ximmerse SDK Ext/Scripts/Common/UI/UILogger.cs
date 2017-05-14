//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ximmerse;

public class UILogger:MonoBehaviour,Log.ILogger
#if UNITY_5&&!UNITY_5_0&&!UNITY_5_1
	,ILogHandler
#endif
{

	#region Fields

	[SerializeField]protected Text m_Text;
	[SerializeField]protected int m_MaxLines;
	[SerializeField]protected string m_Format="tag={0},msg={1}";
	[SerializeField]protected string m_FileName;
	
	[SerializeField]protected Color m_ColorV=Color.black;
	[SerializeField]protected Color m_ColorI=Color.black;
	[SerializeField]protected Color m_ColorD=Color.black;
	[SerializeField]protected Color m_ColorW=Color.yellow;
	[SerializeField]protected Color m_ColorE=Color.red;
	[System.NonSerialized]protected List<string> m_Lines;

	#endregion Fields

	#region Unity Messages

	protected virtual void Awake() {
		m_Lines=new List<string>();
		//
		Log.s_Logger=this;
#if UNITY_5&&!UNITY_5_0&&!UNITY_5_1
		Debug.logger.logHandler=this;
#endif
	}

	protected virtual void Update() {
		
	}
	

	protected virtual void OnDestroy() {
		if(!string.IsNullOrEmpty(m_FileName)) {
			System.IO.File.WriteAllLines(m_FileName,m_Lines.ToArray());
		}
		Log.s_Logger=null;
	}

	#endregion Unity Messages

	#region Methods

	public virtual int v(string tag,string msg) {
		PrintToUI(m_ColorV,string.Format(m_Format,tag,msg));
		return 0;
	}

	public virtual int i(string tag,string msg) {
		PrintToUI(m_ColorI,string.Format(m_Format,tag,msg));
		return 0;
	}

	public virtual int d(string tag,string msg) {
		PrintToUI(m_ColorD,string.Format(m_Format,tag,msg));
		return 0;
	}

	public virtual int w(string tag,string msg) {
		PrintToUI(m_ColorW,string.Format(m_Format,tag,msg));
		return 0;
	}

	public virtual int e(string tag,string msg) {
		PrintToUI(m_ColorE,string.Format(m_Format,tag,msg));
		return 0;
	}

	public virtual void LogFormat(LogType logType,UnityEngine.Object context,string format,params object[] args) {
		Color color=Color.black;
		switch(logType) {
			case LogType.Log:
			case LogType.Assert:
				color=m_ColorD;
			break;
			case LogType.Warning:
				color=m_ColorW;
			break;
			case LogType.Error:
			case LogType.Exception:
				color=m_ColorE;
			break;
		}
		PrintToUI(color,string.Format (format, args) );
	}
	
#if UNITY_5&&!UNITY_5_0&&!UNITY_5_1
	
	[System.NonSerialized]protected ILogHandler m_DefaultLogHandler=Debug.logger.logHandler;
	
	public virtual void LogException(System.Exception exception,Object context) {
		m_DefaultLogHandler.LogException(exception,context);
	}

#endif


	static string[] SPLIT_NEW_LINE=new string[3]{"\r\n","\r","\n"};
	public virtual void PrintToUI(Color color,string text) {
		//
		if(!isActiveAndEnabled){return;}
		//
		string colorHex=(
			((int)(color.r*0xFF)<<24)|
			((int)(color.g*0xFF)<<16)|
			((int)(color.b*0xFF)<< 8)|
			((int)(color.a*0xFF)<< 0)
		).ToString("x8");
		string[] lines=text.Split(SPLIT_NEW_LINE,System.StringSplitOptions.RemoveEmptyEntries);
		int i=0,imax=lines.Length;
		if(imax<=1) {
			m_Lines.Add(string.Format(" <color=#{0}>{1}</color>",colorHex,text));
		}else{
			for(;i<imax;++i) {
				m_Lines.Add(string.Format(" <color=#{0}>{1}</color>",colorHex,lines[i]));
			}
		}
		//
		imax=m_Lines.Count;
		if(imax>m_MaxLines) {
			DisplayText(imax-m_MaxLines,m_MaxLines);
		}else {
			DisplayText(0,imax);
		}
	}

	public virtual void DisplayText(int lineIndex,int numLines) {
		if(m_Text!=null) {
			System.Text.StringBuilder sb=new System.Text.StringBuilder();
			while(numLines-->0) {
				sb.AppendLine(m_Lines[lineIndex++]);
			}
			m_Text.text=sb.ToString();
		}
	}

	#endregion Methods

}