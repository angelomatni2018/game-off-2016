using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackCondition : MonoBehaviour {

	// Maximum groups allowed to form before they attack
	public int maxGroupsAllowed;
	// Whether rogues attack each other
	public bool roguesStruggle;

	public bool internalStruggle;

	Society society;
	public void Awake () {
		society = GameObject.Find("ExperimentHolder").GetComponent<Society>();
	}

	public List<Experiment> ConditionMet(List<Experiment> allExps) {
		List<Experiment> exps = new List<Experiment> ();
		bool groupFight = society.GetNumOfGroups () > maxGroupsAllowed;
		foreach (Experiment e in allExps) {
			if (e.GetGroup () >= Society.NUM_EXTRA_GROUPS && groupFight ||
				e.GetGroup () == Society.ROGUE_GROUP_ID && roguesStruggle) {
				exps.Add (e);
				if (!internalStruggle)
					e.SetGroup (Society.GENERAL_ATTACK_GROUP_ID);
			}
		}
		return exps;
	}
}
