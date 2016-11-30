using UnityEngine;
using System.Collections;

public class LevelControl : MonoBehaviour {

	void Awake() {
		GameObject UI = GameObject.Instantiate (Resources.Load ("LabCanvas")) as GameObject;
		UI.name = "LabCanvas";
		UI.GetComponent<Canvas> ().worldCamera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera>();
	}
}
