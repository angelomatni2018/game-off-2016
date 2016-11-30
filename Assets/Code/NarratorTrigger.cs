using UnityEngine;
using System.Collections;

public class NarratorTrigger : MonoBehaviour {

	// Fields to determine when to trigger the messages
	public Society society;
	public bool repeatable;
	public bool onSocietyStart;
	public bool onSocietyCompletion;
	public bool onFactoryExp;
	public Society.SocState socState;
	public Experiment.ExpState expState;
	public Experiment.AnimState expAnimState;

	// Messages the narrator will say
	public string[] messages;
	int messageSeek;

	// Internal fields to track states
	Experiment exp;
	bool triggered;

	void Setup () {
		messageSeek = 0;
		if (society.Equals(default(Society))) {
			society = ((GameObject)GameObject.Find("ExperimentHolder")).GetComponent<Society> ();
		}
		if (onFactoryExp) { 
			exp = society.GetExpWithId (-1);
		}
	}

	public bool CanTrigger() {
		if (!repeatable && triggered)
			return false;
		if (exp != null) {
			if (!EqualIfNotNull (expState, exp.GetState ()))
				return false;
			//if (!EqualIfNotNull (expAnimState, ...
		}
		if (!EqualIfNotNull (socState, society.GetState ()))
			return false;
		triggered = true;
		return true;
	}

	public bool EqualIfNotNull<T>(T triggerVal, T currentVal) {
		if (triggerVal.Equals (default(T)))
			return true;
		return triggerVal.Equals (currentVal);
	}

	public bool CanTriggerOnStart() {
		if (!onSocietyStart)
			return false;
		triggered = true;
		return true;
	}

	public bool CanTriggerOnCompletion() {
		if (!onSocietyCompletion)
			return false;
		triggered = true;
		return true;
	}

	public string GetNextMessage() {
		if (messageSeek < messages.Length) {
			return messages [messageSeek++];
		}
		return null;
	}
}
