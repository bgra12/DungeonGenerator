using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;
    public int totalFloorCount;

    [HideInInspector] public float minX, maxX, minY, maxY;

    private List<Vector3> floorPosList = new List<Vector3>();

    private void Start()
    {
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
            switch (Random.Range(1, 5))
            {
            case 1:
                currentPos += Vector3.up;
                break;
            case 2:
                currentPos += Vector3.right;
                break;
            case 3:
                currentPos += Vector3.down;
                break;
            case 4:
                currentPos += Vector3.left;
                break;
            }
            //check if randomly generated position is already in the list if so don't add it
            bool inFloorList = false;
            for (int i = 0; i < floorPosList.Count; i++)
            {
                if (Vector3.Equals(currentPos, floorPosList[i]))
                {
                    inFloorList = true;
                    break;
                }
            }
            if (!inFloorList)
            {
                floorPosList.Add(currentPos);
            }
        }
        //create tiles on the each position in the list
        for(int i = 0; i < floorPosList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorPosList[i], Quaternion.identity) as GameObject;
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform);
        }
        StartCoroutine(DelayProgress());
    }

    private IEnumerator DelayProgress()
    {
        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }
        CreateExitDoorway();
    }

    private void CreateExitDoorway()
    {
        Vector3 exitDoorPos = floorPosList[floorPosList.Count - 1];
        GameObject goExitDoor = Instantiate(exitPrefab, exitDoorPos, Quaternion.identity) as GameObject;
        goExitDoor.name = exitPrefab.name;
        goExitDoor.transform.SetParent(transform);
    }

}
