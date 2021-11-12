using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour
{
    public Transform labirentWall;
    public LabirentCell labirentCell;
    public Transform labirentZemin;
    public Vector2Int labirentBuyukluk;
    public int odaKenarAmount = 2;
    public int labirentKenarKoridorAmount = 3;
    public Vector2Int hazineRoom = new Vector2Int(2, 2);
    public Vector2Int savasRoom = new Vector2Int(3, 3);
    public Vector2Int bossRoom = new Vector2Int(4, 4);
    public List<LabirentKat> katlar = new List<LabirentKat>();
    private Dictionary<Vector3Int, LabirentCell> cells = new Dictionary<Vector3Int, LabirentCell>();
    public float labirentKurulumTime = 5;
    public float labirentKurulumTimeNext;
    public bool bossRoomKuruluyor = true;
    public bool savasRoomKuruluyor = true;
    public bool hazineRoomKuruluyor = true;
    public int bossRoomAmount;
    public int savasRoomAmount;
    public int hazineRoomAmount;

    private void Start()
    {
        int bossAlan = katlar[0].allRooms.x * (bossRoom.x + 2) * (bossRoom.x + 2);
        int savasAlan = katlar[0].allRooms.y * (savasRoom.x + 2) * (savasRoom.x + 2);
        int hazineAlan = katlar[0].allRooms.z * (hazineRoom.x + 2) * (hazineRoom.x + 2);
        if ((labirentBuyukluk.x - 3) * (labirentBuyukluk.y - 3) * 4 < bossAlan + savasAlan + hazineAlan)
        {
            Debug.Log("Labirent Kücük. " + Mathf.CeilToInt(Mathf.Sqrt(bossAlan + savasAlan + hazineAlan)) * 2 + " kenarlı bir yer tavsiye ediyoruz.");
        }
        DuvarKur();
    }
    private void DuvarKur()
    {
        // Labirent Zemini ayarla
        labirentZemin.localScale = new Vector3(labirentBuyukluk.x * 40, 1, labirentBuyukluk.y * 40);
        labirentZemin.localPosition = new Vector3(-10, 0, -15);
        // Labirent Duvarlarını ayarla
        for (int x = -labirentBuyukluk.x; x < labirentBuyukluk.x; x++)
        {
            for (int y = -labirentBuyukluk.y; y < labirentBuyukluk.y; y++)
            {
                LabirentCell cell = Instantiate(labirentCell, new Vector3Int(x * 20, 0, y * 20), Quaternion.identity, transform);
                cell.name = "" + (x * 20) + "___" + (y * 20);
                cells.Add(new Vector3Int(x * 20, 0, y * 20), cell);
                if (x == labirentBuyukluk.x - 1)
                {
                    Transform wall = Instantiate(labirentWall, new Vector3(x * 20, 0, y * 20), Quaternion.identity, cell.transform);
                    wall.localPosition = new Vector3(10, 0, 0);
                    wall.GetChild(0).localScale = new Vector3(1, 10, 20);
                    cell.wallRight = wall.gameObject;
                }
                if (y == labirentBuyukluk.y - 1)
                {
                    Transform wall = Instantiate(labirentWall, new Vector3(x * 20, 0, y * 20), Quaternion.identity, cell.transform);
                    wall.localPosition = new Vector3(0, 0, 10);
                    wall.GetChild(0).localScale = new Vector3(20, 10, 1);
                    cell.wallUp = wall.gameObject;
                }
                if (x == -labirentBuyukluk.x && y == -labirentBuyukluk.y)
                {
                    cell.wallDown.SetActive(false);
                    cell.wallLeft.SetActive(false);
                }
                if (x < -labirentBuyukluk.x + labirentKenarKoridorAmount || x > labirentBuyukluk.x - labirentKenarKoridorAmount)
                {
                    cell.isRoom = true;
                }
                if (y < -labirentBuyukluk.y + labirentKenarKoridorAmount || y > labirentBuyukluk.y - labirentKenarKoridorAmount)
                {
                    cell.isRoom = true;
                }
            }
        }
        BossRoomKur();
    }
    private Vector2Int RandomCoordinat(int odaSize)
    {
        int rndStartX = Random.Range(-labirentBuyukluk.x + labirentKenarKoridorAmount, labirentBuyukluk.x - labirentKenarKoridorAmount - odaSize - odaKenarAmount) * 20;
        int rndStartY = Random.Range(-labirentBuyukluk.y + labirentKenarKoridorAmount, labirentBuyukluk.y - labirentKenarKoridorAmount - odaSize - odaKenarAmount) * 20;
        return new Vector2Int(rndStartX, rndStartY);
    }
    private void UseCellForRoom(int roomX, int roomY, Vector2Int rndCoordinat)
    {
        for (int h = -odaKenarAmount; h < roomX + odaKenarAmount; h++)
        {
            for (int c = -odaKenarAmount; c < roomY + odaKenarAmount; c++)
            {
                if (h > 0 && h < roomX)
                {
                    if (c >= 0 && c < roomY)
                    {
                        cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(h * 20, 0, c * 20)]
                            .wallLeft.SetActive(false);
                    }
                }
                if (c > 0 && c < roomY)
                {
                    if (h >= 0 && h < roomX)
                    {
                        cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(h * 20, 0, c * 20)]
                            .wallDown.SetActive(false);
                    }
                }
                cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(h * 20, 0, c * 20)].isRoom = true;
            }
        }
    }
    private void BossRoomKur()
    {
        StartCoroutine(BossRoomYerBulunuyor());
    }
    IEnumerator BossRoomYerBulunuyor()
    {
        bossRoomAmount = 0;
        // Labirent Odalarını ayarlamaya basla - İlk Boss odasını yerleştir.
        Vector2Int rndCoordinat = RandomCoordinat(bossRoom.x);
        UseCellForRoom(bossRoom.x, bossRoom.y, rndCoordinat);
        bossRoomAmount = 1;
        while (bossRoomKuruluyor)
        {
            // Labirent Boss Odalarını ayarla
            rndCoordinat = RandomCoordinat(bossRoom.x);
            bool yerBuldum = true;
            for (int x = -odaKenarAmount; x < bossRoom.x + odaKenarAmount && yerBuldum; x++)
            {
                for (int y = -odaKenarAmount; y < bossRoom.y + odaKenarAmount && yerBuldum; y++)
                {
                    if (cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(x * 20, 0, y * 20)].isRoom)
                    {
                        yerBuldum = false;
                    }
                }
            }
            if (yerBuldum)
            {
                Debug.Log("Boss Kuruldu");
                UseCellForRoom(bossRoom.x, bossRoom.y, rndCoordinat);
                bossRoomAmount++;
                if (bossRoomAmount == katlar[0].allRooms.x)
                {
                    bossRoomKuruluyor = false;
                    SavasRoomKur();
                }
            }
            labirentKurulumTimeNext += Time.deltaTime;
            if (labirentKurulumTimeNext > labirentKurulumTime)
            {
                // Kurulum gerçekleşmedi sil baştan yapalim.
                bossRoomKuruluyor = false;
                SavasRoomKur();
            }
            yield return null;
        }
    }
    private void SavasRoomKur()
    {
        StartCoroutine(SavasRoomYerBulunuyor());
    }
    IEnumerator SavasRoomYerBulunuyor()
    {
        labirentKurulumTimeNext = 0;
        savasRoomAmount = 0;
        while (savasRoomKuruluyor)
        {
            // Labirent Savas Odalarını ayarla
            Vector2Int rndCoordinat = RandomCoordinat(savasRoom.x);
            bool yerBuldum = true;
            for (int x = -odaKenarAmount; x < savasRoom.x + odaKenarAmount && yerBuldum; x++)
            {
                for (int y = -odaKenarAmount; y < savasRoom.y + odaKenarAmount && yerBuldum; y++)
                {
                    if (cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(x * 20, 0, y * 20)].isRoom)
                    {
                        yerBuldum = false;
                    }
                }
            }
            if (yerBuldum)
            {
                Debug.Log("Savas Kuruldu");
                UseCellForRoom(savasRoom.x, savasRoom.y, rndCoordinat);
                savasRoomAmount++;
                if (savasRoomAmount == katlar[0].allRooms.y + 1)
                {
                    savasRoomKuruluyor = false;
                    HazineRoomKur();
                }
            }
            labirentKurulumTimeNext += Time.deltaTime;
            if (labirentKurulumTimeNext > labirentKurulumTime)
            {
                // Kurulum gerçekleşmedi sil baştan yapalim.
                savasRoomKuruluyor = false;
                HazineRoomKur();
            }
            yield return null;
        }
    }
    private void HazineRoomKur()
    {
        StartCoroutine(HazineRoomYerBulunuyor());
    }
    IEnumerator HazineRoomYerBulunuyor()
    {
        hazineRoomAmount = 0;
        labirentKurulumTimeNext = 0;
        while (hazineRoomKuruluyor)
        {
            // Labirent Hazine Odalarını ayarla
            Vector2Int rndCoordinat = RandomCoordinat(hazineRoom.x);
            bool yerBuldum = true;
            for (int x = -odaKenarAmount; x < hazineRoom.x + odaKenarAmount && yerBuldum; x++)
            {
                for (int y = -odaKenarAmount; y < hazineRoom.y + odaKenarAmount && yerBuldum; y++)
                {
                    if (cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(x * 20, 0, y * 20)].isRoom)
                    {
                        yerBuldum = false;
                    }
                }
            }
            if (yerBuldum)
            {
                Debug.Log("Hazine Kuruldu");
                UseCellForRoom(hazineRoom.x, hazineRoom.y, rndCoordinat);
                hazineRoomAmount++;
                if (hazineRoomAmount == katlar[0].allRooms.z)
                {
                    hazineRoomKuruluyor = false;
                    // Herşey hazır
                    LabirentKuruldu();
                }
            }
            labirentKurulumTimeNext += Time.deltaTime;
            if (labirentKurulumTimeNext > labirentKurulumTime)
            {
                labirentKurulumTimeNext = 0;
                // Kurulum gerçekleşmedi sil baştan yapalim.
                hazineRoomKuruluyor = false;
                BossRoomKontrol();
            }
            yield return null;
        }
    }
    private void BossRoomKontrol()
    {
        bool bosslarKuruldu = true;
        if (bossRoomAmount != katlar[0].allRooms.x)
        {
            bosslarKuruldu = false;
            for (int x = odaKenarAmount; x < labirentBuyukluk.x - odaKenarAmount && !bosslarKuruldu; x++)
            {
                for (int y = odaKenarAmount; y < labirentBuyukluk.y - odaKenarAmount && !bosslarKuruldu; y++)
                {
                    Vector3Int rndCoordinat = new Vector3Int(x * 20, 0, y * 20);
                    if (!cells[rndCoordinat].isRoom)
                    {
                        bool yerBuldum = true;
                        for (int e = 0; e < 8 && yerBuldum; e++)
                        {
                            for (int h = 0; h < 8 && yerBuldum; h++)
                            {
                                if (cells[rndCoordinat + new Vector3Int(e * 20, 0, h * 20)].isRoom)
                                {
                                    yerBuldum = false;
                                }
                            }
                        }
                        if (yerBuldum)
                        {
                            bossRoomAmount++;
                            UseCellForRoom(4, 4, new Vector2Int(rndCoordinat.x, rndCoordinat.z));
                            if (bossRoomAmount == katlar[0].allRooms.x)
                            {
                                bosslarKuruldu = true;
                            }
                        }
                    }
                }
            }
        }
        if (bosslarKuruldu)
        {
            Debug.Log("bosslarKuruldu");
            SavasRoomKontrol();
        }
        else
        {
            Debug.Log("bosslarKuruldu");
            LabirentKurulamadi();
        }
    }
    private void SavasRoomKontrol()
    {
        bool savaslarKuruldu = true;
        if (savasRoomAmount != katlar[0].allRooms.y + 1)
        {
            savaslarKuruldu = false;
            for (int x = odaKenarAmount; x < labirentBuyukluk.x - odaKenarAmount && !savaslarKuruldu; x++)
            {
                for (int y = odaKenarAmount; y < labirentBuyukluk.y - odaKenarAmount && !savaslarKuruldu; y++)
                {
                    Vector3Int rndCoordinat = new Vector3Int(x * 20, 0, y * 20);
                    if (!cells[rndCoordinat].isRoom)
                    {
                        bool yerBuldum = true;
                        for (int e = 0; e < 7 && yerBuldum; e++)
                        {
                            for (int h = 0; h < 7 && yerBuldum; h++)
                            {
                                if (cells[rndCoordinat + new Vector3Int(e * 20, 0, h * 20)].isRoom)
                                {
                                    yerBuldum = false;
                                }
                            }
                        }
                        if (yerBuldum)
                        {
                            savasRoomAmount++;
                            UseCellForRoom(3, 3, new Vector2Int(rndCoordinat.x, rndCoordinat.z));
                            if (savasRoomAmount == katlar[0].allRooms.y + 1)
                            {
                                savaslarKuruldu = true;
                            }
                        }
                    }
                }
            }
        }
        if (savaslarKuruldu)
        {
            Debug.Log("savaslarKuruldu");
            HazineRoomKontrol();
        }
        else
        {
            Debug.Log("savaslarKuruldu");
            LabirentKurulamadi();
        }
    }
    private void HazineRoomKontrol()
    {
        bool hazinelarKuruldu = true;
        if (hazineRoomAmount != katlar[0].allRooms.z)
        {
            hazinelarKuruldu = false;
            for (int x = odaKenarAmount; x < labirentBuyukluk.x - odaKenarAmount && !hazinelarKuruldu; x++)
            {
                for (int y = odaKenarAmount; y < labirentBuyukluk.y - odaKenarAmount && !hazinelarKuruldu; y++)
                {
                    Vector3Int rndCoordinat = new Vector3Int(x * 20, 0, y * 20);
                    if (!cells[rndCoordinat].isRoom)
                    {
                        bool yerBuldum = true;
                        for (int e = 0; e < 6 && yerBuldum; e++)
                        {
                            for (int h = 0; h < 6 && yerBuldum; h++)
                            {
                                if (cells[rndCoordinat + new Vector3Int(e * 20, 0, h * 20)].isRoom)
                                {
                                    yerBuldum = false;
                                }
                            }
                        }
                        if (yerBuldum)
                        {
                            hazineRoomAmount++;
                            UseCellForRoom(2, 2, new Vector2Int(rndCoordinat.x, rndCoordinat.z));
                            if (hazineRoomAmount == katlar[0].allRooms.z)
                            {
                                hazinelarKuruldu = true;
                            }
                        }
                    }
                }
            }
        }
        if (hazinelarKuruldu)
        {
            Debug.Log("hazinelarKuruldu");
            LabirentKuruldu();
        }
        else
        {
            Debug.Log("hazinelarKuruldu");
            LabirentKurulamadi();
        }
    }
    private void LabirentKuruldu()
    {

    }
    private void LabirentKurulamadi()
    {
        for (int e = transform.childCount - 1; e > 0; e++)
        {
            Destroy(transform.GetChild(e));
        }
        DuvarKur();
    }
}