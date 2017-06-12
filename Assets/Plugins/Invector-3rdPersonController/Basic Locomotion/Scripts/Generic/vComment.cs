using Invector;
using UnityEngine;

[vClassHeader("vComment",false, "icon_v2")]
public class vComment : vMonoBehaviour
{
	#if UNITY_EDITOR
	[TextAreaAttribute (5, 3000)]
	public string comment;
	#endif
}
