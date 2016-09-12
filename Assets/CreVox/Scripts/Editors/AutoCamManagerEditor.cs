using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CreVox
{
	
	[CustomEditor (typeof(AutoCamManager))]
	public class AutoCamManagerEditor : Editor
	{
		AutoCamManager acm;
		int[] _obs = new int[3 * 3];
		int[] _scl = new int[3 * 3];
		int[] _adj = new int[3 * 3];
		int[] _dir = new int[3 * 3];
		int[] _id = new int[3 * 3];

		bool drawDef = false;

		void Start ()
		{
		}

		public override void OnInspectorGUI ()
		{
			acm = (AutoCamManager)target;
			_obs = acm.obsLayer;
			_scl = acm.sclLayer;
			_adj = acm.adjLayer;
			_dir = acm.dirLayer;
			_id = acm.idLayer;

			acm.mainDir = (CamDir)EditorGUILayout.EnumPopup (
				EditorApplication.isPlaying ? "Main Direction" : "Start Direction",
				acm.mainDir, 
				GUILayout.Width (Screen.width - 60)
			);

			EditorGUILayout.BeginVertical ();
			EditorGUILayout.BeginHorizontal ();
			DrawZone (acm.Turn (6));
			DrawZone (acm.Turn (7));
			DrawZone (acm.Turn (8));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			DrawZone (acm.Turn (3));
			DrawZone (acm.Turn (4));
			DrawZone (acm.Turn (5));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			DrawZone (acm.Turn (0));
			DrawZone (acm.Turn (1));
			DrawZone (acm.Turn (2));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();

			drawDef = EditorGUILayout.Foldout (drawDef,"Default Inspector");
			if (drawDef)
				DrawDefaultInspector ();
		}

		void DrawZone (int _zone)
		{
			float w = (Screen.width - 60) / 3;
			Color oldColor = GUI.color;
			if (_zone == 4)
				GUI.color = Color.yellow;
			else
				GUI.color = _obs [_zone] == 0 ? Color.gray : Color.white;

			EditorGUILayout.BeginVertical ("Box");

			EditorGUILayout.LabelField (
				"id:" + _id [_zone].ToString (),
				EditorStyles.boldLabel,
				GUILayout.Width (w)
			);

			if (_scl [_zone] < 0)
				GUI.color = Color.gray;
			EditorGUILayout.TextField (
				"scr:" + _scl [_zone].ToString (),
				GUILayout.Width (w));
			GUI.color = oldColor;

			EditorGUILayout.TextField (
				"adj:" + _adj [_zone].ToString (),
				GUILayout.Width (w)
			);

			EditorGUILayout.TextField (
				"dir:" + _dir [_zone].ToString (), 
				GUILayout.Width (w)
			);

			EditorGUILayout.EndVertical ();

			GUI.color = oldColor;
		}
	}
}
