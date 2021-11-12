using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
//public class LabirentOda
//{
//    public List<Vector2Int> roomSize = new List<Vector2Int>();
//    public List<Vector2Int> bossOdalar = new List<Vector2Int>();
//}
[System.Serializable]
public class LabirentKat
{
    [Header("X=Boss Room-Y=Savas Room-Z=Hazine Room")]
    public Vector3Int allRooms;
}
public class Labirent_Manager : MonoBehaviour
{
    /// Büyüklük        // Labirentin büyüklüğüdür.
    /// Baslangic       // Labirentin merkezi olacak
    /// Kat sayısı      // Labirent kaç katlı olacak
    /// Hazine odası    // Labirentteki hazine odaları sayısı - her kat için farklıdır.
    /// Gizli oda       // Labirentteki gizli odaların sayısı - her kat için farklıdır.
    /// Savas odasi     // Labirentte canavarlarla savaşmak için olan odalar
    /// Boss odasi      // Labirentteki bosslardır. Kimisi hazineleri korur, kimisi yardımcı olacak şeyler verir.
    /// Koridorlar      // Odalar ve merdivenler dışındaki yerlerdir. Tuzaklarla doludur.

    public Transform labirentWall;
    public LabirentCell labirentCell;
    public Transform labirentZemin;
    public Vector2Int labirentBuyukluk;
    public int labirentOdaOlusumEngelKenarAmount;
    public int labirentOdaOlusumKenarKoridorAmount;
    public Vector2Int hazineRoom;
    public Vector2Int savasRoom;
    public Vector2Int bossRoom;
    public List<LabirentKat> katlar = new List<LabirentKat>();
    private Dictionary<Vector3Int, LabirentCell> cells = new Dictionary<Vector3Int, LabirentCell>();
    public float labirentKurulumTime;
    private float labirentKurulumTimeNext;
    private bool labirentKuruldu;
    private bool labirentYokEdiliyor;
    private Vector3Int allRooms;
    private void Start()
    {
        int labAlan = (labirentBuyukluk.x - labirentOdaOlusumEngelKenarAmount) * (labirentBuyukluk.x - labirentOdaOlusumEngelKenarAmount);
        int roomsAlan = (hazineRoom.x + labirentOdaOlusumKenarKoridorAmount) * (hazineRoom.x + labirentOdaOlusumKenarKoridorAmount) + (savasRoom.y + labirentOdaOlusumKenarKoridorAmount) * (savasRoom.y + labirentOdaOlusumKenarKoridorAmount) + (bossRoom.y + labirentOdaOlusumKenarKoridorAmount) * (bossRoom.y + labirentOdaOlusumKenarKoridorAmount);

        Debug.Log(labAlan);
        Debug.Log(roomsAlan);
        if (labAlan < roomsAlan)
        {
            Debug.Log("Bu labirent Kurulamaz.");
            return;
        }
        else if (labAlan < roomsAlan * 3)
        {
            Debug.Log("Bu labirentin çok az koridor alanı var. " + Mathf.CeilToInt(Mathf.Sqrt(roomsAlan * 3) + labirentOdaOlusumEngelKenarAmount) + " alanlı bir labirent öneririm.");
        }
        LabirentDuvarlarKur();
        LabirentOdalarKur();
    }
    private void LabirentDuvarlarKur()
    {
        labirentYokEdiliyor = false;
        labirentKuruldu = false;
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
                if (x < -labirentBuyukluk.x + labirentOdaOlusumEngelKenarAmount || x > labirentBuyukluk.x - labirentOdaOlusumEngelKenarAmount)
                {
                    cell.isRoom = true;
                }
                if (y < -labirentBuyukluk.y + labirentOdaOlusumEngelKenarAmount || y > labirentBuyukluk.y - labirentOdaOlusumEngelKenarAmount)
                {
                    cell.isRoom = true;
                }
            }
        }
    }
    private void LabirentOdalarKur()
    {
        StartCoroutine(RoomsPrepared());
    }
    private Vector2Int RandomCoordinat()
    {
        int rndStartX = Random.Range(-labirentBuyukluk.x + labirentOdaOlusumEngelKenarAmount, labirentBuyukluk.x - labirentOdaOlusumEngelKenarAmount - bossRoom.x) * 20;
        int rndStartY = Random.Range(-labirentBuyukluk.y + labirentOdaOlusumEngelKenarAmount, labirentBuyukluk.y - labirentOdaOlusumEngelKenarAmount - bossRoom.y) * 20;
        return new Vector2Int(rndStartX, rndStartY);
    }
    private void UseCellForRoom(int roomX, int roomY, Vector2Int rndCoordinat)
    {
        for (int h = -1; h < roomX + 1; h++)
        {
            for (int c = -1; c < roomY + 1; c++)
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
    IEnumerator RoomsPrepared()
    {
        int roomCount = 0;
        // Labirent Katlarını Ayarla
        for (int e = 0; e < katlar.Count; e++)
        {
            Debug.Log(allRooms);
            roomCount = 0;
            // Labirent Odalarını ayarlamaya basla - İlk Boss odasını yerleştir.
            Vector2Int rndCoordinat = RandomCoordinat();
            UseCellForRoom(bossRoom.x, bossRoom.y, rndCoordinat);
            roomCount++;
            // Labirent Boss Odalarını ayarla
            roomCount = 1;
            allRooms.x = 1;
            while (!labirentYokEdiliyor && roomCount < katlar[e].allRooms.x)
            {
                rndCoordinat = RandomCoordinat();
                bool yerBuldum = true;
                for (int x = -2; x < bossRoom.x + 2 && yerBuldum; x++)
                {
                    for (int y = -2; y < bossRoom.y + 2 && yerBuldum; y++)
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
                    roomCount++;
                    allRooms.x++;
                }
                yield return null;
            }
            Debug.Log("Boss Kurulum bitti");
            // Labirent Boss Odalarını ayarla
            roomCount = 0;
            allRooms.y = 0;
            while (!labirentYokEdiliyor && roomCount < katlar[e].allRooms.y + 1)
            {
                rndCoordinat = RandomCoordinat();
                bool yerBuldum = true;
                for (int x = -2; x < savasRoom.x + 2 && yerBuldum; x++)
                {
                    for (int y = -2; y < savasRoom.y + 2 && yerBuldum; y++)
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
                    roomCount++;
                    allRooms.y++;
                }
                yield return null;
            }
            Debug.Log("Savas Kurulum bitti");
            //Labirent Hazine Odalarını ayarla
            roomCount = 0;
            allRooms.z = 0;
            while (!labirentYokEdiliyor && roomCount < katlar[e].allRooms.z)
            {
                rndCoordinat = RandomCoordinat();
                bool yerBuldum = true;
                for (int x = -2; x < hazineRoom.x + 2 && yerBuldum; x++)
                {
                    for (int y = -2; y < hazineRoom.y + 2 && yerBuldum; y++)
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
                    roomCount++;
                    allRooms.z++;
                }
                yield return null;
            }
            Debug.Log("Hazine Kurulum bitti");
            if (katlar[e].allRooms.x == allRooms.x && katlar[e].allRooms.y + 1 == allRooms.y && katlar[e].allRooms.z == allRooms.z)
            {
                Debug.Log("Kurulum bitti");
                labirentKuruldu = true;
            }
        }
    }
    IEnumerator OdaKur(int roomLimit, int roomSize)
    {
        int roomCount = 0;
        bool bitti = false;
        for (int x = -labirentBuyukluk.x + 2; x < labirentBuyukluk.x - 2 && !bitti; x++)
        {
            for (int y = -labirentBuyukluk.y + 2; y < labirentBuyukluk.y - 2 && !bitti; y++)
            {
                if (!cells[new Vector3Int(x * 20, 0, y * 20)].isRoom)
                {
                    bool bittiMusait = true;
                    Vector2Int coor = new Vector2Int(x * 20, y * 20);
                    for (int e = 0; e < roomSize + 2 && bittiMusait; e++)
                    {
                        for (int h = 0; h < roomSize + 2 && bittiMusait; h++)
                        {
                            if (cells[new Vector3Int(x * 20, 0, y * 20) + new Vector3Int(e * 20, 0, h * 20)].isRoom)
                            {
                                bittiMusait = false;
                            }
                        }
                    }
                    if (bittiMusait)
                    {
                        UseCellForRoom(hazineRoom.x, hazineRoom.y, coor + new Vector2Int(20, 20));
                        roomCount++;
                        if (roomCount == roomLimit)
                        {
                            bitti = true;
                        }
                    }
                }
                yield return null;
            }
        }
    }
    private void LabirentiTara()
    {
        labirentKuruldu = true;
        if (katlar[0].allRooms.x - allRooms.x > 0)
        {
            Debug.Log("Boss Kurulum basliyor");
            Debug.Log(katlar[0].allRooms.x - allRooms.x);
            StartCoroutine(OdaKur(katlar[0].allRooms.x - allRooms.x, bossRoom.x));
        }
        if (katlar[0].allRooms.y + 1 - allRooms.y > 0)
        {
            Debug.Log("Savas Kurulum basliyor");
            Debug.Log(katlar[0].allRooms.y - allRooms.y);
            StartCoroutine(OdaKur(katlar[0].allRooms.y - allRooms.y, savasRoom.x));
        }
        if (katlar[0].allRooms.z - allRooms.z > 0)
        {
            Debug.Log("Hazine Kurulum basliyor");
            Debug.Log(katlar[0].allRooms.z - allRooms.z);
            StartCoroutine(OdaKur(katlar[0].allRooms.z - allRooms.z, hazineRoom.x));
            Debug.Log(katlar[0].allRooms.z - allRooms.z);
        }
        if (katlar[0].allRooms.x - allRooms.x > 0 || katlar[0].allRooms.y - allRooms.y > 0 || katlar[0].allRooms.z - allRooms.z > 0)
        {
            Debug.Log(katlar[0].allRooms.x - allRooms.x > 0);
            Debug.Log(katlar[0].allRooms.y - allRooms.y > 0);
            Debug.Log(katlar[0].allRooms.z - allRooms.z > 0);
            allRooms = new Vector3Int(0, 0, 0);
            LabirentiYokEt();
        }
    }
    private void LabirentiYokEt()
    {
        for (int e = transform.childCount - 1; e > 0; e--)
        {
            Destroy(transform.GetChild(e).gameObject);
        }
        Debug.Log("Labirenti Yok Et");
        cells.Clear();
        labirentKuruldu = false;
        LabirentDuvarlarKur();
        LabirentOdalarKur();
    }
    private void Update()
    {
        if (!labirentKuruldu && !labirentYokEdiliyor)
        {
            labirentKurulumTimeNext += Time.deltaTime;
            if (labirentKurulumTimeNext > labirentKurulumTime)
            {
                Debug.Log("Kurulum durdu.");
                labirentKurulumTimeNext = 0;
                labirentYokEdiliyor = true;
                LabirentiTara();
            }
        }
    }
}