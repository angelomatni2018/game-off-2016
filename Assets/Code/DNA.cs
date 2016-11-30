using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DNA : MonoBehaviour {
	GameObject looseNucleobases;
	Experiment experiment;
	Society society;
	// nucleobase slots indexed by gene and nucleobase position for that gene
	DNASlot[,] slots;
	// list of genes for this DNA strand
	List<Gene> genes;

	void StandardSetup() {
		genes = new List<Gene> ();
		experiment = transform.parent.GetComponent<Experiment>();
		looseNucleobases = GameObject.Find ("NucleobaseHolder");
		society = GameObject.Find("ExperimentHolder").GetComponent<Society>();
		int numBases = society.NUM_BASES_PER_GENE;
		foreach (string geneT in society.geneMap) {
			genes.Add (GeneValues.geneInits [geneT](numBases));
		}
		slots = new DNASlot[society.geneMap.Length,society.NUM_BASES_PER_GENE];
	}

	// Use this for initialization
	public void Setup (int[] valueArray) {
		StandardSetup ();

		DNASlot slotObj;
		GameObject geneHolder;
		int slotValue = -1, divisor;
		for (int i = 0; i < slots.GetLength(0); i++) {
			slotValue = valueArray [i];
			divisor = (int)Mathf.Pow (GeneValues.NUM_BASE_TYPES, slots.GetLength(1) - 1);
			// SET THE GENEHOLDER FROM A PREFAB HOLDING DNASlots OBJECTS IN IT TOO. REMOVE SLOTOBJ INSTANTIATION THEN -------------------------------
			geneHolder = (GameObject)GameObject.Instantiate (GeneValues.dnaslots[society.NUM_BASES_PER_GENE], transform);
			//print ("Parent: " + geneHolder.transform.parent.parent.position + "  " + geneHolder.transform.parent.parent.localScale);
			//print ("Self: " + geneHolder.transform.position + "  " + geneHolder.transform.localScale);
			//print ("Size: " + geneHolder.GetComponent<SpriteRenderer> ().sprite.bounds.size);
			//print ("Scale: " + geneHolder.transform.localScale);
			Vector3 slotSize = geneHolder.GetComponent<SpriteRenderer> ().sprite.bounds.size;
			slotSize = Vector3.Scale (slotSize, geneHolder.transform.localScale);
			Vector3 slotCenter = slotSize / 2;
			float xpos = (-slots.GetLength (0) / 2 + i) * slotSize.x + slotCenter.x * (slots.GetLength(0) % 2 == 0 ? 1 : 0);
			float ypos = experiment.GetComponent<Experiment> ().sHeight / 2 + slotSize.y - slotCenter.y;
			geneHolder.transform.localPosition = new Vector3 (xpos, ypos, 0);
			//print ("Height: " + experiment.GetComponent<Experiment> ().sHeight + "  " + slotSize.y);
			//print ("Pos: " + geneHolder.transform.localPosition);
			//print ("Slot index: " + i + " slot value: " + slotValue + " start divisor: " + divisor);
			for (int j = slots.GetLength(1) - 1; j > -1; j--) {
				slotObj = geneHolder.transform.GetChild(j).GetComponent<DNASlot>();
				//	print ("new divisor: " + (divisor / GeneValues.NUM_BASE_TYPES) + " calculated value: " + (slotValue / divisor));
				slotObj.Setup (slotValue / divisor);
				slotValue %= divisor;
				divisor /= GeneValues.NUM_BASE_TYPES;
				slotObj.gene = i;
				slotObj.slot = j;
				slots [i, j] = slotObj;
				genes [i].AddBase (j, slotObj.GetComponentInChildren<Nucleobase>());
				// SET THE GENE TYPE USING ADD TO GENE HERE -----------------------------------------------------------------------------------------
			}
		}
		// Get all dna slots for each gene
		/*slots = new DNASlot[transform.childCount,numBases];
		for (int i = 0; i < transform.childCount; i++) {
			Transform g = transform.GetChild (i);
			foreach (DNASlot d in g.GetComponentsInChildren<DNASlot>()) {
				slots [d.gene, d.slot] = d;
			}
		}*/
		ProcessDNA ();
	}

	public void Setup() {
		StandardSetup ();
		GameObject geneHolder;
		DNASlot slotObj;
		for (int i = 0; i < slots.GetLength (0); i++) {
			geneHolder = (GameObject)GameObject.Instantiate (GeneValues.dnaslots[society.NUM_BASES_PER_GENE], transform);
			//print ("Parent: " + geneHolder.transform.parent.parent.position + "  " + geneHolder.transform.parent.parent.localScale);
			//print ("Self: " + geneHolder.transform.position + "  " + geneHolder.transform.localScale);
			//print ("Size: " + geneHolder.GetComponent<SpriteRenderer> ().sprite.bounds.size);
			//print ("Scale: " + geneHolder.transform.localScale);
			Vector3 slotSize = geneHolder.GetComponent<SpriteRenderer> ().sprite.bounds.size;
			slotSize = Vector3.Scale (slotSize, geneHolder.transform.localScale);
			Vector3 slotCenter = slotSize / 2;
			float xpos = (-slots.GetLength (0) / 2 + i) * slotSize.x + slotCenter.x * (slots.GetLength(0) % 2 == 0 ? 1 : 0);
			float ypos = experiment.sHeight / 2 + slotSize.y - slotCenter.y;
			geneHolder.transform.localPosition = new Vector3 (xpos, ypos, 0);
			//print ("Height: " + experiment.sHeight + "  " + slotSize.y);
			//print ("Pos: " + geneHolder.transform.localPosition);
			for (int j = 0; j < slots.GetLength (1); j++) {
				slotObj = geneHolder.transform.GetChild(j).GetComponent<DNASlot>();
				slotObj.gene = i;
				slotObj.slot = j;
				slots [i, j] = slotObj;
			}
		}
		ProcessDNA ();
	}

	public void AddToGene(int gene, int slot, Nucleobase b) {
		// Take care of overlap nucleo bases
		if (genes [gene].HasBase (slot)) {
			Nucleobase oldB = genes [gene].RemoveBase (slot);
			b.transform.parent = looseNucleobases.transform;
			oldB.transform.position = oldB.startLoc;
			// Break nucleobase ties to dna
			oldB.UnqueueSlot (this);
			//print ("Processing Forced Removal");
		}
		genes [gene].AddBase (slot, b);
		slots [gene, slot].AddBase (b);
		b.transform.parent = slots[gene,slot].transform;
		b.transform.localPosition = slots[gene,slot].GetComponent<Collider2D>().offset;
		//print ("Processing Addition");
		ProcessDNA ();
	}

	public void RemoveGene(int gene, int slot) {
		if (genes [gene].HasBase (slot)) {
			Nucleobase b = genes [gene].RemoveBase (slot);
			slots [gene, slot].RemoveBase ();
			b.transform.parent = looseNucleobases.transform;
			//print ("Processing Removal");
		}
		ProcessDNA ();
	}

	void ProcessDNA() {
		foreach (Gene g in genes) {
			g.UpdateExperiment (experiment.gameObject, gameObject);
		}
	}

	public void SetExperiment(GameObject newExp) {
		experiment = newExp.GetComponent<Experiment>();
	}

	public int GetGeneByIndex(int index) {
		return genes [index].GetGeneType ();
	}

	public int[] GetGeneValues() {
		int[] values = new int[genes.Count];
		for (int i = 0; i < genes.Count; i++) {
			values [i] = genes [i].GetGeneType ();
		}
		return values;
	}

	public void MakeVisibleAndInteractive(bool visible, bool active) {
		/*for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).gameObject.SetActive (visible);
		}*/
		foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
			sr.enabled = visible;
		}
		foreach (Collider2D col in GetComponentsInChildren<Collider2D>()) {
			col.enabled = active;
		}
	}
		
	public bool SameTypeOfExp(int[] map) {
		for (int i = 0; i < genes.Count; i++) {
			if (map [i] != genes [i].GetGeneType ())
				return false;
		}
		return true;
	}

	public bool HasFilledGenes() {
		foreach (Gene g in genes) {
			if (g.GetGeneType() < 0)
				return false;
		}
		return true;
	}

	public void CalibrateNucleobasesForJoin() {
		Vector4 bounds = society.UI.societyBounds;
		foreach (DNASlot s in slots) {
			s.GetBase ().SetStartLocToOffsetOfPos ();
			s.GetBase ().SetBounds (bounds);
		}
	}
}
