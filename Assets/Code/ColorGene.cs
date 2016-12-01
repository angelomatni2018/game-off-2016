using UnityEngine;
using System.Collections;

public class ColorGene : Gene {

	public ColorGene(int num) : base(num) {}

	//public ColorGene(int type, int num) : base(type,num) {}

	public override void UpdateExperiment(GameObject exp, GameObject dna) {
		//Debug.Log ("Exp: " + exp.name + "  " + geneType);
		if (geneType < 0)
			exp.GetComponent<Experiment> ().SetColor (new Color (.8f, .8f, .8f, .5f));
		else
			exp.GetComponent<Experiment> ().SetColor (GeneValues.colors [geneType]);
	}
}
