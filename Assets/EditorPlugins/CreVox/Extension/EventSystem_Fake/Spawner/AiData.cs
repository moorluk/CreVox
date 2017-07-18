using UnityEngine;

[CreateAssetMenu(menuName = "Setting/AiData")]
public class AiData : ScriptableObject
{
    public float toggle;
    public float eye;
    public float ear;

    public Vector3 toggleOffset;
    public Vector4[] toggleOffsets;
}
