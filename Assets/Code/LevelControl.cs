using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelControl : MonoBehaviour {

	void Awake() {
		GameObject UI = GameObject.Instantiate (Resources.Load ("LabCanvas")) as GameObject;
		UI.name = "LabCanvas";
		UI.GetComponent<Canvas> ().worldCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera>();
		GameObject level = GameObject.Find ("LevelHolder");
		if (level != null) {
			GameObject.Find ("GroupConditionText").GetComponent<Text> ().text = level.GetComponent<GameControl> ().groupConditions;
			GameObject.Find ("AttackConditionText").GetComponent<Text> ().text = level.GetComponent<GameControl> ().attackConditions;
		}

	}
}
