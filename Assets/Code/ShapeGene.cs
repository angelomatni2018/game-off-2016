using UnityEngine;
using System.Collections;

public class ShapeGene : Gene {

	public ShapeGene(int num) : base(num) {}

	//public ShapeGene(int type, int num) : base(type,num) {}

	public override void UpdateExperiment(GameObject exp, GameObject dna) {
		// CHANGE HOW THE EXPERIMENT IS REPLACED.
		// Copy over the experiment component making sure id and current act stay intact.
		GameObject newShape;
		if (geneType < 0)
			newShape = GeneValues.nonshape;
		else
			newShape = GeneValues.shapes [geneType];

		//Debug.Log(newExperiment + " | " + exp + " | " + GeneValues.ExperimentHolder);
		newShape = (GameObject) GameObject.Instantiate (newShape, exp.transform.position, Quaternion.identity, exp.transform);
		dna.transform.parent = exp.transform;
		dna.GetComponent<DNA>().SetExperiment (exp);
		exp.GetComponent<Experiment>().ReplaceShapeObj(newShape);
		//exp.gameObject.name = "ToBeDestroyed";
		//exp.GetComponent<Experiment> ().DestroySelf ();
	}
}
