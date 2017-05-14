//=============================================================================
//
// Copyright 2016 Ximmerse, LTD. All rights reserved.
//
//=============================================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Ximmerse.UI;

public class ShowImageOnClick:MonoBehaviour, IPointerClickHandler {
	public RawImage target;
	public Texture source;
	public float vibrationTime=.25f;
	[System.NonSerialized]public Color m_Color=Color.white;
	
	public void OnPointerClick(PointerEventData eventData) {
		if(target!=null) {
			if(source==null) {
				RawImage rawImage=GetComponent<RawImage>();
				source=rawImage.texture;
				m_Color=rawImage.color;
			}
			target.texture=source;
			target.color=m_Color;
			VRInputModule.TriggerVibration(eventData,vibrationTime);
		}
	}
}
