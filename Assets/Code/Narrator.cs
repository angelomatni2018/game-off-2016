using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Narrator : MonoBehaviour {

	public float TEXT_PRINT_RATE;
	public float TEXT_WAIT_TIME;

	Text narratorText;
	GameObject textOptionsHolder;

	List<NarratorTrigger> nts;

	NarratorTrigger activeMessager;
	char[] currentMessage;
	int messageIndex;
	float timer;

	void Start () {
		narratorText = GameObject.Find ("NarratorText").GetComponent<Text>();
		textOptionsHolder = GameObject.Find ("TextOptionsHolder");
		for (int i = 0; i < textOptionsHolder.transform.childCount; i++) {
			nts.Add (textOptionsHolder.transform.GetChild (i).GetComponent<NarratorTrigger> ());
			if (nts [nts.Count - 1].CanTriggerOnStart ()) {
				BeginMessaging (nts[nts.Count - 1]);
			}
		}
	}

	public void Complete() {
		// Will run checks for onCompleted NarratorTriggers
	}

	void Update() {
		if (!activeMessager.Equals (default(NarratorTrigger))) {
			foreach (NarratorTrigger nt in nts) {
				if (nt.CanTrigger ()) {
					BeginMessaging (nt);
				}
			}
		} else if (messageIndex < 0 && (timer += Time.deltaTime) < TEXT_WAIT_TIME) {
			timer = 0;
			messageIndex = 0;
		} else if ((timer += Time.deltaTime) < TEXT_PRINT_RATE) {
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
	}

	void EndMessaging() {
		activeMessager = default(NarratorTrigger);
		narratorText.text = "";
	}
}
