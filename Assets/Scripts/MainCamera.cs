using UnityEngine;
using System.Collections;

public class MainCamera : MonoBehaviour {
	public float mapX = 100.0f;
	public float  mapY = 100.0f;
	private float scrollSpeed = 10f;
	public float minX;
	public float minY;
	public float maxX;
	public float maxY;
	public float dragSpeed = 100.0f;
	private GameObject mainCamera;
	private Transform target;
	private Vector3 curPosition;
	private bool canMove = true;
	private float vertExtent;
	private float horzExtent;
	private float cameraDistance;
	public float cameraDistanceMin = 40f;
	public float cameraDistanceMax = 100f;


	// Use this for initialization
	void Start () {
	  mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
	  vertExtent = mainCamera.GetComponent<Camera>().orthographicSize;    
	  horzExtent = vertExtent * Screen.width / Screen.height;

	  // Calculations assume map is position at the origin
	  minX = horzExtent - mapX / 2.0f;
	  maxX = mapX / 2.0f - horzExtent;
	  minY = vertExtent - mapY / 2.0f;
	  maxY = mapY / 2.0f - vertExtent;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (AbleToMoveCamera()) {
			MoveCamera();
		}
	}

	public bool AbleToMoveCamera() {
		return canMove;
	}

	public void DisableCameraMovement() {
		canMove = false;
	}

	public void EnableCameraMovement(){
		canMove = true;
	}
		
	public void PanTo(Vector2 target) {
		StartCoroutine("HandlePanning", target);
	}


	private IEnumerator HandlePanning (Vector2 target)
	{
		var smoothing = 3.0f;
		while(Vector2.Distance(transform.position, target) > 0.05f) {
			var pos = Vector2.Lerp(transform.position, target, smoothing * Time.deltaTime);
			transform.position = new Vector3 (pos.x, pos.y, transform.position.z);

			yield return null;
		}
		StopCoroutine ("HandlePanning");
	}

	private void MoveCamera() {
		if (Input.anyKey) {
			StopCoroutine ("HandlePanning");
		}

		// Zoom Out
		if (Input.GetAxis("Mouse ScrollWheel") > 0) {
			if (Camera.main.fieldOfView < cameraDistanceMax) {
				Camera.main.fieldOfView += scrollSpeed;
			}
		}
			
		// Zoom In
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			if (Camera.main.fieldOfView > cameraDistanceMin) {
				Camera.main.fieldOfView -= scrollSpeed;
			}
		}

		// Pan Up
		if (Input.GetKey(KeyCode.W)) {
			if (maxY < mainCamera.transform.position.y) {
				mainCamera.transform.position += new Vector3 (0f, Time.deltaTime * dragSpeed, 0f);
			}
		}
		// Pan Left
		if (Input.GetKey(KeyCode.A)) {
			if (minX < mainCamera.transform.position.x) {
				mainCamera.transform.position += new Vector3 (-Time.deltaTime * dragSpeed, 0f, 0f);
			}
		}
		// Pan Down
		if (Input.GetKey(KeyCode.S)) {
			if (minY < mainCamera.transform.position.y) {
				mainCamera.transform.position += new Vector3 (0f, -Time.deltaTime * dragSpeed, 0f);
			}
		}
		// Pan Right
		if (Input.GetKey(KeyCode.D)) {
			if (maxX < mainCamera.transform.position.x) {
				mainCamera.transform.position += new Vector3 (Time.deltaTime * dragSpeed, 0f, 0f);
			}
		}
	}
}