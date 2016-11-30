using UnityEngine;
using System.Collections;

public class MenuControl : MonoBehaviour {

	public GameObject[] menus;
	int menuIndex;

	void Start() {
		menuIndex = 0;
		SwitchMenu (menuIndex);
	}

	public void SwitchMenu(int index) {
		for (int i = 0; i < menus.Length; i++) {
			menus [i].SetActive (index == i);
		}
	}

	public void LoadLevel(GameObject controlObj) {
		GameControl gc = ((GameObject)GameObject.Instantiate (controlObj)).GetComponent<GameControl> ();
		gc.Setup ();
		if (!gc.LoadNextLevel ()) {
			// Since level didn't load, destroy gc:
			Destroy (gc.gameObject);
		}
	}
}
