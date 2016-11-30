using UnityEngine;
using System.Collections;

public class UIFramework : MonoBehaviour {

	public Vector4 societyBounds;
	public Vector4 goalBounds;
	public Vector4 inventoryBounds;
	public Vector4 nbInInventBounds;

	Society society;
	GameObject joinButton, unjoinButton;

	void Awake() {
		society = ((GameObject)GameObject.Find("ExperimentHolder")).GetComponent<Society> ();
		joinButton = GameObject.Find ("JoinButton");
		unjoinButton = GameObject.Find ("UnjoinButton");
		joinButton.SetActive (true);
		unjoinButton.SetActive (false);
	}

	public void SpawnFromFactory() {
		society.TriggerSpawn ();
	}

	public void JoinExperiments() {
		if (society.TriggerJoin ()) {
			joinButton.SetActive (false);
			unjoinButton.SetActive (true);
		}
	}

	public void FinishJoin() {
		if (society.TriggerEndJoin ()) {
			joinButton.SetActive (true);
			unjoinButton.SetActive (false);
		}
	}
}
