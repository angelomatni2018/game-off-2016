using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Experiment : MonoBehaviour {

	// An experiment is defined by its color, its size, its speed, and its shape
	// An experiment has an age, ... 
	// An experiment has genes, and each gene is made up of x bases

	[HideInInspector]
	public float sWidth, sHeight;
	public float speed;
	public DNA dna;
	public int id;
	Society society;

	int expGroup;

	// 0 for alone, 1 for stable group, 2 for moving, 3 for interacting with other experiment
	public enum ExpState { Grouped, Moving, Attacking, Showing, Editing, Pending, Death };
	ExpState state, preJoin;
	public enum AnimState { Neutral, Angry };
	public enum Direction { None = -1, Up, Right, Down, Left };
	AnimState aState;
	Direction dir;

	Collider2D hitCol;
	[HideInInspector]
	public Rigidbody2D hitRb;
	SpriteRenderer sRend;
	Animator anim;
	SpriteRenderer animSRend;
	int numOverlap;

	AIAction currentAct;

	public void BaseSetup() {
		society =  ((GameObject)GameObject.Find("ExperimentHolder")).GetComponent<Society> ();
		dna = GetComponentInChildren<DNA>();
		hitRb = GetComponent<Rigidbody2D> ();

		// Messy way of getting collider used by rigidbody
		Transform child;
		for (int i = 0; i < transform.childCount; i++) {
			child = transform.GetChild (i);
			if (!child.tag.Equals ("Experiment"))
				continue;
			SetShapeFields(child.gameObject);
		}

		expGroup = -1;
		numOverlap = 0;
		aState = AnimState.Neutral;
		dir = Direction.Down;
	}

	/*void Setup (GameObject oldExp = null) {
		BaseSetup ();
		if (oldExp != null) {
			transform.name = sRend.name.Substring (0, sRend.name.Length - 7) + id.ToString ();
			id = oldExp.GetComponent<Experiment> ().id;
		} else {
			id = -1;
		}
	}*/

	internal void Update () {
		if (currentAct != null && !currentAct.Update ())
			currentAct = null;
		
		switch (state) {
		case ExpState.Moving:
			//print ("I am : " + expGroup);
			aState = AnimState.Neutral;
			if (expGroup == Society.JOINING_GROUP_ID) {
				currentAct = new TeleportToAction (this, society.GetGroupStats (expGroup).GetExpSpot (this));
				SetState (ExpState.Editing);
				break;
			}
			if (expGroup == Society.ROGUE_GROUP_ID)
				currentAct = new MoveToGroupAction (this, society.AvoidGroups (this));
			else
				currentAct = new MoveToGroupAction (this, society.GetGroupStats (expGroup).GetExpSpot (this));
			SetState(ExpState.Pending);
			break;
		case ExpState.Attacking:
			aState = AnimState.Angry;
			if (expGroup < Society.NUM_EXTRA_GROUPS)
				currentAct = new AttackGroupAction (this, society.COMOfGroup (expGroup));
			else
				currentAct = new AttackGroupAction (this, society.GetGroupStats (expGroup).center);
			SetState(ExpState.Pending);
			break;
		case ExpState.Death:
			DestroySelf ();
			break;
		}

		UpdateAnimator ();
	}

	void UpdateAnimator() {
		if (anim == null) {
			return;
		}
		
		if (numOverlap > 0 && sRend.color.a == 1) {
			ChangeOpacity (sRend, .5f);
			ChangeOpacity (animSRend, .5f);
		} else if (numOverlap == 0 && sRend.color.a != 1) {
			ChangeOpacity (sRend, 1);
			ChangeOpacity (animSRend, 1);
		}
		//if (id == -1) 
		//	print (name + "  " + aState + "  " + dir);
		anim.SetInteger ("state", (int)aState);
		anim.SetInteger ("dir", (int)dir);
	}

	void ChangeOpacity(SpriteRenderer r, float alpha) {
		r.color = new Color (r.color.r, r.color.g, r.color.b, alpha);
	}

	public void SetGroup(int num) {
		expGroup = num;
	}

	public int GetGroup() {
		return expGroup;
	}

	public Society GetSoc() {
		return society;
	}

	public void RemoveAct(ExpState newState) {
		if (currentAct != null) {
			society.DecInteractor ();
			currentAct = null;
			state = newState;
		}
	}

	public void SetState(ExpState es) {
		state = es;
		
		if (state == ExpState.Editing) {
			dna.MakeVisibleAndInteractive (true, true);
		} else if (state == ExpState.Showing) {
			dna.MakeVisibleAndInteractive (true, false);
		} else {
			dna.MakeVisibleAndInteractive (false, false);
		}
	}

	public ExpState GetState() {
		return state;
	}

	public void SetDir(Direction d) {
		dir = d;
	}

	public void ToggleHitCollider(bool active) {
		hitCol.enabled = active;
	}

	public void TurnTowards(Vector3 loc) {
		if (loc.x > 0) {
			dir = Experiment.Direction.Right;
		} else if (loc.x < 0) {
			dir = Experiment.Direction.Left;
		} else if (loc.y > 0) {
			dir = Experiment.Direction.Up;
		} else {
			dir = Experiment.Direction.Down;
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		//print ("Enter: " + col);
		if (col.tag.Equals ("Experiment")) {
			numOverlap++;
		}
	}

	void OnTriggerExit2D(Collider2D col) {
		//print ("Exit: " + col);
		if (col.tag.Equals ("Experiment") && numOverlap > 0) {
			numOverlap--;
		}
	}

	void OnCollisionEnter2D(Collision2D col) {
		//print (col.collider);
		if (col.collider.tag.Equals("Experiment") && society.GetGroupStats(expGroup).InGroup(col.gameObject.GetComponent<Experiment>().id)) {
			society.GetGroupStats(expGroup).EndAttackIfPossible (id);
		}
	}

	public void DestroySelf() {
		society.RemoveExperiment (this);
		Destroy (gameObject);
	}

	public void ReplaceShapeObj(GameObject shape) {
		// Destroy previous shape object before instantiating new fields
		Destroy (sRend.gameObject);
		SetShapeFields (shape);
		anim.gameObject.transform.localPosition = new Vector3 ();
		transform.name = sRend.name.Substring (0, sRend.name.Length - 7) + id.ToString ();
	}

	void SetShapeFields(GameObject shape) {
		anim = shape.GetComponentInChildren<Animator> ();
		animSRend = anim.GetComponent<SpriteRenderer> ();
		sRend = shape.GetComponent<SpriteRenderer> ();
		sWidth = sRend.sprite.bounds.size.x * transform.localScale.x;
		sHeight = sRend.sprite.bounds.size.y * transform.localScale.y;
		foreach (Collider2D col in shape.GetComponents<Collider2D>()) {
			if (!col.isTrigger) {
				hitCol = col;
				break;
			}
		}
	}

	public void SetColor(Color c) {
		sRend.color = c;
	}

	void OnMouseDown() {
		print ("Num left: " + society.NumInteracting() + " Can interact: " + society.CanInteractWith(this));
		if (!society.CanInteractWith(this))
			return;
		if (state == ExpState.Showing) {
			society.RemoveFromSelected (this);
			Deselect ();
			return;
		}
		society.AddToSelected (this);
		preJoin = state;
		SetState (ExpState.Showing);
	}

	public void Deselect() {
		SetState (preJoin);
	}
}
