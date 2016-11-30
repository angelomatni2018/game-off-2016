using UnityEngine;
using System.Collections;

public class AttackGroupAction : MoveToGroupAction {

	Rigidbody2D rb;

	public AttackGroupAction(Experiment e, Vector3 target) : base(e, target) {
		//Debug.Log ("Starting attack: " + e.id +  "  " + target);
		rb = e.hitRb;
		e.ToggleHitCollider (true);
		rb.isKinematic = false;
	}

	public override bool Update() {
		Vector3 currentPos = e.transform.position;
		if (distToStep < TOL) {
			Vector3 distLeft = targetPos - currentPos;
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
			rb.MovePosition((Vector2)currentPos + dist * stepTarget.normalized);
			//Debug.Log ("New : " + e.transform.position);
			distToStep -= dist;
		}
		return true;
	}
}
