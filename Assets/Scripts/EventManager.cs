using UnityEngine;
using System.Collections;
using OSM;

public class EventManager : MonoBehaviour {
	public delegate void GameEvent(string eventText, Vector2 eventLocation);
	public static event GameEvent OnGameEvent;

	private float fireChance = 0.9f;
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
				OnGameEvent ("There was a break in!", new Vector2(0,0));
			if (Random.value < medicalChance)
				OnGameEvent ("There was a car accident!", new Vector2(0,0));
		}
	}

	void RandomFire () {
		Building b = GameObjectManager.Instance.GetRandomBuilding ();

		string building_name = b.way.GetTag ("name");
		var eventText = (building_name != "") ? "A fire has started in " + building_name + "!" : "A fire has started!";

		OnGameEvent(eventText, b.way.GetPosition());
	}
}
