using UnityEngine;
using System.Collections;

public class ColorGene : Gene {

	public ColorGene(int num) : base(num) {}

	//public ColorGene(int type, int num) : base(type,num) {}

	public override void UpdateExperiment(GameObject exp, GameObject dna) {
		//Debug.Log ("Exp: " + exp.name + "  " + exp.GetComponent<Experiment> ());
		if (geneType < 0)
			exp.GetComponent<Experiment> ().SetColor (new Color (1, 1, 1, .5f));
		else
			exp.GetComponent<Experiment> ().SetColor (GeneValues.colors [geneType]);
	}
}
