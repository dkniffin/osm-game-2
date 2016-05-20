using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameObjectManager : Singleton<GameObjectManager> {
	private List<Building> buildings = new List<Building>();

	public void AddBuilding(Building b) {
		buildings.Add (b);
	}

	public List<Building> GetBuildings () {
		return buildings;
	}

	public Building GetRandomBuilding() {
		var i = UnityEngine.Random.Range (0, (buildings.Count - 1));
		Building b = buildings [i];
		return b;
	}
}
	