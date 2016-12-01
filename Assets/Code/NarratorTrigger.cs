using UnityEngine;
using System.Collections;

public class NarratorTrigger : MonoBehaviour {

	// Fields to determine when to trigger the messages
	public Society society;
	public bool repeatable;
	public bool onSocietyStart;
	public bool onSocietyCompletion;
	public bool onFactoryExp;
	public bool onSameGenes;
	public int[] expGeneMap;
	public bool onFilledGenes;
	public bool onNumExps;
	public bool onAttack;
	public int numExpsInSociety;
	public Society.SocState socState;
	public bool checkSoc;
	public Experiment.ExpState expState;
	public bool checkExp;
	public Experiment.AnimState expAnimState;
	public bool checkAnim;

	// Messages the narrator will say
	public string[] messages;
	int messageSeek;

	// Internal fields to track states
	Experiment exp;
	bool triggered;

	public void Setup () {
		messageSeek = 0;
		if (society == null) {
			society = ((GameObject)GameObject.Find("ExperimentHolder")).GetComponent<Society> ();
		}
	}

	public bool CanTrigger() {
		if (!repeatable && triggered)
			return false;
		if (onNumExps && numExpsInSociety != society.GetNumOfExps ()) {
			return false;
		}
		if (exp != null) {
			if (!EqualIfNotNull (expState, exp.GetState (), checkExp))
				return false;
			//if (!EqualIfNotNull (expAnimState, ...
		}
		if (!EqualIfNotNull (socState, society.GetState (), checkSoc)) {
			return false;
		}
		print(onAttack + "  " + (!society.ExpsAreAttackingEachOther ()));
		if (onAttack && !society.ExpsAreAttackingEachOther ())
			return false;
		if (onFactoryExp && onFilledGenes && !society.GetExpWithId(-1).dna.HasFilledGenes ()) {
			return false;
		}
		if (onSameGenes && !society.GetExpWithId(-1).dna.SameTypeOfExp (expGeneMap)) {
			return false;
		}
		
		if (onSocietyCompletion && !society.GetState ().Equals (Society.SocState.Finished)) {
			return false;
		}
		triggered = true;
		return true;
	}

	public bool EqualIfNotNull<T>(T triggerVal, T currentVal, bool check) {
		if (!check || triggerVal.Equals (default(T)))
			return true;
		return triggerVal.Equals (currentVal);
	}

	public bool CanTriggerOnStart() {
		if (!onSocietyStart)
			return false;
		triggered = true;
		return true;
	}

	public string GetNextMessage() {
		if (messageSeek < messages.Length) {
			return messages [messageSeek++];
		}
		return "";
	}
}
