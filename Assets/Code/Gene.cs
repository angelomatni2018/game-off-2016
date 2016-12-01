using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class Gene {
	//Only for reference. I just don't want to have to cast in c# constantly
	//enum CodonType {Color, Size, Speed, Shape};
	//enum BaseType {C,G,A,T};
	protected int geneType;
	protected Nucleobase[] nucleobases;

	public Gene(int num) {
		nucleobases = new Nucleobase[num];
		geneType = -1;
	}

	/*public Gene(int type, int num) {
		nucleobases = new Nucleobase[num];
		geneType = type;
		DeriveBases ();
	}*/

	// Check for case where a base is already present
	public void AddBase(int num, Nucleobase b) {
		if (nucleobases [num] != b) {
			nucleobases [num] = b;
			DeriveType ();
		}
	}

	public Nucleobase RemoveBase(int num) {
		Nucleobase b = nucleobases [num];
		if (b) {
			nucleobases [num] = null;
			DeriveType ();
		}
		return b;
	}

	public bool HasBase(int num) {
		return nucleobases [num] != null;
	}

	public int GetGeneType() {
		return geneType;
	}

	void DeriveType() {
		int type = 0;
		int b = 1;
		for (int i = 0; i < nucleobases.Length; i++) {
			if (!nucleobases [i]) {
				geneType = -1;
				return;
			}
			type += b * nucleobases[i].value;
			b *= GeneValues.NUM_BASE_TYPES;
		}
		geneType = type;
	}

	/*void DeriveBases() {
		// Used to determine Nucleobases that make up this gene
		int b = (int)Mathf.Pow (GeneValues.NUM_BASE_TYPES, nucleobases.Length - 1);
		int j = 0;
		for (int i = nucleobases.Length - 1; i > -1; i--) {
			int num = geneType / b;
			nucleobases [j++] = GeneValues.Nucleobases [num].GetComponent<Nucleobase>();
			geneType %= b;
			b /= GeneValues.NUM_BASE_TYPES;
		}
	}*/

	public virtual void UpdateExperiment(GameObject exp, GameObject dna) {}
}


public static class GeneValues {
	public static int NUM_BASE_TYPES;

	/*static GameObject NucleoHolder = GameObject.Find ("NucleobaseHolder");
	static GameObject ExperimentHolder = GameObject.Find("ExperimentHolder");
	static GameObject FactoryHolder = GameObject.Find ("FactoryHolder");
	static GameObject GoalHolder = GameObject.Find ("GoalHolder");
	static GameObject basePrefab = Resources.Load ("bases/nucleobase") as GameObject;
	static GameObject tempExp = Resources.Load ("temp_experiment") as GameObject;*/

	// No genes have 0 or 1 slots for now
	public static GameObject[] dnaslots = new GameObject[3] {
		null,
		null,
		Resources.Load ("slots/2-slotted") as GameObject
	};

	public static Dictionary<string,Func<int,Gene>> geneInits = new Dictionary<string, Func<int, Gene>>() {
		{"color", delegate(int num) { return new ColorGene(num); }},
		{"shape", delegate(int num) { return new ShapeGene(num); }}
	};

	public static GameObject[] Nucleobases = new GameObject[4] {
		Resources.Load("bases/a") as GameObject,
		Resources.Load("bases/c") as GameObject,
		Resources.Load("bases/g") as GameObject,
		Resources.Load("bases/t") as GameObject
	};

	public static Color[] colors = new Color[16] {
		Color.red ,
		Color.blue ,
		Color.yellow ,
		new Color(0,1,0) , //lime
	 	new Color(1,140f/255,0) , //orange
	 	Color.black ,
		new Color(75f/255,0,130f/255) , //indigo
		Color.grey ,
	    Color.cyan ,
		Color.magenta ,
	 	Color.white ,
	 	new Color(210f/255,105f/255,30f/255) , //brown
	 	new Color(139f/255,0,0) , //darkred
	 	new Color(0,100f/255,0) , //darkgreen
	 	new Color(255f/255,105f/255,180f/255) , //hotpink
		new Color(218f/255,165f/255,32f/255)  //goldenrod
	};

	public static GameObject nonshape = Resources.Load("shapes/noshape") as GameObject;

	public static GameObject[] shapes = new GameObject[16] {
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/square") as GameObject,
		Resources.Load("shapes/diamond") as GameObject,
		Resources.Load("shapes/circle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject,
		Resources.Load("shapes/triangle") as GameObject
		/*Resources.Load("shapes/pentagon") as GameObject,
		Resources.Load("shapes/cross") as GameObject,
		Resources.Load("shapes/trapezoid") as GameObject,
		Resources.Load("shapes/circle") as GameObject,
		Resources.Load("shapes/donut") as GameObject,
		Resources.Load("shapes/parallelogram") as GameObject,
		Resources.Load("shapes/asterisk") as GameObject,
		Resources.Load("shapes/diamond") as GameObject,
		Resources.Load("shapes/heptagon") as GameObject,
		Resources.Load("shapes/crescent") as GameObject,
		Resources.Load("shapes/curvilinear_triangle") as GameObject,
		Resources.Load("shapes/teardrop") as GameObject, 
		Resources.Load("shapes/y") as GameObject,
		Resources.Load("shapes/z") as GameObject*/
	};
}
