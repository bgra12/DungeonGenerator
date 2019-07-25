using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DungeonType { Caverns, Rooms, Winding }

public class DungeonManager : MonoBehaviour
{
    public GameObject[] randomItems, randomEnemies;
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;
    [Range(50, 1000)] public int totalFloorCount;
    [Range(0, 100)] public int itemSpawnPercent;
    [Range(0, 100)] public int enemySpawnPercent;
    public DungeonType dungeonType;

    [HideInInspector] public float minX, maxX, minY, maxY;

    private List<Vector3> floorPosList = new List<Vector3>();
    LayerMask floorMask, wallMask;

    private void Start()
    {
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");

        switch (dungeonType)
        {
        case DungeonType.Caverns:
            CavernWalker();
            break;
        case DungeonType.Rooms:
            RoomWalker();
            break;
        case DungeonType.Winding:
            WindingWalker();
            break;
        }
    }

    private void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void CavernWalker()
    {
        Vector3 currentPos = Vector3.zero;
        floorPosList.Add(currentPos);
        //create position list randomly
        while (floorPosList.Count < totalFloorCount)
        {
            currentPos += RandomDirection();
            if (!CheckIfInFloorList(currentPos))
            {
                floorPosList.Add(currentPos);
            }
        }
        StartCoroutine(DelayProgress());
    }

    private void RoomWalker()
    {
        Vector3 currentPos = Vector3.zero;
        floorPosList.Add(currentPos);
        //create position list randomly
        while (floorPosList.Count < totalFloorCount)
        {
            currentPos = PickARandomDirectionAndWalk(currentPos);
            CreateRandomRoomAtCurrentPosition(currentPos); //create random room after long walk
        }
        StartCoroutine(DelayProgress());
    }

    private void WindingWalker()
    {
        Vector3 currentPos = Vector3.zero;
        floorPosList.Add(currentPos);
        //create position list randomly
        while (floorPosList.Count < totalFloorCount)
        {
            currentPos = PickARandomDirectionAndWalk(currentPos);
            int roll = Random.Range(0, 101);
            if (roll > 50)
            {
                CreateRandomRoomAtCurrentPosition(currentPos); //create random room after long walk
            }
        }
        StartCoroutine(DelayProgress());
    }

    private void CreateRandomRoomAtCurrentPosition(Vector3 currentPos)
    {
        int width = Random.Range(1, 5);
        int height = Random.Range(1, 5);
        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector3 offSet = new Vector3(w, h, 0);
                if (!CheckIfInFloorList(currentPos + offSet))
                {
                    floorPosList.Add(currentPos + offSet);
                }
            }
        }
    }

    private Vector3 PickARandomDirectionAndWalk(Vector3 currentPos)
    {
        Vector3 walkDir = RandomDirection();
        int walkLength = Random.Range(9, 18);
        for (int i = 0; i < walkLength; i++)
        {
            if (!CheckIfInFloorList(currentPos + walkDir))
            {
                floorPosList.Add(currentPos + walkDir);
            }
            currentPos += walkDir;
        }
        return currentPos;
    }

    private bool CheckIfInFloorList(Vector3 position)
    {
        //check if randomly generated position is already in the list if so don't add it
        for (int i = 0; i < floorPosList.Count; i++)
        {
            if (Vector3.Equals(position, floorPosList[i]))
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 RandomDirection()
    {
        switch (Random.Range(1, 5))
        {
        case 1:
            return Vector3.up;
        case 2:
            return Vector3.right;
        case 3:
            return Vector3.down;
        case 4:
            return Vector3.left;
        }
        return Vector3.zero;
    }

    private IEnumerator DelayProgress()
    {
        //create tiles on the each position in the list
        for (int i = 0; i < floorPosList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorPosList[i], Quaternion.identity) as GameObject;
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform);
        }
        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }
        CreateExitDoorway();
        Vector2 hitSize = Vector2.one * 0.8f;
        for (int x = (int)minX - 2; x <= (int)maxX + 2; x++)
        {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++)
            {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask);
                if (hitFloor)
                {
                    if (!Vector2.Equals(hitFloor.transform.position, floorPosList[floorPosList.Count - 1]))
                    {
                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                        SpawnRandomItems(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                        SpawnRandomEnemies(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                    }
                }
            }
        }
    }

    private void SpawnRandomEnemies(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        if (!hitTop && !hitBottom && !hitLeft && !hitRight)
        {
            int roll = Random.Range(1, 101);
            if (roll <= enemySpawnPercent)
            {
                int enemyIndex = Random.Range(0, randomEnemies.Length);
                GameObject goEnemy = Instantiate(randomEnemies[enemyIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goEnemy.name = randomEnemies[enemyIndex].name;
                goEnemy.transform.SetParent(hitFloor.transform);
            }
        }
    }

    private void SpawnRandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        if ((hitBottom || hitRight || hitBottom || hitLeft) && !(hitTop && hitBottom) && !(hitLeft && hitRight))
        {
            int roll = Random.Range(1, 101);
            if (roll <= itemSpawnPercent)
            {
                int itemIndex = Random.Range(0, randomItems.Length);
                GameObject goItem = Instantiate(randomItems[itemIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goItem.name = randomItems[itemIndex].name;
                goItem.transform.SetParent(hitFloor.transform);
            }
        }
    }

    private void CreateExitDoorway()
    {
        Vector3 exitDoorPos = floorPosList[floorPosList.Count - 1];
        GameObject goExitDoor = Instantiate(exitPrefab, exitDoorPos, Quaternion.identity) as GameObject;
        goExitDoor.name = exitPrefab.name;
        goExitDoor.transform.SetParent(transform);
    }

}
