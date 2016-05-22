using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Notifications : MonoBehaviour {

	public GameObject notificationPrefab;

	private float currentYPosition = 0.0f;

	private List<GameObject> notifications = new List<GameObject>();

	void OnEnable()  {
		EventManager.OnGameEvent += Add;
	}


	void OnDisable() {
		EventManager.OnGameEvent -= Add;
	}

	void Add (string eventText, Vector2 eventLocation) {
		// Build a notification button, and set it's position
		GameObject notification = Instantiate(notificationPrefab) as GameObject;
		notification.transform.SetParent(this.transform, false);
		SetPosition (notification);

		// Set the text for the notification
		Text notificationText = notification.GetComponentInChildren<Text>();
		notificationText.text = eventText;

		// Add it to the queue
		notifications.Add(notification);

		// Add an onClick listener for it
		Button b = notification.GetComponent<Button>();
		b.onClick.AddListener (delegate {
			Camera.main.GetComponent<MainCamera> ().PanTo (eventLocation);
			RemoveNotification(notification);
		});
	}

	private void RemoveNotification(GameObject notification) {
		notification.SetActive(false);
		notifications.Remove (notification);
		UpdatePositions ();
	}

	private void UpdatePositions() {
		currentYPosition = 0.0f;
		foreach (GameObject n in notifications) {
			SetPosition (n);
		}
	}

	private void SetPosition(GameObject notification) {
		var pos = notification.transform.localPosition;
		notification.transform.localPosition = new Vector3(pos.x, currentYPosition, 0);
		currentYPosition -= notification.GetComponent<RectTransform>().rect.height;
	}
}
