using UnityEngine;
using UnityEditor;

namespace CreVox
{

	public class BoxCursorUtils
	{

		public static GameObject CreateBoxCursor(Transform _Parent, Vector3 _CursorSize)
		{
			GameObject bCursor;
			BoxCursor cur = EditorUtils.GetAssetsWithScript<BoxCursor>(PathCollect.editorPath)[0];
			bCursor = PrefabUtility.InstantiatePrefab(cur.gameObject) as GameObject;
			bCursor.transform.SetParent(_Parent);
			bCursor.transform.localScale = _CursorSize;
//			bCursor.hideFlags = HideFlags.HideInHierarchy;
			UpdateBox(bCursor, _Parent.position, Vector3.up);
			return bCursor;
		}

		public static void UpdateBox(GameObject box, Vector3 _pos, Vector3 _dir)
		{
			box.transform.position = _pos;
		
			//切換箭頭顯示方向
			BoxCursor dir = box.GetComponent<BoxCursor>();
			dir.Center.SetActive(_dir == Vector3.zero);
			dir.Xplus.SetActive(_dir.x > 0);
			dir.Xminor.SetActive(_dir.x < 0);
			dir.Yplus.SetActive(_dir.y > 0);
			dir.Yminor.SetActive(_dir.y < 0);
			dir.Zplus.SetActive(_dir.z > 0);
			dir.Zminor.SetActive(_dir.z < 0);
		}
	}
}