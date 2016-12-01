using UnityEngine;
using System.Collections;

public class TeleportToAction : AIAction {

	public TeleportToAction(Experiment e, Vector3 target) : base(e, target) {
		e.transform.position = target;
		e.dna.CalibrateNucleobasesForJoin ();
	}

	public override bool Update() {
		// For use when a button triggers the end of a join, not a keyboard click
		//if (e.GetState () != Experiment.ExpState.Editing)
		//Debug.Log ("Input: " + Input.GetKeyDown(KeyCode.D) + "  ID: " + e.id);
		if (e.GetState () == Experiment.ExpState.Pending) {
			return End ();
		}
		return true;
	}

	protected override bool End() {
		e.GetSoc().DecInteractor();
		return false;
	}
}
