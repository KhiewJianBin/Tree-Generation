using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// https://github.com/RolandR/plants
/// https://draemm.li/various/plants/
/// conversion of js to c# unity
/// </summary>
public class Tree_Generation : MonoBehaviour
{
	public int treeCount = 1;
	public string seed = "A tree.";
	public float baseWidth = 15f;
	public float branchOverwidth = 0.33f;
	public float lengthWidthRatio = 5;
	public float lengthConstant = 20f;
	public float lengthRandomness = 1f;
	public float bendiness = 0.3f;
	public float angleSpan = 30f;
	public float spanRandomness = 30f;
	public float angleRandomness = 20f;
	public float widthRandomness = 1.3f;
	public int branchCount = 2;
	public float gravity = 0;
	public float minWidth = 0.26f;
	public float maxDepth = 13f;
	public float stemAngle = 0f;
	public float stemWeight = 0f;
	public float elasticity = 13f;
	public int thinBranchStrength = 1;
	public bool keepStructure = true;
	public bool animateWind = true;
	public float windSpeed = 0;
	public float windDirection = 270f;
	public float windTurbulence = 0.3f;
	public int branchLimit = 3000;
	// Leaves
	//,leafLength: 10

    void Start()
    {
        
    }

    void Update()
    {
        
    }

	void GenerateTrees(object config)
    {
		Random.InitState(seed.GetHashCode());

		List<plant> trees = new List<plant>();
		List<List<parent>> flatTrees = new List<List<parent>>();

		int totalBranchCount = 0;

		for (int i = 0; i < treeCount; i++)
		{
			var baseWidth = this.baseWidth + this.baseWidth * (Random.value - 0.5f) * widthRandomness;

			trees.Add(new plant(new branch(0 + Mathf.Deg2Rad * angleRandomness * (Random.value - 0.5f),
				baseWidth,
				baseWidth * lengthWidthRatio + +baseWidth * lengthWidthRatio * ((Random.value - 0.5f) * lengthRandomness) + lengthConstant,
				0,
				true)));

			totalBranchCount += generateStructure(trees[i]);
		}

		for (int i = 0; i < treeCount; i++)
		{
			flatTrees.Add(flatten(trees[i].structure));
		}

        //renderer = new Renderer();
        //renderer.renderAll();

        //document.getElementById("branchCount").innerHTML = totalBranchCount;
        if (totalBranchCount >= branchLimit)
        {
			print("branchLimitExceeded");
        }
        else
        {
			print("");
        }

    }
	List<parent> flatten(branch branch, parent parent = null)
	{
		List<parent> flat = new List<parent>();

		parent newparent = new parent(branch.angle, branch.angle, branch.centerOffset, branch.len, branch.width,new Vector2(0, 0), parent, 0, 0);
		
		flat.Add(newparent);

        foreach (var i in branch.branches)
        {
			if(i != null)
			{
				flat.AddRange(flatten(i, newparent));
			}
		}
		
		return flat;
	}
	class parent
	{
		public float angle;
		public float bendAngle;
		public float centerOffset;
		public float len;
		public float width;
		public parent par;
		public Vector2 end;
		public float cosAngle;
		public float sinAngle;
		public parent(float angle, float bendAngle, float centerOffset,float len, float width, Vector2 end,parent par,float cosAngle,float sinAngle)
		{
			this.angle = angle;
			this.bendAngle = bendAngle;
			this.centerOffset = centerOffset;
			this.len = len;
			this.width = width;
			this.par = par;
			this.end = end;
			this.cosAngle = cosAngle;
			this.sinAngle = sinAngle;
		}
	}

