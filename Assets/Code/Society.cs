using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Society : MonoBehaviour {

	[HideInInspector]
	public static int ROGUE_GROUP_ID = 1, GENERAL_ATTACK_GROUP_ID = 0, JOINING_GROUP_ID = 2, NUM_EXTRA_GROUPS = 3;
	// Spawn in experiments in interactive area
	// Dictate groups experiments will be in
	// Dictate actinos based on groupings to them
	public float GROUP_RADIUS;
	static float EXP_RADIUS = 2f;
	public int NUM_BASE_TYPES;
	public int NUM_BASES_PER_GENE;
	public int MAX_EXP_JOIN;

	public UIFramework UI;
	GameObject SocStatus;
	Vector3 societyCenter;

	public string[] geneMap;

	// A reduced 2 dimensional array of experiment # and gene # 
	[System.Serializable]
	public struct geneMapSet {
		public int[] mapList;
	}
	public geneMapSet[] geneMapDefaultExps;
	public geneMapSet[] goalGeneMapping;

	// An array of values for nucleobases to add to the pool at starttime
	public int[] looseNucleobaseDefaults;

	[HideInInspector]
	public GroupCondition[] gConditions;
	[HideInInspector]
	public AttackCondition[] aConditions;
	Transform expHolder;
	Transform factoryHolder;
	Transform goalHolder;
	Transform tempExp;

	//Used in determining centroids for groups
	GameObject node, groupMarker;
	Vector3[] rogueSpots;
	bool[] takenRogueSpot;

	// Sorted list of experiments (based on position in sand box)
	List<Experiment> experiments;
	List<Experiment> mouseSelectedExps;
	Experiment factoryExp;

	// State is 1 for starting interactions, 2 for pending interactions, 3 for stable
	public enum SocState { Sleeping, Waiting, Regrouping, Attacking, Joining, CheckVictory, Finished };
	SocState state, nextState;
	int numInteracting;

	GroupStats[] groupStats;
	int numberOfGroups;

	// Use this for initialization
	void Awake () {
		if (geneMap.Length == 0)
			geneMap = new string[2] { "shape", "color" };
		experiments = new List<Experiment> ();
		mouseSelectedExps = new List<Experiment> ();
		gConditions = GetComponents<GroupCondition> ();
		aConditions = GetComponents<AttackCondition> ();
		GeneValues.NUM_BASE_TYPES = NUM_BASE_TYPES;
		expHolder = ((GameObject)GameObject.Find("ExperimentHolder")).transform;
		factoryHolder = ((GameObject)GameObject.Find ("FactoryHolder")).transform;
		goalHolder = ((GameObject)GameObject.Find ("GoalHolder")).transform;
		tempExp = ((GameObject)Resources.Load ("temp_experiment")).transform;
		foreach (GroupCondition gc in GetComponents<GroupCondition>()) {
			gc.Setup ();
		}
	}

	void Start() {			
		// Rogue node setup
		GameObject rogueNode = (GameObject)Resources.Load ("nodes/roguenode");
		rogueSpots = new Vector3[rogueNode.transform.childCount];
		for (int i = 0; i < rogueNode.transform.childCount; i++) {
			rogueSpots [i] = rogueNode.transform.GetChild (i).position;
		}
		groupMarker = Resources.Load ("nodes/groupmarker") as GameObject;

		// UI Setup
		UI = GameObject.Find ("LabCanvas").GetComponent<UIFramework> ();
		SocStatus = GameObject.Find ("SocietyStatus");
		SocStatus.SetActive (false);
		societyCenter = new Vector3 (UI.societyBounds.x + UI.societyBounds.z / 2, UI.societyBounds.y + UI.societyBounds.w / 2, 0);

		// GameObject Setup
		for (int i = 0; i < geneMapDefaultExps.Length; i++) {
			Experiment e = CreateExperiment (expHolder, i, NewBoundedRandomLoc());
			e.dna.Setup (geneMapDefaultExps[i].mapList);
		}
		SpawnGoalExperiments ();
		AddFactoryExperiment ();
		AddNucleobases ();

		// Interaction Setup		
		for (int i = 0; i < expHolder.childCount; i++) {
			Experiment e = expHolder.GetChild (i).GetComponent<Experiment> ();
			// Check if this is a valid experiment
			if (e.gameObject.activeInHierarchy && !e.gameObject.name.Equals("ToBeDestroyed"))
				experiments.Add(e);
		}
		state = SocState.Regrouping;
	}

	Experiment CreateExperiment(Transform holder, int id, Vector3 pos) {
		Experiment exp = ((GameObject)GameObject.Instantiate (Resources.Load ("temp_experiment"),holder)).GetComponent<Experiment>();
		exp.BaseSetup ();
		exp.id = id;
		exp.transform.position = pos;
		return exp;
	}

	// Update is called once per frame
	void Update () {
		//print (numInteracting);
		switch (state) {
		case SocState.CheckVictory:
			ActivateStatusUIWith (false, "");
			CheckForVictory ();
			break;
		case SocState.Waiting:
			if (numInteracting == 0) {
				state = nextState;
			}
			break;
		case SocState.Regrouping:
			BucketExpsIntoGroups ();
			foreach (Experiment e in experiments) {
				e.SetState (Experiment.ExpState.Moving);
			}
			ActivateStatusUIWith (true, "Grouping");
			state = SocState.Waiting;
			nextState = SocState.Attacking;
			DrawGroupMarkers ();
			break;
		case SocState.Attacking:
			ActivateStatusUIWith (true, "Attacking");
			UpdateExpAttacks ();
			state = SocState.Waiting;
			nextState = SocState.CheckVictory;
			break;
		case SocState.Joining:
			EraseGroupMarkers ();
			JoinSelectedExps ();
			state = SocState.Waiting;
			nextState = SocState.Regrouping;
			break;
		}
	}
		
	void ActivateStatusUIWith(bool active, string text) {
		SocStatus.SetActive (active);
		SocStatus.GetComponentInChildren<Text> ().text = text;
	}

	void InsertExpIntoList(Experiment e) {
		//int index;
		//if ((index = experiments.BinarySearch (e, (Experiment x, Experiment y) => x.transform.position.x.CompareTo (y.transform.position.x))) < 0)
		//	index = 0;
		//experiments.Insert (index, e);
		//print ("Added: " + e.name);
	}

	public Experiment GetExpWithId(int id) {
		if (id < 0)
			return factoryExp;
		return experiments [id];
	}

	void JoinSelectedExps() {
		groupStats [JOINING_GROUP_ID] = new GroupStats (this);
		groupStats [JOINING_GROUP_ID].center = societyCenter;
		numInteracting = 0;
		foreach (Experiment e in mouseSelectedExps) {
			numInteracting++;
			e.SetState (Experiment.ExpState.Moving);
			e.SetGroup (JOINING_GROUP_ID);
			groupStats [JOINING_GROUP_ID].AddToGroup(e.id);
		}
		groupStats [JOINING_GROUP_ID].ResortExps ();
	}

	void UpdateExpAttacks() {
		List<Experiment> attackingExps = experiments;
		groupStats[GENERAL_ATTACK_GROUP_ID] = new GroupStats (this);
		foreach (AttackCondition ac in aConditions) {
			attackingExps = ac.ConditionMet (attackingExps);
		}
		if (attackingExps.Count > 1) {
			numInteracting = attackingExps.Count;
			foreach (Experiment e in attackingExps) {
				if (e.id == GENERAL_ATTACK_GROUP_ID)
					groupStats [GENERAL_ATTACK_GROUP_ID].AddToGroup (e.id);
				else
					GetGroupStats (e.GetGroup ()).InitAttack ();		
				e.SetState (Experiment.ExpState.Attacking);
			}
			groupStats [GENERAL_ATTACK_GROUP_ID].InitAttack ();
		}
	}

	// It sorts experiments into groups, 
	// populating group stats with info on each group, and
	// setting the group value of each experiment accordingly.
	void BucketExpsIntoGroups() {
		numInteracting = experiments.Count;
		int[] numInGroup = new int[numInteracting];
		List<Experiment>[] expGroups = new List<Experiment>[numInteracting];
		GroupStats[] tempStats = new GroupStats[numInteracting];

		// Reset rogue spot bindings
		takenRogueSpot = new bool[rogueSpots.Length];

		// Figure what group each experiment goes in
		int allGroups = 0;
		numberOfGroups = 0;
		foreach (Experiment e in experiments) {
			if (numberOfGroups == allGroups) {
				numInGroup [allGroups++] = 0;
				tempStats [numberOfGroups] = new GroupStats (this);
				expGroups [numberOfGroups] = new List<Experiment> ();
			}
			for (int i = 0; i < allGroups; i++) {
				// Check if experiment fits in group and set index to that group if it worked
				//print("Checking: " + e.name + "  " + e.id);
				if (tempStats [i].CanAddToGroup (e)) {
					expGroups [i].Add (e);
					numInGroup [i]++;
					if (i == numberOfGroups) {
						numberOfGroups++;
					}
					break;
				}
			}
		}

		// Index starts at NUM_EXTRA_GROUPS, because extra groups are at the beginning
		numberOfGroups = NUM_EXTRA_GROUPS;
		groupStats = new GroupStats[experiments.Count + NUM_EXTRA_GROUPS];
		groupStats [ROGUE_GROUP_ID] = new GroupStats (this);
		for (int i = 0; i < allGroups; i++) {
			if (expGroups [i].Count > 1) {
				groupStats [numberOfGroups] = tempStats [i];
				groupStats [numberOfGroups].SetGroupSort(numInGroup [i]);
				foreach (Experiment e in expGroups[i]) {
					e.SetGroup (numberOfGroups);
					groupStats [numberOfGroups].AddToGroup(e.id);
				}	
				numberOfGroups++;
			} else if (numInGroup [i] > 0) {
				expGroups [i] [0].SetGroup (ROGUE_GROUP_ID);
				groupStats[ROGUE_GROUP_ID].AddToGroup(expGroups [i] [0].id);
			}
		}
		// print groups of experiments
		/*for (int i = 0; i < experiments.Count; i++) {
			print (experiments[i].name + "  " + experiments [i].GetGroup ());
		}*/

		// Calculate where each group's centroid is.
		Vector2[] centroids = new Vector2[numberOfGroups];
		int groupId;
		foreach (Experiment e in experiments) {
			groupId = e.GetGroup ();
			if (centroids[groupId].Equals(default(Vector2)))
				centroids[groupId] = new Vector2 ();
			centroids[groupId] += (Vector2)e.transform.position;
		}
		for (int i = 0; i < centroids.Length; i++) {
			if (centroids[i].Equals (default(Vector2)))
				continue;
			centroids [i] /= groupStats [i].numToSort;
			groupStats [i].center = centroids [i];
			//print (groupStats [i].center);
		}

		// Clip group centers to node points
		if (numberOfGroups > NUM_EXTRA_GROUPS) {
			node = (GameObject)Resources.Load ("nodes/node" + (numberOfGroups - NUM_EXTRA_GROUPS).ToString ());
			for (int i = 0; i < node.transform.childCount; i++) {
				Vector3 nodePos = node.transform.GetChild (i).transform.position;
				int closest = -1;
				float minDist = Mathf.Infinity, tempDist;
				for (int j = NUM_EXTRA_GROUPS; j < numberOfGroups; j++) {
					if (groupStats [j].nodeFound)
						continue;
					tempDist = Vector3.Distance (groupStats [j].center, nodePos);
					if (tempDist < minDist) {
						closest = j;
						minDist = tempDist;
					}
				}
				groupStats [closest].nodeFound = true;
				groupStats [closest].center = societyCenter + nodePos;
				groupStats [closest].ResortExps ();
			}
		}
	}

	public Vector2 AvoidGroups(Experiment e) {
		// Random selection (old method, but still might be used elsewhere)
		//System.Random rnd = new System.Random ();
		//foreach (int i in Enumerable.Range(0, rogueSpots.Length).OrderBy(r => rnd.Next())) {

		int spot = 0;
		float minDist = Mathf.Infinity;
		Vector3 bestSpot = e.transform.position;
		for (int i = 0; i < rogueSpots.Length; i++) {
			if (!takenRogueSpot[i] && Vector3.Distance(rogueSpots[i], e.transform.position) < minDist) {
				if (NearGroup (rogueSpots [i]))
					continue;
				bestSpot = rogueSpots[i];
				spot = i;
				minDist = Vector3.Distance (rogueSpots[i], e.transform.position);
			}
		}
		takenRogueSpot [spot] = true;
		return bestSpot;
	}

	bool NearGroup(Vector3 pos) {
		print (groupStats.Length);
		foreach (Experiment e in experiments) {
			if (e.GetGroup () != ROGUE_GROUP_ID)
				continue;
			if (Vector3.Distance (e.transform.position, pos) < EXP_RADIUS)
				return true;
		}
		for (int i = NUM_EXTRA_GROUPS; i < numberOfGroups; i++) {
			if (Vector3.Distance (groupStats [i].center, pos) < GROUP_RADIUS)
				return true;
		}
		return false;
	}

	void DrawGroupMarkers() {
		GameObject holder = new GameObject ();
		GameObject.Instantiate (holder);
		holder.name = "GroupMarkerHolder";
		for (int i = NUM_EXTRA_GROUPS; i < numberOfGroups; i++) {
			GameObject.Instantiate (groupMarker, groupStats [i].center, Quaternion.identity, holder.transform);
		}
	}

	void EraseGroupMarkers() {
		Destroy (GameObject.Find ("GroupMarkerHolder"));
	}

	public SocState GetState() {
		return state;
	}

	public GroupStats GetGroupStats(int index) {
		return groupStats [index];
	}

	public int GetNumOfGroups() {
		return numberOfGroups;
	}

	public int GetNumOfExps() {
		return experiments.Count;
	}

	public bool ExpsAreAttackingEachOther() {
		return groupStats[GENERAL_ATTACK_GROUP_ID] != null && groupStats [GENERAL_ATTACK_GROUP_ID].numToSort > 0;
	}

	public void DecInteractor() {
		numInteracting--;
	}

	public int NumInteracting() {
		return numInteracting;
	}

	public Vector3 COMOfGroup(int groupId) {
		Vector3 com = new Vector3(0,0,0);
		int num = 0;
		foreach (Experiment e in experiments) {
			if (e.GetGroup () == groupId) {
				com += e.transform.position;
				num++;
			}
		}
		return com / num;
	}

	public bool CanInteractWith(Experiment e) {
		return numInteracting == 0 && experiments.IndexOf (e) >= 0;
	}

	public void TriggerSpawn() {
		if (factoryExp.dna.HasFilledGenes()) {
			AddExperimentToWorld (factoryExp);
			AddFactoryExperiment ();
		}
	}

	public bool TriggerJoin() {
		if (state == SocState.Sleeping && mouseSelectedExps.Count > 1) {
			state = SocState.Joining;
			return true;
		}
		return false;
	}

	public bool TriggerEndJoin() {
		foreach (Experiment e in mouseSelectedExps) {
			if (!e.dna.HasFilledGenes ())
				return false;
		}
		foreach (Experiment e in mouseSelectedExps) {
			e.SetState (Experiment.ExpState.Pending);
		}
		mouseSelectedExps = new List<Experiment> ();
		return true;
	}

	void AddExperimentToWorld(Experiment e) {
		e.id = experiments.Count;
		experiments.Add (e);
		e.transform.parent = expHolder;
		e.transform.position = NewBoundedRandomLoc();
		state = SocState.Regrouping;
	}

	void AddFactoryExperiment() {
		factoryExp = CreateExperiment(factoryHolder, -1, new Vector3(4f,10.5f));
		factoryExp.SetState(Experiment.ExpState.Editing);
		factoryExp.dna.Setup ();
	}

	public void AddFactoryExpToList(Experiment e) {
		e.id = experiments.Count;
		experiments.Add (e);
	}

	public void RemoveExperiment(Experiment e) {
		// Make sure it wasn't selected somehow before dieing
		mouseSelectedExps.Remove (e);

		int i = experiments.IndexOf (e);
		if (i < 0)
			return;
		experiments.RemoveAt(i);
		if (i >= experiments.Count)
			return;
		for (int j = i; j < experiments.Count; j++) {
			experiments [j].id = j;
		}
	}

	public Vector3 NewBoundedRandomLoc() {
		Vector3 pos;
		int tries = 0;
		do {
			tries++;
			if (tries > 10)
				return new Vector3();
			pos = new Vector3 (UnityEngine.Random.Range (UI.societyBounds.x, UI.societyBounds.x + UI.societyBounds.z), UnityEngine.Random.Range (UI.societyBounds.y, UI.societyBounds.y + UI.societyBounds.w), 0);
		} while (Physics2D.OverlapCircle(pos,2f,1 << 9) != null);
		return pos;
	}

	public void AddToSelected(Experiment e) {
		if (mouseSelectedExps.Count == MAX_EXP_JOIN) {
			Experiment old = mouseSelectedExps[0];
			old.Deselect ();
			mouseSelectedExps.RemoveAt (0);
		}
		mouseSelectedExps.Add (e);
	}

	public void RemoveFromSelected(Experiment e) {
		mouseSelectedExps.Remove (e);
	}

	void CheckForVictory() {
		bool victory = true;
		bool allMatch = true;
		bool[] checkedOff = new bool[experiments.Count];
		int numCheckedOff = 0;
		int[,] values = new int[experiments.Count, geneMap.Length];
		int counter = 0;
		for (int i = 0; i < experiments.Count; i++) {
			foreach (int j in experiments [i].dna.GetGeneValues ()) {
				values [i, counter++] = j;
			}
			counter = 0;
		}
		if (goalGeneMapping.Length == 0 && GetNumOfExps() > 0) {
			victory = false;
		} else {
			for (int i = 0; i < goalGeneMapping.Length; i++) {
				for (int j = 0; j < experiments.Count; j++) {
					if (checkedOff [j])
						continue;
					allMatch = true;
					for (int k = 0; k < geneMap.Length; k++) {
						if (values [j, k] != goalGeneMapping [i].mapList [k]) {
							allMatch = false;
							break;
						}
					}
					if (allMatch) {
						checkedOff [j] = true;
						numCheckedOff++;
						break;
					}
				}
				if (numCheckedOff == i) {
					victory = false;
					break;
				}
			}
		}

		if (victory) {
			state = SocState.Finished;
			UI.ReportFinished ();
		} else {
			state = SocState.Sleeping;
		}
		//print ("Victorious: " + victory);
	}

	void SpawnGoalExperiments() {
		Transform child;
		SpriteRenderer expSprite = null;
		for (int i = 0; i < tempExp.transform.childCount; i++) {
			child = tempExp.transform.GetChild (i);
			if (!child.tag.Equals ("Experiment"))
				continue;
			expSprite = child.GetComponent<SpriteRenderer> ();
		}
		Vector3 expSize = expSprite.sprite.bounds.size * 2/3;
		Vector3 expCenter = expSize / 2;
		expCenter.y = -expCenter.y;
		//print ("Size: " + expSize + " Center: " + expCenter);
		Vector3 goalExpPos = new Vector3 (UI.goalBounds.x, UI.goalBounds.y + UI.goalBounds.w, 0); 
		for (int i = 0; i < goalGeneMapping.Length; i++) {
			Experiment e = CreateExperiment (goalHolder, i, goalExpPos + expCenter);
			e.dna.Setup (goalGeneMapping[i].mapList);
			e.SetState (Experiment.ExpState.Pending);
			goalExpPos.x += expSize.x;
			if (goalExpPos.x + expSize.x >= UI.goalBounds.x + UI.goalBounds.z) {
				goalExpPos.x = UI.goalBounds.x;
				goalExpPos.y -= expSize.y;
			}
		}
	}

	void AddNucleobases() {
		SpriteRenderer nSprite = ((GameObject)Resources.Load ("bases/nucleobase")).GetComponent<SpriteRenderer>();
		Vector3 nSize = Vector3.Scale(nSprite.sprite.bounds.size, nSprite.transform.localScale);
		Vector3 nCenter = nSize / 2;
		nCenter.y = -nCenter.y;
		Vector3 pos = new Vector3 (UI.inventoryBounds.x, UI.inventoryBounds.y + UI.inventoryBounds.w, 0); 
		for (int i = 0; i < looseNucleobaseDefaults.Length; i++) {
			Nucleobase n = ((GameObject)GameObject.Instantiate (nSprite.gameObject,pos + nCenter,Quaternion.identity,((GameObject) GameObject.Find ("NucleobaseHolder")).transform)).GetComponent<Nucleobase>();
			n.value = looseNucleobaseDefaults [i];
			pos.x += nSize.x;
			if (pos.x + nSize.x >= UI.inventoryBounds.x + UI.inventoryBounds.z) {
				pos.x = UI.inventoryBounds.x;
				pos.y -= nSize.y;
			}
		}
	}
}

public static class ListExtensions
{
	public static int BinarySearch<T>(this List<T> list,
		T item,
		Func<T, T, int> compare)
	{
		return list.BinarySearch(item, new ComparisonComparer<T>(compare));
	}
}

public class ComparisonComparer<T> : IComparer<T>
{
	private readonly Comparison<T> comparison;

	public ComparisonComparer(Func<T, T, int> compare)
	{
		if (compare == null)
		{
			throw new ArgumentNullException("comparison");
		}
		comparison = new Comparison<T>(compare);
	}

	public int Compare(T x, T y)
	{
		return comparison(x, y);
	}
}
