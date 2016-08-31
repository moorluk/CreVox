using UnityEngine;

namespace CreVox
{

	public class PaletteItem : MonoBehaviour
	{
#if UNITY_EDITOR
		public enum Category
		{
			System,
			Build,
			Chara,
		}

		public Category category = Category.System;
		public string itemName = "";
		public Object inspectedScript; 
#endif

		// Use this for initialization
		void Start()
		{
	
		}
	
		// Update is called once per frame
		void Update()
		{
	
		}
	}
}