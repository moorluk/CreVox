﻿using UnityEngine;
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

		Color oldColor = Color.white;
		float w = 55;

		bool drawDef = false;

		void OnEnable ()
		{
			acm = (AutoCamManager)target;
			_obs = acm.obsLayer;
			_scl = acm.sclLayer;
			_adj = acm.adjLayer;
			_dir = acm.dirLayer;
			_id = acm.idLayer;
		}

		public override void OnInspectorGUI ()
		{
			GUILayout.BeginHorizontal ();
			acm.mainDir = (CamDir)EditorGUILayout.EnumPopup (
				EditorApplication.isPlaying ? "Main Direction" : "Start Direction"
				, acm.mainDir);
			EditorGUILayout.IntField ((int)acm.mainDir, GUILayout.Width (50));
			GUILayout.EndHorizontal ();

			w = EditorGUILayout.Slider ("DebugBlock Size", w, 50, 100);
			GUILayout.Space (5);
			Rect n = GUILayoutUtility.GetLastRect ();

			GUI.color = oldColor;
			EditorGUI.DrawRect (new Rect (10, n.position.y + n.height, w * 3 + 95, w * 3 + 63), oldColor);

			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal ();
			DrawBlock (acm.Turn (6));
			DrawBlock (acm.Turn (7));
			DrawBlock (acm.Turn (8));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			DrawBlock (acm.Turn (3));
			DrawBlock (acm.Turn (4));
			DrawBlock (acm.Turn (5));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			DrawBlock (acm.Turn (0));
			DrawBlock (acm.Turn (1));
			DrawBlock (acm.Turn (2));
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();

			GUILayout.Space (5);
			drawDef = EditorGUILayout.Foldout (drawDef, "Default Inspector");
			if (drawDef)
				DrawDefaultInspector ();
		}

		void DrawCorner ()
		{
			GUILayout.Label (
				""
				, GUILayout.Height (5), GUILayout.Width (5)
			);
		}

		void DrawEdge (float _height, float _width, WorldPos _pos, Direction _dir)
		{
			GUI.color = acm.volume.GetBlock (_pos.x, _pos.y, _pos.z).IsSolid (_dir) ? Color.gray : oldColor;
			EditorStyles.textArea.margin = new RectOffset (2, 2, 10, 10);
			GUILayout.TextArea (
				_dir.ToString ()
				, (GUI.color == oldColor) ? "Label" : "TextArea"
				, GUILayout.Height (_height), GUILayout.Width (_width)
			);
			GUI.color = oldColor;
		}

		void DrawZone (int _zone)
		{
			GUILayout.BeginVertical ();
			float _h = w / 4 - 1;

			GUILayout.TextArea (
				"id:" + _id [_zone].ToString ()
				, EditorStyles.miniTextField
				, GUILayout.Width (w), GUILayout.Height (_h)
			);

			GUI.color = (_scl [_zone] < 0) ? Color.red : oldColor;
			GUILayout.TextArea (
				"scr:" + _scl [_zone].ToString ()
				, EditorStyles.miniTextField
				, GUILayout.Width (w), GUILayout.Height (_h)
			);

			GUI.color = (_adj [_zone] != _dir [_zone]) ? Color.yellow : oldColor;
			GUILayout.TextArea (
				"adj:" + _adj [_zone].ToString ()
				, EditorStyles.miniTextField
				, GUILayout.Width (w), GUILayout.Height (_h)
			);

			GUI.color = oldColor;
			GUILayout.TextArea (
				"dir:" + _dir [_zone].ToString ()
				, EditorStyles.miniTextField
				, GUILayout.Width (w), GUILayout.Height (_h)
			);

			GUILayout.EndVertical ();
		}

		void DrawBlock (int _zone)
		{
			WorldPos _pos = acm.GetNeighbor (acm.curPos, _zone);

			GUI.color = _obs [_zone] == 0 ? Color.gray : oldColor;			
			GUILayout.BeginVertical (""
				, (GUI.color == oldColor) ? "Label" : "TextArea"
				, GUILayout.Width (w + 16), GUILayout.Height (w + 16));
			GUI.color = oldColor;

			GUILayout.BeginHorizontal ();
			DrawCorner ();
			DrawEdge (5, w, _pos, acm.Turn (Direction.north,acm.mainDir));
			DrawCorner ();
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			DrawEdge (w, 5, _pos, acm.Turn (Direction.west,acm.mainDir));
			DrawZone (_zone);
			DrawEdge (w, 5, _pos, acm.Turn (Direction.east,acm.mainDir));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			DrawCorner ();
			DrawEdge (5, w, _pos, acm.Turn (Direction.south,acm.mainDir));
			DrawCorner ();
			GUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
		}
	}
}
