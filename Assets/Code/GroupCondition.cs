using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GroupCondition : MonoBehaviour {

	public static int MAX_EXPS = 100000;
	// Gene to be checked on (values of gene are irrelevant, only its type matters)
	public string geneT;
	public int maxTypesAllowed;

	// Defaults to Mathf.Infinity for each type, in other words no bounds
	public int[] maxExpPerType;

	Society society;
	public void Setup () {
		society = GameObject.Find("ExperimentHolder").GetComponent<Society>();
		if (maxExpPerType.Length == 0) {
			maxExpPerType = new int[(int)Mathf.Pow (GeneValues.NUM_BASE_TYPES, society.NUM_BASES_PER_GENE)];
			for (int i = 0; i < maxExpPerType.Length; i++) {
				maxExpPerType [i] = MAX_EXPS;
			}
		}
	}

	public bool ConditionMet(Experiment e, int[,] numExp) {
		int type = -1;
		bool allGenesTrue = true;
		for (int i = 0; i < society.geneMap.Length; i++) {
			if (society.geneMap [i] != geneT)
				continue;
			type = e.dna.GetGeneByIndex (i);

			// Calculate how many types of experiments there are for this gene
			int counter = 1; 
			for (int j = 0; j < numExp.GetLength (1); j++) {
				if (numExp [i, j] > 0 && j != type)
					counter++;
			}

			// If exceeded cap on # of experiments of any one type or # of types of experiments 
			if (numExp [i, type] == maxExpPerType [i] || counter > maxTypesAllowed)
				allGenesTrue = false;
		}
		return allGenesTrue;
	}
}

public class GroupStats {
	// Used to sort experiments in society List based on whatever condition the group wants to sort by
	public int[] sortedExps;
	public int numToSort;
	Society soc;
	int[,] numExpPerType;

	// Center position of group
	public Vector2 center;
	public Vector2[] expSpots;
	// Used in algorithm to center group
	public bool nodeFound;

	// Used for attacking
	public bool[] bumped;

	public GroupStats(Society society) {
		soc = society;
		sortedExps = new int[soc.GetNumOfExps ()];
		numToSort = 0;		
		nodeFound = false;
		bumped = null;
		numExpPerType = new int[soc.geneMap.Length, (int)Mathf.Pow(GeneValues.NUM_BASE_TYPES,soc.NUM_BASES_PER_GENE)];
		for (int i = 0; i < numExpPerType.GetLength (0); i++) {
			for (int j = 0; j < numExpPerType.GetLength (1); j++) {
				numExpPerType [i, j] = 0;
			}
		}
	}

	public void SetGroupSort(int size) {
		sortedExps = new int[size];
	}

	public void AddToGroup(int expId) {
		sortedExps [numToSort++] = expId;
	}
		
	public bool CanAddToGroup(Experiment e) {
		bool allTrue = true;
		foreach (GroupCondition gc in soc.gConditions) {
			if (!gc.ConditionMet (e, numExpPerType)) {
				allTrue = false;
			}			
		}
		if (allTrue) {
			for (int i = 0; i < soc.geneMap.Length; i++) {
				numExpPerType [i,e.dna.GetGeneByIndex (i)]++;
			}
		}
		//Debug.Log ("Checked: " + e.name + " and: " + allTrue);
		return allTrue;
	}

	public void ResortExps() {
		int[] newSorted = new int[numToSort];
		expSpots = new Vector2[numToSort];
		float minDist, tempDist;
		int counter = 0, minIndex;
		foreach (Vector2 spot in GroupValues.ExpSpots[numToSort]) {
			minDist = Mathf.Infinity;
			minIndex = -1;
			for (int i = 0; i < numToSort; i++) {
				if (sortedExps[i] < 0)
					continue;
				if ((tempDist = Vector3.Distance (soc.GetExpWithId (sortedExps[i]).transform.position, (Vector3)spot)) < minDist) {
					minIndex = i;
					minDist = tempDist;
				}
			}
			expSpots [counter] = spot;
			newSorted [counter++] = sortedExps [minIndex];
			sortedExps [minIndex] = -1;
		}
		sortedExps = newSorted;
	}

	public Vector2 GetExpSpot(Experiment e) {
		for (int i = 0; i < numToSort; i++) {
			if (sortedExps [i] == e.id) {
				//Debug.Log (e.id + "  " + e.GetGroup() + "  " + (center + expSpots[i]) + " " + expSpots[i]);
				return center + expSpots [i];
			}
		}
		return center;
	}

	public bool InGroup(int id) {
		foreach (int i in sortedExps) {
			if (id == i)
				return true;
		}
		return false;
	}

	public void InitAttack() {
		if (bumped == null) {
			bumped = new bool[numToSort];
			for (int i = 0; i < numToSort; i++) {
				bumped[i] = false;
			}
		}
	}

	public void EndAttackIfPossible(int expId) {
		bool allBumped = true;
		for (int i = 0; i < numToSort; i++) {
			//Debug.Log ("Bumped: " + i + "  " + bumped [i]);
			if (sortedExps [i] == expId) {
				bumped [i] = true;
			}
			allBumped &= bumped [i];
		}
		if (allBumped) {
			for (int i = 0; i < numToSort; i++) {
				soc.GetExpWithId (sortedExps [i]).RemoveAct (Experiment.ExpState.Death);
			}
		}
	}
}

public static class GroupValues {
	public static List<Vector2[]> ExpSpots = new List<Vector2[]> () {
		null,
		null,
		new Vector2[2] { new Vector2(0,3), new Vector2(0,-3) },
		new Vector2[3] { new Vector2(0,3), new Vector2(-3,-2.5f), new Vector2(3,-2.5f) },
		new Vector2[4] { new Vector2(-5,5), new Vector2(5,5), new Vector2(-5,-5), new Vector2(5, -5) },
		new Vector2[5] { new Vector2(0,5), new Vector2(-5,.5f), new Vector2 (5,.5f), new Vector2(-3.25f, -5), new Vector2(3.25f, -5) },
		new Vector2[6] { new Vector2(0,6), new Vector2(-5,2.5f), new Vector2(5,2.5f), new Vector2(-5,-2.5f), new Vector2(5,-2.5f), new Vector2(0,-6) },
	};
}