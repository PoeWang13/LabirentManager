using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LabirentKatPart
{
    public Vector2Int labirentSize;
    public LabirentKatRooms katRooms;
}
public class Labirent_Manager : MonoBehaviour
{
    public Labirent_Kat_Part_Maker labirent_Kat_Part_Maker;
    public List<LabirentKatPart> labirentKats = new List<LabirentKatPart>();
    [SerializeField] private List<Labirent_Kat_Part_Maker> labirent_Kat_Part_Makers = new List<Labirent_Kat_Part_Maker>();
    private void Start()
    {
        for (int e = 0; e < labirentKats.Count; e++)
        {
            Instantiate(labirent_Kat_Part_Maker).LabirentKurulumBasla(this, labirentKats[e].labirentSize, labirentKats[e].katRooms);
            labirent_Kat_Part_Makers.Add(labirent_Kat_Part_Maker);
        }
    }
    public void LabirentKatPartBitti(int labirentOrder)
    {
        labirent_Kat_Part_Makers[labirentOrder].transform.Rotate(0, (labirentOrder % 4) * 90, 0);
        labirent_Kat_Part_Makers[labirentOrder].transform.position = new Vector3(0, (labirentOrder / 4) * 100, 0);
    }
}