﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Narrator : MonoBehaviour {

	public float START_WAIT;
	public float TEXT_PRINT_RATE;
	//public float TEXT_WAIT_TIME;

	public int[] scientistMapList;

	Text narratorText;
	GameObject textOptionsHolder;

	List<NarratorTrigger> nts;

	UIFramework UI;
	NarratorTrigger activeMessager;
	char[] currentMessage;
	int messageIndex;
	float timer;
	float levelTime;

	void Start () {
		levelTime = 0;
		activeMessager = null;
		UI = GameObject.Find ("LabCanvas").GetComponent<UIFramework> ();
		narratorText = GameObject.Find ("NarratorText").GetComponent<Text>();
		textOptionsHolder = GameObject.Find ("TextOptionsHolder");
		Experiment e = GetComponent<Experiment> ();
		e.BaseSetup ();
		e.dna.Setup (scientistMapList);
		e.dna.MakeVisibleAndInteractive (false, false);
		nts = new List<NarratorTrigger> ();
		for (int i = 0; i < textOptionsHolder.transform.childCount; i++) {
			nts.Add (textOptionsHolder.transform.GetChild (i).GetComponent<NarratorTrigger> ());
			nts [nts.Count - 1].Setup ();
			if (nts [nts.Count - 1].CanTriggerOnStart ()) {
				BeginMessaging (nts[nts.Count - 1]);
			}
		}
	}

	public void Complete() {
		// Will run checks for onCompleted NarratorTriggers
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Return)) {
			NextAction ();
		}

		levelTime += Time.deltaTime;
		if (levelTime < START_WAIT)
			return;
	
		if (activeMessager == null) {
			foreach (NarratorTrigger nt in nts) {
				if (nt.CanTrigger ()) {
					BeginMessaging (nt);
				}
			}
		} else if (messageIndex >= 0 && (timer += Time.deltaTime) < TEXT_PRINT_RATE) {
			timer = 0;
			if (messageIndex < currentMessage.Length) {
				narratorText.text += currentMessage [messageIndex++];
			} else if ((currentMessage = activeMessager.GetNextMessage ().ToCharArray ()).Length > 0) {
				messageIndex = -1;
			} else {
				EndMessaging ();
			}
		}
	}

	void BeginMessaging(NarratorTrigger nt) {
		activeMessager = nt;
		timer = 0;
		messageIndex = 0;
		narratorText.text = "";
		currentMessage = nt.GetNextMessage ().ToCharArray();
		UI.ToggleContinueButton (true);
	}

	void EndMessaging() {
		UI.ToggleContinueButton (false);
		activeMessager = null;
		//narratorText.text = "";
	}

	public void NextAction() {
		if (messageIndex == -1) {
			narratorText.text = "";
			messageIndex = 0;
		}
	}
}