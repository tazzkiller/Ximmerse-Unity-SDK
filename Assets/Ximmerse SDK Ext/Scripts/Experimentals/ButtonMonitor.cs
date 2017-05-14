using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ximmerse.InputSystem;

public class ButtonMonitor:MonoBehaviour {

	#region Nested Types

	[System.Serializable]
	public class ButtonUIPair:UKeyValuePair<ControllerButton,Selectable>,
		IPointerEnterHandler,
		IPointerExitHandler,
		IPointerClickHandler
	{
		public virtual void OnPointerEnter(PointerEventData eventData) {
			ExecuteEvents.Execute<IPointerEnterHandler>(
				value.gameObject,
				eventData,
				ExecuteEvents.pointerEnterHandler
			);
		}

		public virtual void OnPointerExit(PointerEventData eventData) {
			ExecuteEvents.Execute<IPointerExitHandler>(
				value.gameObject,
				eventData,
				ExecuteEvents.pointerExitHandler
			);
		}

		public virtual void OnPointerClick(PointerEventData eventData) {
			ExecuteEvents.Execute<IPointerClickHandler>(
				value.gameObject,
				eventData,
				ExecuteEvents.pointerClickHandler
			);
		}
	}

#if UNITY_EDITOR

	[UnityEditor.CustomPropertyDrawer(typeof(ButtonUIPair))]
	public class ButtonUIPairDrawer:UKeyValuePairDrawer<string,Sprite>{}

#endif

	#endregion Nested Types

	#region Fields

	[SerializeField]protected ControllerType m_Controller=ControllerType.LeftController;
	[System.NonSerialized]protected ControllerInput m_ControllerInput;
	[SerializeField]protected ButtonUIPair[] m_Buttons=new ButtonUIPair[0];
	[System.NonSerialized]protected PointerEventData m_EventData;

	#endregion Fields

	#region Unity Messages

	protected virtual void Start() {
		m_ControllerInput=ControllerInputManager.instance.GetControllerInput(m_Controller);
		m_EventData=new PointerEventData(EventSystem.current);
	}

	protected virtual void Update() {
		if(m_ControllerInput!=null) {
			for(int i=0,imax=m_Buttons.Length;i<imax;++i) {
				if(m_Buttons[i].value!=null){
					if(m_ControllerInput.GetButtonDown(m_Buttons[i].key)){
						m_Buttons[i].OnPointerEnter(m_EventData);
					}else if(m_ControllerInput.GetButtonUp(m_Buttons[i].key)){
						m_Buttons[i].OnPointerExit(m_EventData);
					}
					//
					if(m_ControllerInput.GetButtonDown(((uint)m_Buttons[i].key>>16))){
						m_Buttons[i].OnPointerClick(m_EventData);
					}
				}
			}
		}
	}

	#endregion Unity Messages

}
