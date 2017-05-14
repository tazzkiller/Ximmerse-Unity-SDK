using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.UI {
	

#if UNITY_5 && !UNITY_5_0
	[CreateAssetMenu(fileName="New UI Atlas",menuName="Ximmerse SDK/UI Atlas",order=800)]
#endif
	public class UIAtlas:ScriptableObject {

		#region Nested Types

		[System.Serializable]
		public class StringSpritePair:UKeyValuePair<string,Sprite>{}

#if UNITY_EDITOR

		[UnityEditor.CustomPropertyDrawer(typeof(StringSpritePair))]
		public class StringSpritePairDrawer:UKeyValuePairDrawer<string,Sprite>{}

#endif

		#endregion Nested Types

		#region Fieds

		[SerializeField]protected StringSpritePair[] m_Sprites=new StringSpritePair[0];
		[System.NonSerialized]protected Dictionary<string,StringSpritePair> m_SpriteMap;
		[System.NonSerialized]protected bool m_Inited=false;

		#endregion Fieds

		#region Methods

		protected virtual void InitAtlas() {
			if(m_Inited) {
				return;
			}
			m_Inited=true;
			//
			int i=0,imax=m_Sprites.Length;
			m_SpriteMap=new Dictionary<string, StringSpritePair>(imax);
			for(;i<imax;++i) {
				if(!string.IsNullOrEmpty(m_Sprites[i].key)) {
					m_SpriteMap.Add(m_Sprites[i].key,m_Sprites[i]);
				}
			}
		}

		public virtual Sprite GetSprite(int index) {
			if(!m_Inited) {
				InitAtlas();
			}
			//
			if(index>=0&&index<m_Sprites.Length) {
				return m_Sprites[index].value;
			}else {
				return null;
			}
		}

		public virtual Sprite GetSprite(string key) {
			if(!m_Inited) {
				InitAtlas();
			}
			//
			StringSpritePair sprite;
			if(!string.IsNullOrEmpty(key)&&m_SpriteMap.TryGetValue(key,out sprite)) {
				return sprite.value;
			}else {
				return null;
			}
		}


		#endregion Methods

	}
}