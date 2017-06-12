using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;

public class vFootStepFromTexture : vFootPlantingPlayer 
{
    public AnimationType animationType = AnimationType.Humanoid;
	public bool debugTextureName;
  
	private int surfaceIndex = 0;	
	private Terrain terrain;
    private TerrainCollider terrainCollider;
	private TerrainData terrainData;
	private Vector3 terrainPos;

	public vFootStepTrigger leftFootTrigger;
	public vFootStepTrigger rightFootTrigger;
    public Transform currentStep;
    public List<vFootStepTrigger> footStepTriggers;
	void Start () 
	{		
		if (terrain == null) 
		{ 
			terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                terrainData = terrain.terrainData;
                terrainPos = terrain.transform.position;
                terrainCollider = terrain.GetComponent<TerrainCollider>();                
            }
		}

        var colls = GetComponentsInChildren<Collider>();
        if(animationType == AnimationType.Humanoid)
        {
            if (leftFootTrigger == null && rightFootTrigger == null)
            {
                Debug.Log("Missing FootStep Sphere Trigger, please unfold the FootStep Component to create the triggers.");
                return;
            }
            else
            {
                leftFootTrigger.trigger.isTrigger = true;
                rightFootTrigger.trigger.isTrigger = true;
                Physics.IgnoreCollision(leftFootTrigger.trigger, rightFootTrigger.trigger);
                for(int i=0;i<colls.Length;i++)
                {
                    var coll = colls[i];
                    if (coll.enabled && coll.gameObject != leftFootTrigger.gameObject)
                        Physics.IgnoreCollision(leftFootTrigger.trigger, coll);
                    if (coll.enabled && coll.gameObject != rightFootTrigger.gameObject)
                        Physics.IgnoreCollision(rightFootTrigger.trigger, coll);
                }               
            }
        }           
        else
        {
            for (int i = 0; i < colls.Length; i++)
            {
                var coll = colls[i];
                for(int a=0;a<footStepTriggers.Count;a++)
                {
                    var trigger = footStepTriggers[i];
                    trigger.trigger.isTrigger = true;
                    if (coll.enabled && coll.gameObject != trigger.gameObject)
                        Physics.IgnoreCollision(trigger.trigger, coll);
                }                       
            }
        }
        
	}
	
	private float[] GetTextureMix(Vector3 WorldPos)
	{
		// returns an array containing the relative mix of textures
		// on the main terrain at this world position.
		
		// The number of values in the array will equal the number
		// of textures added to the terrain.
		
		// calculate which splat map cell the worldPos falls within (ignoring y)
		int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
        if (!terrainCollider.bounds.Contains(WorldPos)) return new float[0];
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1 );
		
		// extract the 3D array data to a 1D array:
		float[] cellMix = new float[ splatmapData.GetUpperBound(2) + 1 ];
		
		for(int n=0; n<cellMix.Length; n++){
			cellMix[n] = splatmapData[ 0, 0, n ];
		}
		return cellMix;
	}
	
	private int GetMainTexture(Vector3 WorldPos)
	{
		// returns the zero-based index of the most dominant texture
		// on the main terrain at this world position.
		float[] mix = GetTextureMix(WorldPos);
		
		float maxMix = 0;
		int maxIndex = 0;
		
		// loop through each mix value and find the maximum
		for(int n=0; n<mix.Length; n++)
		{
			if ( mix[n] > maxMix )
			{
				maxIndex = n;
				maxMix = mix[n];
			}
		}
		return maxIndex;
	}
    /// <summary>
    /// Step on Terrain
    /// </summary>
    /// <param name="footStepObject"></param>
	public void StepOnTerrain(FootStepObject footStepObject)
    {
        if (currentStep != null && currentStep == footStepObject.sender) return;
        currentStep = footStepObject.sender;

        if (terrainData)
            surfaceIndex = GetMainTexture(footStepObject.sender.position);
      
        //bool valid = (terrainData != null && terrainData.splatPrototypes != null && terrainData.splatPrototypes.Length > 0
                    //&& terrainData.splatPrototypes.Length <= surfaceIndex && terrainData.splatPrototypes[surfaceIndex].texture != null);
        var name = (terrainData != null && terrainData.splatPrototypes.Length > 0)? ( terrainData.splatPrototypes[surfaceIndex]).texture.name:"" ;
        footStepObject.name = name;
        PlayFootFallSound(footStepObject);

        if (debugTextureName)
            Debug.Log(name);
    }
    /// <summary>
    /// Step on Mesh
    /// </summary>
    /// <param name="footStepObject"></param>
	public void StepOnMesh(FootStepObject footStepObject)
	{
        if (currentStep != null && currentStep == footStepObject.sender) return;
        currentStep = footStepObject.sender;
        PlayFootFallSound (footStepObject);
		if (debugTextureName)
			Debug.Log (footStepObject.name);
	}    
}
public enum AnimationType
{
    Humanoid, Generic
}