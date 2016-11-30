using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	Experiment scientist;
	public string[] levelNames;
	int levelIndex;

	// Use this for initialization
	public void Setup () {
		DontDestroyOnLoad (gameObject);
		gameObject.name = "LevelHolder";
		if (levelIndex.Equals (default(int))) {
			levelIndex = 0;
		} else {
			levelIndex++;

			scientist = GameObject.Find ("Scientist").GetComponent<Experiment>();
			scientist.BaseSetup ();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool LoadNextLevel() {
		if (levelNames.Length > levelIndex) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (levelNames [levelIndex]);
			return true;
		}
		return false;
	}
}
