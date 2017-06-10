using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CreVox
{
	public class MissingPiece : LevelPiece 
	{
		public bool usePrefab = false;
		public PaletteItem tempObj = null;

		public override void SetupPiece(BlockItem item)
		{
		}
	}
}