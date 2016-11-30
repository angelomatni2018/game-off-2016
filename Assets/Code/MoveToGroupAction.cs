using UnityEngine;
using System.Collections;

public class MoveToGroupAction : AIAction {

	protected float TOL = .01f;

	// The max zig-zagging distance in each direction an experiment should make
	protected Vector2 zzStretch;

	// The next place to move to
	protected Vector2 stepTarget;
	protected float distToStep;
	protected Vector2 direction;

	public MoveToGroupAction(Experiment e, Vector3 target) : base(e, target) {
		zzStretch = new Vector2 (e.sWidth, e.sHeight);
		stepTarget = e.transform.position;
		distToStep = 0;
		//Debug.Log ("Base info for " + e.id + ": " + targetPos + "  " + e.transform.position + "  " + zzStretch);
	}

	public override bool Update() {
		Vector3 currentPos = e.transform.position;
		if (Vector3.Distance (currentPos, targetPos) < TOL)
			return End ();
		if (distToStep < TOL) {
			Vector3 distLeft = targetPos - currentPos;
			if (distLeft.magnitude < TOL)
				return End ();
			int axis = Mathf.Abs(distLeft.y) > Mathf.Abs(distLeft.x) ? 1 : 0;

			// Calculate next stepTarget and distToStep
			distToStep = Mathf.Min(distLeft [axis],zzStretch [axis]); 
			//Debug.Log ("Dir: " + (axis ^ 1) + " " + (axis ^ 0) + " Next step: " + new Vector3 (distToStep * (axis ^ 1), distToStep * (axis ^ 0), 0));
			stepTarget = new Vector3 (distToStep * (axis ^ 1), distToStep * (axis ^ 0), 0);
			e.TurnTowards (stepTarget);
			distToStep = Mathf.Abs (distToStep);
		} else {
			float dist = Mathf.Min(e.speed * Time.deltaTime, distToStep);
			//Debug.Log ("Dist: " + dist + " Old: " + e.transform.position);
			e.transform.Translate (dist * stepTarget.normalized);
			//Debug.Log ("New : " + e.transform.position);
			distToStep -= dist;
		}
		return true;
	}

	protected override bool End() {
		e.GetSoc().DecInteractor();
		e.SetDir (Experiment.Direction.Down);
		//Debug.Log (e.name + " num left: " + e.GetSoc ().NumInteracting ());
		e.SetState (Experiment.ExpState.Grouped);
		return false;
	}
}
