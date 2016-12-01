using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFramework : MonoBehaviour {

	public Vector4 societyBounds;
	public Vector4 goalBounds;
	public Vector4 inventoryBounds;
	public Vector4 nbInInventBounds;

	Society society;
	Narrator narrator;
	GameObject joinButton, unjoinButton, continueButton;
	GameObject bindings;

	bool finished;

	void Awake() {
		narrator = GameObject.Find ("Scientist").GetComponent<Narrator> ();
		society = ((GameObject)GameObject.Find("ExperimentHolder")).GetComponent<Society> ();
		joinButton = GameObject.Find ("JoinButton");
		unjoinButton = GameObject.Find ("UnjoinButton");
		continueButton = GameObject.Find ("ContinueButton");
		bindings = GameObject.Find ("BindingsHelp");
		bindings.SetActive (false);
		joinButton.SetActive (true);
		unjoinButton.SetActive (false);

		finished = false;
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

	public void ToggleBindings() {
		bindings.SetActive (!bindings.activeInHierarchy);
	}

	public void QuitLevel() {
		UnityEngine.SceneManagement.SceneManager.LoadScene ("mainmenu");
	}

	public void ReloadLevel() {
		UnityEngine.SceneManagement.SceneManager.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name);
	}

	public void ReportFinished() {
		continueButton.SetActive (true);
		continueButton.GetComponentInChildren<Text> ().text = "Next Level";
		finished = true;
		print ("Finished");
	}

	public void ToggleContinueButton(bool visible) {
		if (!finished) {
			continueButton.SetActive (visible);
		}
	}

	public void AdvanceNarrative() {
		if (finished) {
			GameObject.Find ("LevelHolder").GetComponent<GameControl> ().LoadNextLevel ();
		} else {
			narrator.NextAction ();
		}
	}
}
