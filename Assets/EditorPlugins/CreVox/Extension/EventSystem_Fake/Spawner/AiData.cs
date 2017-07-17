using UnityEngine;

[CreateAssetMenu(menuName = "Setting/AiData")]
public class AiData : ScriptableObject
{
    public int toggle;
    public int eye;
    public int ear;

    public Vector3 toggleOffset;
    public Vector3[] toggleOffsets;
}
