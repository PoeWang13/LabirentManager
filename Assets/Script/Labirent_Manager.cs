using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LabirentKatPart
{
    public int labirentSize;
    public LabirentKatRooms katRooms = new LabirentKatRooms();
    public LabirentKatPart(int labirentSize, LabirentKatRooms katRooms)
    {
        this.labirentSize = labirentSize;
        this.katRooms.bossRooms = katRooms.bossRooms;
        this.katRooms.savasRooms = katRooms.savasRooms;
        this.katRooms.hazineRooms = katRooms.hazineRooms;
    }
}
public class Labirent_Manager : MonoBehaviour
{
    public Labirent_Kat_Part_Maker labirent_Kat_Part_Maker;
    public List<LabirentKatPart> labirentKats = new List<LabirentKatPart>();
    [SerializeField] private List<Labirent_Kat_Part_Maker> labirent_Kat_Part_Makers = new List<Labirent_Kat_Part_Maker>();
    [SerializeField] private List<Vector3> labirent_Kat_Part_Offsets = new List<Vector3>();
    private void Start()
    {
        for (int e = 0; e < labirentKats.Count * 4; e++)
        {
            Labirent_Kat_Part_Maker labirent = Instantiate(labirent_Kat_Part_Maker);
            labirent.LabirentKurulumBasla(this, labirentKats[e / 4], e);
            labirent_Kat_Part_Makers.Add(labirent);
        }
    }
    public void LabirentKatPartBitti(int labirentOrder, int labirentScale)
    {
        labirent_Kat_Part_Makers[labirentOrder].transform.Rotate(0, labirentOrder % 4 * 90, 0);
        if (labirentOrder % 4 == 0)
        {
            labirent_Kat_Part_Makers[labirentOrder].transform.position = labirent_Kat_Part_Offsets[labirentOrder % 4] 
                + new Vector3(0, labirentOrder / 4 * 100, 0);
        }
        else if (labirentOrder % 4 == 1)
        {
            labirent_Kat_Part_Makers[labirentOrder].transform.position = labirent_Kat_Part_Offsets[labirentOrder % 4] 
                + new Vector3(0, labirentOrder / 4 * 100, -labirentScale);
        }
        else if (labirentOrder % 4 == 2)
        {
            labirent_Kat_Part_Makers[labirentOrder].transform.position = labirent_Kat_Part_Offsets[labirentOrder % 4] 
                + new Vector3(-labirentScale, labirentOrder / 4 * 100, -labirentScale);
        }
        else if (labirentOrder % 4 == 3)
        {
            labirent_Kat_Part_Makers[labirentOrder].transform.position = labirent_Kat_Part_Offsets[labirentOrder % 4] 
                + new Vector3(-labirentScale, labirentOrder / 4 * 100, 0);
        }
    }
}