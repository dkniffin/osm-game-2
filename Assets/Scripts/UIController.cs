using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour {

	private bool isEnabled = true;
	private float guiBackgroundWidth = 100;
	private float guiBackgroundHeight = 100;
	private float xPos, yPos;

	public void CreatePopUpMenu(GameObject building) {
		Vector2 screenPos = Camera.main.WorldToScreenPoint (building.transform.position);
		xPos = screenPos.x - guiBackgroundWidth / 2;
		yPos = Screen.height - screenPos.y;
		GUI.Box(new Rect(xPos, yPos, guiBackgroundWidth, guiBackgroundHeight), gameObject.tag.ToString());
 	}

	public bool GUIEnabled() {
		return isEnabled;
	}

	public void EnableGUI() {
		isEnabled = true;
	}

	public void DisableGUI() {
		isEnabled = false;
	}

	public void ToggleGUIElement() {
		if (GUIEnabled()) {
			DisableGUI();
		} else {
			EnableGUI();
		}
	}
}
