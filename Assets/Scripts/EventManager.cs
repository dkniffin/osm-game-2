using UnityEngine;
using System.Collections;
using OSM;

public class EventManager : MonoBehaviour {
	public delegate void GameEvent(string eventText, Vector2 eventLocation);
	public static event GameEvent OnGameEvent;

	private float fireChance = 0.1f;
	private float crimeChance = 0.1f;
	private float medicalChance = 0.1f;

	// Use this for initialization
	void Start () {
		InvokeRepeating ("GenerateEvents", 1.0f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
	}

	void GenerateEvents () {
		if (OnGameEvent != null) {
			if (Random.value < fireChance)
				RandomFire ();
			if (Random.value < crimeChance)
				RandomCrime ();
			if (Random.value < medicalChance)
				RandomMedical ();
		}
	}

	void RandomFire () {
		Building b = GameObjectManager.Instance.GetRandomBuilding ();

		string building_name = b.way.GetTag ("name");
		var eventText = (building_name != "") ? "A fire has started in " + building_name + "!" : "A fire has started!";

		var position = b.way.GetPosition ();
		DrawIcon((GameObject)Resources.Load ("Prefabs/fire"), position);
		OnGameEvent(eventText, position);
	}

	void RandomCrime() {
		var position = RandomPosition ();
		DrawIcon((GameObject)Resources.Load ("Prefabs/break-in"), position);
		OnGameEvent ("There was a break in!", position);
	}

	void RandomMedical() {
		var position = RandomPosition ();
		DrawIcon((GameObject)Resources.Load ("Prefabs/car-accident"), position);
		OnGameEvent ("There was a car accident!", position);
	}

	private Vector2 RandomPosition() {
		// TODO: Don't hardcode bounds
		return new Vector2(Random.Range(0.0f, 2000.0f), Random.Range(0.0f, 2000.0f));
	}

	private void DrawIcon(GameObject iconPrefab, Vector2 position) {
		Instantiate (iconPrefab, position, Quaternion.identity);
	}
}
