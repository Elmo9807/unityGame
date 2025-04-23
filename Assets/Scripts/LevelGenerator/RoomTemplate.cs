using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "Level Generation/Room Template")]
public class RoomTemplate : ScriptableObject
{
    public GameObject roomPrefab;
    public bool hasTopExit;
    public bool hasBottomExit;
    public bool hasLeftExit;
    public bool hasRightExit;
}
