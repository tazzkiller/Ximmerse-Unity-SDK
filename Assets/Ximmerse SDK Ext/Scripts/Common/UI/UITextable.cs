using UnityEngine;
using UnityEngine.UI;

namespace Ximmerse.UI {

	public class UITextable:MonoBehaviour {

		#region Fields

		[SerializeField]protected GameObject m_Go;
		[SerializeField]protected Text m_Text;
		[SerializeField]protected UIAtlas m_UIAtlas;
		[SerializeField]protected Image m_Image;

		#endregion Fields

		#region Methods

		public virtual void Display(string value) {
			if(string.IsNullOrEmpty(value)) {
				SetActive(false);
				return;
			}else {
				SetActive(true);
			}
			//
			if(value.StartsWith("@Image:")) {
				SetText(null);
				SetImage(value.Substring(7));
			}else {
				SetText(value);
				SetImage(null);
			}
		}

		public virtual void SetActive(bool value) {
			if(m_Go==null) {
				m_Go=gameObject;
			}
			//
			m_Go.SetActive(value);
		}

		public virtual void SetText(string value) {
			if(m_Text!=null) {
				m_Text.text=value;
			}
		}

		public virtual void SetImage(string value) {
			if(m_Image!=null) {
				Sprite sprite=null;
				if(m_UIAtlas) {
					sprite=m_UIAtlas.GetSprite(value);
				}
				//
				m_Image.enabled=(sprite!=null);
				m_Image.sprite=sprite;
			}
		}

		#endregion Methods

	}

}
