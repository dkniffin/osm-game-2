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
		var i = notifications.IndexOf (notification);
		notification.SetActive(false);
		notifications.RemoveAt (i);
		UpdatePositions ();
	}

	private void UpdatePositions () {
		currentYPosition = 0.0f;
		foreach (GameObject n in notifications) {
			SetPosition (n);
		}
	}

	/*
	 * This *almost* works. There's some issues with overlapping buttons, after a few notifications are clicked.
	private void UpdatePositionsSlide(int indexOfRemoval) {
		var numberToMove = (notifications.Count - indexOfRemoval);
		var notificationsToBeMoved = notifications.GetRange (indexOfRemoval, numberToMove);

		var buttonHeight = notificationPrefab.GetComponent<RectTransform> ().rect.height;
		currentYPosition = 0.0f - (indexOfRemoval * buttonHeight);

		foreach (GameObject n in notificationsToBeMoved) {
			StartCoroutine(SlideNotification(n, currentYPosition));
			currentYPosition -= buttonHeight;
		}
	}

	private IEnumerator SlideNotification (GameObject notification, float targetY)
	{
		var smoothing = 3.0f;
		while(Mathf.Abs(notification.transform.localPosition.y - targetY) > 0.01f) {
			var y = Mathf.Lerp (notification.transform.localPosition.y, targetY, smoothing * Time.deltaTime);
			notification.transform.localPosition = new Vector3 (notification.transform.localPosition.x, y, notification.transform.localPosition.z);

			yield return null;
		}
		StopCoroutine ("SlideNotification");
	}
	*/

	private void SetPosition(GameObject notification) {
		var pos = notification.transform.localPosition;
		notification.transform.localPosition = new Vector3(pos.x, currentYPosition, 0);
		currentYPosition -= notification.GetComponent<RectTransform>().rect.height;
	}
}