	int generateStructure(plant plant)
    {
		var totalBranchCount = 1; // One branch already exists

		float generateLength(float previousWidth)
		{
			float length = previousWidth * lengthWidthRatio + lengthConstant;
			length += length * ((Random.value - 0.5f) * lengthRandomness);
			if (length < 0)
			{
				length = 0;
			}
			return length;
		}
		List<float> distributeWidths(float previousWidth,int branchCount, int stemNo)
		{
			List<float> widths = new List<float>();
			List<float> weights = new List<float>();
			float sum = 0;
			// generate weights and calculate sum
			for (int i = 0; i < branchCount; i++)
			{
				var weight = 1 + (Random.value - 0.5f) * widthRandomness;
				if (i != stemNo)
				{
					weight -= weight * stemWeight;
				}
				weights.Add(weight);
				sum += weight;
			}
			// distribute width according to weights
			for (var i = 0; i < branchCount; i++)
			{
				var width = (weights[i] / sum) * previousWidth;
				width += (previousWidth - width) * branchOverwidth;
				widths.Add(width);
			}

			return widths;
		}
		List<float> distributeAngles(int branchCount, int stemNo)
		{
			float randomAngle = Mathf.Deg2Rad * angleRandomness;

			float spanRandomness = Mathf.Deg2Rad * this.spanRandomness;
			float angleSpan = Mathf.Deg2Rad * this.angleSpan;
			angleSpan = angleSpan + (Random.value - 0.5f) * spanRandomness;

			List<float> angles = new List<float>();
			float spanPerBranch = 0;
			if (branchCount > 1)
			{
				spanPerBranch = angleSpan / (branchCount - 1);
			}
			else
			{
				spanPerBranch = angleSpan;
			}
			float startingAngle = 0 - angleSpan / 2;

			for (int i = 0; i < branchCount; i++)
			{
				var angle = startingAngle + i * spanPerBranch;
				angles.Add(angle);
			}

			var stemAngle = angles[stemNo];

			for (int i = 0; i < branchCount; i++)
			{
				angles[i] = (angles[i] - (stemAngle) * stemAngle);
				angles[i] += (Random.value - 0.5f) * randomAngle;
			}
			return angles;
		}
		void addBranches(branch branch, int depth)
        {
			branch.branches = new List<branch>();

			var branchCount = this.branchCount;
			var stemNo = depth % branchCount;
			var angles = distributeAngles(branchCount, stemNo);
			var widths = distributeWidths(branch.width, branchCount, stemNo);
			
			for(var i = 0; i < branchCount; i++){
				
				if(widths[i] < minWidth){

					if(keepStructure){
						// Here, we do all rand() calls that would have done if minimum width weren't in place
						float endBranches = Mathf.Pow(branchCount, (maxDepth - depth));
						float skippedBranchings = endBranches - 1;

						var randCallsPerBranching = 1 + 3 * branchCount;

						var randCallsSkipped = randCallsPerBranching * skippedBranchings + 1;

						while(randCallsSkipped-- > 0){
							//Random.value;;
						}
					}
					
					continue;
				}

				if(totalBranchCount >= branchLimit){
					break;
				}
				
				totalBranchCount++;
				
				var centersSpan = branch.width - widths[0]/2 - widths[branchCount-1]/2;
				var centerOffset = (i / (branchCount-1)) * centersSpan + widths[0]/2 - branch.width/2;

				branch newBranch = new branch(generateLength(branch.width),
					angles[i],
					widths[i],
					centerOffset);
				
				branch.branches.Add(newBranch);
				
				if(depth < maxDepth){
					addBranches(newBranch, depth + 1);
				}
			}
        }

		addBranches(plant.structure, 2);

		return totalBranchCount;
	}
	class plant
    {
		public branch structure;
		public plant(branch structure)
        {
			this.structure = structure;
		}
    }
	class branch
	{
		public float angle;
		public float width;
		public float len;
		public float centerOffset;
		public bool isRoot;
		public List<branch> branches;
		public branch(float len, float angle, float width, float centerOffset, bool isRoot = false)
		{
			this.len = len;
			this.angle = angle;
			this.width = width;
			this.centerOffset = centerOffset;
			this.isRoot = isRoot;
		}
	}
}
