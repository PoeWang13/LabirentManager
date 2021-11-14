using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LabirentKatRooms
{
    [Header("X=Boss Room-Y=Savas Room-Z=Hazine Room")]
    public Vector2Int bossRooms;
    public Vector2Int savasRooms;
    public Vector2Int hazineRooms;
}
public class Labirent_Kat_Part_Maker : MonoBehaviour
{
    [Header("Atamalar")]
    [SerializeField] private Labirent_Manager labirentManager;
    [SerializeField] private Transform labirentZemin;
    [SerializeField] private Transform labirentWall;
    [SerializeField] private LabirentCell labirentCell;
    [SerializeField] private GameObject labirentTeleporter;
    [SerializeField] private int labirentOrder;
    [Header("Labirent Buyukluk")]
    [Header("X=Oda kenar-Y=Labirent kenar")]
    [SerializeField] private Vector2Int kenarLimitler = new Vector2Int(2, 3);
    [Header("Room size")]
    [SerializeField] private Vector2Int hazineRoom = new Vector2Int(2, 2);
    [SerializeField] private Vector2Int savasRoom = new Vector2Int(3, 3);
    [SerializeField] private Vector2Int bossRoom = new Vector2Int(4, 4);
    [Header("Kurulacak Kat. Içinde X=Kurulacak oda amount, Y=Kurulan oda amount")]
    [SerializeField] private LabirentKatPart kat;
    [Header("X=Kurulum time limit-Y=Kurulum time limit next")]
    [SerializeField] private Vector2 kurulumTime = new Vector2(2, 3);
    private List<Vector2Int> savasOdalar = new List<Vector2Int>();
    private Dictionary<Vector3Int, LabirentCell> cells = new Dictionary<Vector3Int, LabirentCell>();
    private List<Vector3Int> gidilenYollar = new List<Vector3Int>();
    private Vector3Int currentCellKoor;
    private Vector3Int newKoor;
    public void LabirentKurulumBasla(Labirent_Manager manager, LabirentKatPart kat, int labirentOrder)
    {
        labirentManager = manager;
        this.kat = new LabirentKatPart(kat.labirentSize, kat.katRooms);
        this.labirentOrder = labirentOrder;
        currentCellKoor = new Vector3Int(-kat.labirentSize * 20, 0, -kat.labirentSize * 20);
        DuvarKur();
    }
    private void DuvarKur()
    {
        int bossAlan = (kat.katRooms.bossRooms.x + 4) * (kat.katRooms.bossRooms.y + 4);
        int savasAlan = (kat.katRooms.savasRooms.x + 4) * (kat.katRooms.savasRooms.y + 4);
        int hazineAlan = (kat.katRooms.hazineRooms.y + 4) * (kat.katRooms.hazineRooms.y + 4);
        if ((kat.labirentSize - 3) * (kat.labirentSize - 3) * 4 < bossAlan + savasAlan + hazineAlan)
        {
            Debug.Log("Labirent Kücük. " + Mathf.CeilToInt(Mathf.Sqrt(bossAlan + savasAlan + hazineAlan)) * 2 + " kenarlı bir yer tavsiye ediyoruz.");
        }
        // Labirent Zemini ayarla
        labirentZemin.localScale = new Vector3(kat.labirentSize * 40, 1, kat.labirentSize * 40);
        labirentZemin.localPosition = new Vector3(-10, 0, -15);
        // Labirent Duvarlarını ayarla
        for (int x = -kat.labirentSize; x < kat.labirentSize; x++)
        {
            for (int y = -kat.labirentSize; y < kat.labirentSize; y++)
            {
                LabirentCell cell = Instantiate(labirentCell, new Vector3Int(x * 20, 0, y * 20), Quaternion.identity, transform.GetChild(1));
                cell.name = "" + (x * 20) + "___" + (y * 20);
                cells.Add(new Vector3Int(x * 20, 0, y * 20), cell);
                if (x == kat.labirentSize - 1)
                {
                    Transform wall = Instantiate(labirentWall, new Vector3(x * 20, 0, y * 20), Quaternion.identity, cell.transform);
                    wall.localPosition = new Vector3(10, 5, -5);
                    wall.localScale = new Vector3(1, 10, 20);
                    //wall.GetChild(0).localScale = new Vector3(1, 10, 20);
                    cell.wallRight = wall.gameObject;
                }
                if (y == kat.labirentSize - 1)
                {
                    Transform wall = Instantiate(labirentWall, new Vector3(x * 20, 0, y * 20), Quaternion.identity, cell.transform);
                    wall.localPosition = new Vector3(0, 5, 5);
                    wall.localScale = new Vector3(20, 10, 1);
                    //wall.GetChild(0).localScale = new Vector3(20, 10, 1);
                    cell.wallUp = wall.gameObject;
                }
                if (x == -kat.labirentSize && y == -kat.labirentSize)
                {
                    cell.wallDown.SetActive(false);
                    cell.wallLeft.SetActive(false);
                }
                if (x < -kat.labirentSize + kenarLimitler.y || x > kat.labirentSize - kenarLimitler.y)
                {
                    cell.isRoom = true;
                }
                if (y < -kat.labirentSize + kenarLimitler.y || y > kat.labirentSize - kenarLimitler.y)
                {
                    cell.isRoom = true;
                }
            }
        }
        cells[currentCellKoor].isUsed = true;
        cells[currentCellKoor].transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        gidilenYollar.Add(currentCellKoor);
        BossRoomKur();
    }
    private Vector2Int RandomCoordinat(int odaSize)
    {
        int rndStartX = Random.Range(-kat.labirentSize + kenarLimitler.y, kat.labirentSize - kenarLimitler.y - odaSize - kenarLimitler.x) * 20;
        int rndStartY = Random.Range(-kat.labirentSize + kenarLimitler.y, kat.labirentSize - kenarLimitler.y - odaSize - kenarLimitler.x) * 20;
        return new Vector2Int(rndStartX, rndStartY);
    }
    private void UseCellForRoom(int roomX, int roomY, Vector2Int rndCoordinat)
    {
        for (int h = -kenarLimitler.x; h < roomX + kenarLimitler.x; h++)
        {
            for (int c = -kenarLimitler.x; c < roomY + kenarLimitler.x; c++)
            {
                Vector3Int koor = new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(h * 20, 0, c * 20);
                if (h > 0 && h < roomX)
                {
                    if (c >= 0 && c < roomY)
                    {
                        cells[koor].wallLeft.SetActive(false);
                        cells[koor].roomSize = new Vector2Int(roomX, roomY);
                        cells[koor].startingKoor = new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y);
                    }
                }
                if (c > 0 && c < roomY)
                {
                    if (h >= 0 && h < roomX)
                    {
                        cells[koor].wallDown.SetActive(false);
                        cells[koor].roomSize = new Vector2Int(roomX, roomY);
                        cells[koor].startingKoor = new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y);
                    }
                }
                if (h == 0 && c == 0)
                {
                    cells[koor].roomSize = new Vector2Int(roomX, roomY);
                    cells[koor].startingKoor = new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y);
                }
                cells[koor].isRoom = true;
            }
        }
    }
    private void BossRoomKur()
    {
        Debug.Log("Boss odaları kurulmaya baslandı.");
        StartCoroutine(BossRoomYerBulunuyor());
    }
    IEnumerator BossRoomYerBulunuyor()
    {
        // Labirent Odalarını ayarlamaya basla - İlk Boss odasını yerleştir.
        Vector2Int rndCoordinat = RandomCoordinat(bossRoom.x);
        UseCellForRoom(bossRoom.x, bossRoom.y, rndCoordinat);
        kat.katRooms.bossRooms.y = 1;
        if (kat.katRooms.bossRooms.x == 1)
        {
            SavasRoomKur();
        }
        while (kat.katRooms.bossRooms.x != kat.katRooms.bossRooms.y)
        {
            // Labirent Boss Odalarını ayarla
            rndCoordinat = RandomCoordinat(bossRoom.x);
            bool yerBuldum = true;
            for (int x = -kenarLimitler.x; x < bossRoom.x + kenarLimitler.x && yerBuldum; x++)
            {
                for (int y = -kenarLimitler.x; y < bossRoom.y + kenarLimitler.x && yerBuldum; y++)
                {
                    if (cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(x * 20, 0, y * 20)].isRoom)
                    {
                        yerBuldum = false;
                    }
                }
            }
            if (yerBuldum)
            {
                UseCellForRoom(bossRoom.x, bossRoom.y, rndCoordinat);
                kat.katRooms.bossRooms.y++;
                if (kat.katRooms.bossRooms.x == kat.katRooms.bossRooms.y)
                {
                    SavasRoomKur();
                }
            }
            kurulumTime.y += Time.deltaTime;
            if (kurulumTime.y > kurulumTime.x)
            {
                // Kurulum gerçekleşmedi sil baştan yapalim.
                BossRoomKontrol();
                break;
            }
            yield return null;
        }
    }
    private void BossRoomKontrol()
    {
        if (kat.katRooms.bossRooms.x != kat.katRooms.bossRooms.y)
        {
            bool bosslarKuruldu = false;
            for (int x = -kat.labirentSize + kenarLimitler.y; x < kat.labirentSize - kenarLimitler.y && !bosslarKuruldu; x++)
            {
                for (int y = -kat.labirentSize + kenarLimitler.y; y < kat.labirentSize - kenarLimitler.y && !bosslarKuruldu; y++)
                {
                    Vector2Int rndCoordinat = new Vector2Int(x * 20, y * 20);
                    if (!cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y)].isRoom)
                    {
                        bool yerBuldum = YerKontrol(rndCoordinat, 4);
                        if (yerBuldum)
                        {
                            Debug.Log("Boss icin yer buldum.");
                            kat.katRooms.bossRooms.y++;
                            UseCellForRoom(bossRoom.x, bossRoom.y, rndCoordinat + new Vector2Int(40, 40));
                            if (kat.katRooms.bossRooms.x == kat.katRooms.bossRooms.y)
                            {
                                Debug.Log("Boss odaları kuruldu.");
                                bosslarKuruldu = true;
                            }
                        }
                    }
                }
            }
        }
        SavasRoomKur();
    }
    private void SavasRoomKur()
    {
        Debug.Log("Savas odaları kurulmaya baslandı.");
        StartCoroutine(SavasRoomYerBulunuyor());
    }
    IEnumerator SavasRoomYerBulunuyor()
    {
        kurulumTime.y = 0;
        while (kat.katRooms.savasRooms.x + 1 != kat.katRooms.savasRooms.y)
        {
            // Labirent Savas Odalarını ayarla
            Vector2Int rndCoordinat = RandomCoordinat(savasRoom.x);
            bool yerBuldum = true;
            for (int x = -kenarLimitler.x; x < savasRoom.x + kenarLimitler.x && yerBuldum; x++)
            {
                for (int y = -kenarLimitler.x; y < savasRoom.y + kenarLimitler.x && yerBuldum; y++)
                {
                    if (cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(x * 20, 0, y * 20)].isRoom)
                    {
                        yerBuldum = false;
                    }
                }
            }
            if (yerBuldum)
            {
                UseCellForRoom(savasRoom.x, savasRoom.y, rndCoordinat);
                savasOdalar.Add(rndCoordinat + new Vector2Int(20, 20));
                kat.katRooms.savasRooms.y++;
                if (kat.katRooms.savasRooms.x + 1 == kat.katRooms.savasRooms.y)
                {
                    HazineRoomKur();
                }
            }
            kurulumTime.y += Time.deltaTime;
            if (kurulumTime.y > kurulumTime.x)
            {
                // Kurulum gerçekleşmedi sil baştan yapalim.
                SavasRoomKontrol();
                break;
            }
            yield return null;
        }
    }
    private void SavasRoomKontrol()
    {
        if (kat.katRooms.savasRooms.x + 1 != kat.katRooms.savasRooms.y)
        {
            bool savaslarKuruldu = false;
            for (int x = -kat.labirentSize + kenarLimitler.y; x < kat.labirentSize - kenarLimitler.y && !savaslarKuruldu; x++)
            {
                for (int y = -kat.labirentSize + kenarLimitler.y; y < kat.labirentSize - kenarLimitler.y && !savaslarKuruldu; y++)
                {
                    Vector2Int rndCoordinat = new Vector2Int(x * 20, y * 20);
                    if (!cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y)].isRoom)
                    {
                        bool yerBuldum = YerKontrol(rndCoordinat, 3);
                        if (yerBuldum)
                        {
                            Debug.Log("Savas icin yer buldum.");
                            kat.katRooms.savasRooms.y++;
                            UseCellForRoom(savasRoom.x, savasRoom.y, rndCoordinat + new Vector2Int(40, 40));
                            savasOdalar.Add(rndCoordinat + new Vector2Int(60, 60));
                            if (kat.katRooms.savasRooms.x + 1 == kat.katRooms.savasRooms.y)
                            {
                                Debug.Log("Savas odaları kuruldu.");
                                savaslarKuruldu = true;
                            }
                        }
                    }
                }
            }
        }
        HazineRoomKur();
    }
    private void HazineRoomKur()
    {
        Debug.Log("Hazine odaları kurulmaya baslandı.");
        StartCoroutine(HazineRoomYerBulunuyor());
    }
    IEnumerator HazineRoomYerBulunuyor()
    {
        kurulumTime.y = 0;
        while (kat.katRooms.hazineRooms.x != kat.katRooms.hazineRooms.y)
        {
            // Labirent Hazine Odalarını ayarla
            Vector2Int rndCoordinat = RandomCoordinat(hazineRoom.x);
            bool yerBuldum = true;
            for (int x = -kenarLimitler.x; x < hazineRoom.x + kenarLimitler.x && yerBuldum; x++)
            {
                for (int y = -kenarLimitler.x; y < hazineRoom.y + kenarLimitler.x && yerBuldum; y++)
                {
                    if (cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y) + new Vector3Int(x * 20, 0, y * 20)].isRoom)
                    {
                        yerBuldum = false;
                    }
                }
            }
            if (yerBuldum)
            {
                UseCellForRoom(hazineRoom.x, hazineRoom.y, rndCoordinat);
                kat.katRooms.hazineRooms.y++;
                if (kat.katRooms.hazineRooms.x == kat.katRooms.hazineRooms.y)
                {
                    // Herşey hazır
                    Debug.Log("Labirent kuruldu.");
                    LabirentKuruldu();
                }
            }
            kurulumTime.y += Time.deltaTime;
            if (kurulumTime.y > kurulumTime.x)
            {
                kurulumTime.y = 0;
                // Kurulum gerçekleşmedi sil baştan yapalim.
                HazineRoomKontrol();
                break;
            }
            yield return null;
        }
    }
    private void HazineRoomKontrol()
    {
        bool hazinelarKuruldu = true;
        if (kat.katRooms.hazineRooms.x != kat.katRooms.hazineRooms.y)
        {
            hazinelarKuruldu = false;
            for (int x = -kat.labirentSize + kenarLimitler.y; x < kat.labirentSize - kenarLimitler.y && !hazinelarKuruldu; x++)
            {
                for (int y = -kat.labirentSize + kenarLimitler.y; y < kat.labirentSize - kenarLimitler.y && !hazinelarKuruldu; y++)
                {
                    Vector2Int rndCoordinat = new Vector2Int(x * 20, y * 20);
                    if (!cells[new Vector3Int(rndCoordinat.x, 0, rndCoordinat.y)].isRoom)
                    {
                        bool yerBuldum = YerKontrol(rndCoordinat, 2);
                        if (yerBuldum)
                        {
                            Debug.Log("Hazine icin yer buldum.");
                            UseCellForRoom(hazineRoom.x, hazineRoom.y, rndCoordinat + new Vector2Int(40, 40));
                            kat.katRooms.hazineRooms.y++;
                            if (kat.katRooms.hazineRooms.x == kat.katRooms.hazineRooms.y)
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
            Debug.Log("Labirent kuruldu.");
            LabirentKuruldu();
        }
        else
        {
            LabirentKurulamadi();
        }
    }
    private void LabirentKurulamadi()
    {
        Debug.Log("Labirent tam olarak kurulamadı. Daha büyük bir labirent kurmayı deneyim.");
        LabirentKuruldu();
    }
    private void LabirentKuruldu()
    {
        LabirentYolYap();
    }
    public bool YerKontrol(Vector2Int kontrolYer, int room)
    {
        bool yerBuldum = true;
        for (int e = 0; e < room + 4 && yerBuldum; e++)
        {
            for (int h = 0; h < room + 4 && yerBuldum; h++)
            {
                if (cells[new Vector3Int(kontrolYer.x, 0, kontrolYer.y) + new Vector3Int(e * 20, 0, h * 20)].isRoom)
                {
                    yerBuldum = false;
                }
            }
        }
        return yerBuldum;
    }
    private (Vector3Int, int) YonBul(Vector3Int cellKoor)
    {
        newKoor = new Vector3Int(0, 0, 0);
        List<int> yeniYer = new List<int> { 0, 1, 2, 3 };
        while (yeniYer.Count > 0)
        {
            int rnd = yeniYer[Random.Range(0, yeniYer.Count)];
            yeniYer.Remove(rnd);
            if (rnd == 0) // ileri
            {
                newKoor = cellKoor + new Vector3Int(0, 0, 20);
                if (cells.TryGetValue(newKoor, out LabirentCell cell))
                {
                    if (!cell.isUsed)
                    {
                        return (newKoor, 0);
                    }
                }
            }
            else if (rnd == 1) // sag
            {
                newKoor = cellKoor + new Vector3Int(20, 0, 0);
                if (cells.TryGetValue(newKoor, out LabirentCell cell))
                {
                    if (!cell.isUsed)
                    {
                        return (newKoor, 1);
                    }
                }
            }
            else if (rnd == 2) // geri
            {
                newKoor = cellKoor + new Vector3Int(0, 0, -20);
                if (cells.TryGetValue(newKoor, out LabirentCell cell))
                {
                    if (!cell.isUsed)
                    {
                        return (newKoor, 2);
                    }
                }
            }
            else if (rnd == 3) // sol
            {
                newKoor = cellKoor + new Vector3Int(-20, 0, 0);
                if (cells.TryGetValue(newKoor, out LabirentCell cell))
                {
                    if (!cell.isUsed)
                    {
                        return (newKoor, 3);
                    }
                }
            }
        }
        return (cellKoor, -1);
    }
    private void LabirentYolYap()
    {
        StartCoroutine(LabirentYolMaking());
    }
    IEnumerator LabirentYolMaking()
    {
        bool yolBitti = false;
        while (!yolBitti)
        {
            (Vector3Int, int) newCellKoor = YonBul(currentCellKoor);
            if (currentCellKoor != newCellKoor.Item1) // Komsu yöne gidebildik.
            {
                // Yeni yolu bulduk. Sisteme kaydedip işaretle
                cells[newCellKoor.Item1].transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
                cells[newCellKoor.Item1].isUsed = true;
                gidilenYollar.Add(newCellKoor.Item1);
                // Yola giden duvarı sil
                if (newCellKoor.Item2 == 0)
                {
                    cells[newCellKoor.Item1].wallDown.SetActive(false);
                }
                else if (newCellKoor.Item2 == 1)
                {
                    cells[newCellKoor.Item1].wallLeft.SetActive(false);
                }
                else if (newCellKoor.Item2 == 2)
                {
                    cells[currentCellKoor].wallDown.SetActive(false);
                }
                else if (newCellKoor.Item2 == 3)
                {
                    cells[currentCellKoor].wallLeft.SetActive(false);
                }
                currentCellKoor = newCellKoor.Item1;
                // Yeni yol eğer 1 odaya aitse odanın tüm bölmelerini kullanılmış olarak işaretle
                if (cells[newCellKoor.Item1].isRoom)
                {
                    Debug.Log("<color=yellow>Oda buldum.</color>");
                    for (int h = 0; h < cells[newCellKoor.Item1].roomSize.x; h++)
                    {
                        for (int c = 0; c < cells[newCellKoor.Item1].roomSize.y; c++)
                        {
                            Debug.Log("<color=blue>Odayı işaretledim.</color>");
                            cells[cells[newCellKoor.Item1].startingKoor + new Vector3Int(h * 20, 0, c * 20)].isUsed = true;
                            cells[cells[newCellKoor.Item1].startingKoor + new Vector3Int(h * 20, 0, c * 20)].transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
                        }
                    }
                }
            }
            else // Gidilecek komsu yön yok
            {
                MeshRenderer mesh = cells[newCellKoor.Item1].transform.GetChild(0).GetComponent<MeshRenderer>();
                if (mesh.material.color == Color.red)
                {
                    Debug.Log("<color=red>Düzgün yol bitti geriye dönüyoruz.</color>");
                    mesh.material.color = Color.blue;
                }
                gidilenYollar.RemoveAt(gidilenYollar.Count - 1);
                if (gidilenYollar.Count != 0)
                {
                    currentCellKoor = gidilenYollar[gidilenYollar.Count - 1];
                }
                else
                {
                    Debug.Log("<color=green>Labirent yollar tamamiyle açıldı.</color>");
                    yolBitti = true;
                }
            }
            yield return null;
        }
        TeleportYap();
    }
    private void TeleportYap()
    {
        Debug.Log("<color=cyan>Labirent teleport yapılıyor.</color>");
        if (savasOdalar.Count > 0)
        {
            Vector2Int rndSavasOda = savasOdalar[Random.Range(0, savasOdalar.Count)];
            Instantiate(labirentTeleporter, new Vector3(rndSavasOda.x, 00, rndSavasOda.y - 5), Quaternion.identity, transform);
        }
        labirentManager.LabirentKatPartBitti(labirentOrder, kat.labirentSize * 40);
    }
}