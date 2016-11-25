using UnityEngine;
//using UnityEditor;

namespace CreVox
{

	public class BoxCursorUtils
	{

		public static GameObject CreateBoxCursor(Transform _Parent, Vector3 _CursorSize)
		{
			GameObject bCursor;
//			#if UNITY_EDITOR
//			BoxCursor cur = EditorUtils.GetAssetsWithScript<BoxCursor> (PathCollect.assetsPath) [0];
			bCursor = GameObject.Instantiate(Resources.Load<GameObject>(PathCollect.box));
			bCursor.transform.SetParent(_Parent);
			bCursor.transform.localScale = _CursorSize;
			bCursor.transform.localRotation = Quaternion.Inverse (_Parent.rotation);
			UpdateBox(bCursor, _Parent.position, Vector3.zero);
			return bCursor;
//			#else
//			return null;
//			#endif
		}

		public static void UpdateBox(GameObject box, Vector3 _pos, Vector3 _dir)
		{
			box.transform.position = _pos;
//			Debug.Log (_dir);
		
			//切換箭頭顯示方向
			BoxCursor dir = box.GetComponent<BoxCursor>();
			dir.Center.SetActive(_dir == Vector3.zero);
			dir.Xplus.SetActive(_dir.x > 0.5f);
			dir.Xminor.SetActive(_dir.x < -0.5f);
			dir.Yplus.SetActive(_dir.y > 0.5f);
			dir.Yminor.SetActive(_dir.y < -0.5f);
			dir.Zplus.SetActive(_dir.z > 0.5f);
			dir.Zminor.SetActive(_dir.z < -0.5f);
		}
	}
}