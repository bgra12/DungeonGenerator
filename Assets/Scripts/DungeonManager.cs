using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    public GameObject[] randomItems, randomEnemies;
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;
    [Range(50, 1000)] public int totalFloorCount;
    [Range(0, 100)] public int itemSpawnPercent;
    [Range(0, 100)] public int enemySpawnPercent;

    [HideInInspector] public float minX, maxX, minY, maxY;

    private List<Vector3> floorPosList = new List<Vector3>();
    LayerMask floorMask, wallMask;

    private void Start()
    {
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");
        RandomWalker();
    }

    private void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void RandomWalker()
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
        //create tiles on the each position in the list
        for (int i = 0; i < floorPosList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorPosList[i], Quaternion.identity) as GameObject;
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform);
        }
        StartCoroutine(DelayProgress());
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
