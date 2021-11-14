using UnityEngine;

public class LabirentCell : MonoBehaviour
{
    public bool isRoom;
    public bool isUsed;
    public GameObject wallUp;
    public GameObject wallRight;
    public GameObject wallDown;
    public GameObject wallLeft;
    public Vector3Int startingKoor;
    public Vector2Int roomSize;
}