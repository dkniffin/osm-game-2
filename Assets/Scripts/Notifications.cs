using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Notifications : MonoBehaviour {

	public GameObject notificationPrefab;

	private float currentYPosition = 0.0f;

	void OnEnable()
	{
		EventManager.OnGameEvent += Add;
	}


	void OnDisable()
	{
		EventManager.OnGameEvent -= Add;
	}

	void Add (string eventText, Vector2 eventLocation) {
		// Build a notification button, and set it's position
		GameObject notification = Instantiate(notificationPrefab) as GameObject;
		notification.transform.SetParent(this.transform, false);
		var pos = notification.transform.localPosition;
		notification.transform.localPosition = new Vector3(pos.x, currentYPosition, 0);
		currentYPosition -= notification.GetComponent<RectTransform>().rect.height;

		// Set the text for the notification
		Text notificationText = notification.GetComponentInChildren<Text>();
		notificationText.text = eventText;

		// Add an onClick listener for it
		Button b = notification.GetComponent<Button>();
		b.onClick.AddListener (delegate {
			MoveCameraTo (eventLocation);
//			notification.SetActive(false);
		});
	}

	private void MoveCameraTo(Vector2 location) {
		var newCameraPosition = new Vector3(location.x, location.y, Camera.main.transform.position.z);
		Camera.main.transform.position = newCameraPosition;
	}
}
