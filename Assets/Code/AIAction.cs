using UnityEngine;
using System.Collections;

public class AIAction {

	protected Experiment e;
	protected Vector3 targetPos;

	public AIAction(Experiment exp, Vector3 target) {
		e = exp;
		targetPos = target;
	}

	public virtual bool Update() {
		return false;
	}

	protected virtual bool End() {
		return false;
	}
}
