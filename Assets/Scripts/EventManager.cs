using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour {

	public delegate void GameEvent(string eventText);
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
				OnGameEvent ("A fire has started!");
			if (Random.value < crimeChance)
				OnGameEvent ("There was a break in!");
			if (Random.value < medicalChance)
				OnGameEvent ("There was a car accident!");
		}
	}
}
