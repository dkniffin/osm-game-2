using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Notifications : MonoBehaviour {

	public GameObject notificationPrefab;

	private float currentYPosition = 0.0f;

	void OnEnable()
	{
		EventManager.OnGameEvent += UpdateNotifications;
	}


	void OnDisable()
	{
		EventManager.OnGameEvent -= UpdateNotifications;
	}

	void UpdateNotifications (string eventText) {
		GameObject notification = Instantiate(notificationPrefab) as GameObject;
		notification.transform.SetParent(this.transform, false);
		var pos = notification.transform.localPosition;
		notification.transform.localPosition = new Vector3(pos.x, currentYPosition, 0);


		Text notificationText = notification.GetComponent<Text> ();
		notificationText.text = eventText;

		currentYPosition -= notificationText.preferredHeight + 5;
	}
}
