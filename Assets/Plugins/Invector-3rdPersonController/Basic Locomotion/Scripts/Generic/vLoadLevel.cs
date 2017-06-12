using UnityEngine;
using System.Collections;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class vLoadLevel : MonoBehaviour 
{
	[Tooltip("Write the name of the level you want to load")]
	public string levelToLoad;
	[Tooltip("True if you need to spawn the character into a transform location on the scene to load")]
	public bool findSpawnPoint = true;
	[Tooltip("Assign here the spawnPoint name of the scene that you will load")]
	public string spawnPointName;
	
	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag.Equals("Player"))
		{
			var spawnPointFinderObj = new GameObject("spawnPointFinder");
			var spawnPointFinder = spawnPointFinderObj.AddComponent<vFindSpawnPoint>();
			//Debug.Log(spawnPointName+" "+gameObject.name);
			
			spawnPointFinder.AlighObjetToSpawnPoint(other.gameObject,spawnPointName);
			
			#if UNITY_5_3_OR_NEWER
				SceneManager.LoadScene(levelToLoad);
			#else
        		Application.LoadLevel(levelToLoad);
			#endif
		}
	}
}
