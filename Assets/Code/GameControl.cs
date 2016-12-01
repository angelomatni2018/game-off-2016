using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	Experiment scientist;
	public string[] levelNames;
	public string groupConditions, attackConditions;

	int levelIndex;

	// Use this for initialization
	public void Setup () {
		DontDestroyOnLoad (gameObject);
		gameObject.name = "LevelHolder";
		levelIndex = -1;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool LoadNextLevel() {
		levelIndex++;
		if (levelNames.Length > levelIndex) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (levelNames [levelIndex]);
			return true;
		} else if (levelIndex > 0) {
			// Hacky way of saying you're playing a level and are at the end, so load the menu again
			UnityEngine.SceneManagement.SceneManager.LoadScene("mainmenu");
			return true;
		}
		levelIndex--;
		return false;
	}
}
