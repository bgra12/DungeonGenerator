using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float alertRange;
    public Vector2 patrolInterval;
    public float chaseSpeed;
    public Vector2 damageRange;

    Player player;
    Vector2 currentPos;
    LayerMask obstacleMask, walkableMask;
    List<Vector2> availableMovementList = new List<Vector2>();
    List<Node> nodesList = new List<Node>();
    bool isMoving;

    void Start()
    {
        player = FindObjectOfType<Player>();
        currentPos = transform.position;
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player");
        walkableMask = LayerMask.GetMask("Wall", "Enemy");
        StartCoroutine(Movement());
    }

    private void StartPatrol()
    {
        availableMovementList.Clear();
        Vector2 size = Vector2.one * 0.8f;

        Collider2D hitUp = Physics2D.OverlapBox(currentPos + Vector2.up, size, 0, obstacleMask);
        if (!hitUp)
        {
            availableMovementList.Add(Vector2.up);
        }
        Collider2D hitRight = Physics2D.OverlapBox(currentPos + Vector2.right, size, 0, obstacleMask);
        if (!hitRight)
        {
            availableMovementList.Add(Vector2.right);
        }
        Collider2D hitDown = Physics2D.OverlapBox(currentPos + Vector2.down, size, 0, obstacleMask);
        if (!hitDown)
        {
            availableMovementList.Add(Vector2.down);
        }
        Collider2D hitLeft = Physics2D.OverlapBox(currentPos + Vector2.left, size, 0, obstacleMask);
        if (!hitLeft)
        {
            availableMovementList.Add(Vector2.left);
        }
        if (availableMovementList.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMovementList.Count);
            currentPos += availableMovementList[randomIndex];
        }
        StartCoroutine(Move(Random.Range(patrolInterval.x, patrolInterval.y)));
    }

    private IEnumerator Move(float speed)
    {
        isMoving = true;
        while (Vector2.Distance(transform.position, currentPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentPos, 5f * Time.deltaTime);
            yield return null;
        }
        transform.position = currentPos;
        yield return new WaitForSeconds(speed);
        isMoving = false;
    }

    private IEnumerator Movement()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (!isMoving)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= alertRange)
                {
                    if (dist <= 1.1f)
                    {
                        StartAttack();
                        yield return new WaitForSeconds(Random.Range(0.5f, 1.15f));
                    }
                    else
                    {
                        Vector2 newPos = FindNextStep(transform.position, player.transform.position);
                        if (newPos != currentPos)
                        {
                            currentPos = newPos;
                            StartCoroutine(Move(chaseSpeed));
                        }
                        else
                        {
                            StartPatrol();
                        }
                    }
                }
                else
                {
                    StartPatrol();
                }
            }
        }
    }

    private void StartAttack()
    {
        int roll = Random.Range(0, 100);
        if (roll > 50)
        {
            float damageAmount = Mathf.Ceil(Random.Range(damageRange.x, damageRange.y));
            Debug.Log(name + "attacked and hit for: " + damageAmount + "Points of Damage");
        }
        else
        {
            Debug.Log(name + "attacked and missed!");
        }
    }

    private Vector2 FindNextStep(Vector2 startPos, Vector2 targetPos)
    {
        int listIndex = 0;
        Vector2 myPos = startPos;
        nodesList.Clear();
        nodesList.Add(new Node(startPos, startPos));
        while (myPos != targetPos && listIndex < 1000 && nodesList.Count > 0)
        {
            //check up down left right if tiles are walkable than add into the list
            CheckNode(myPos + Vector2.up, myPos);
            CheckNode(myPos + Vector2.down, myPos);
            CheckNode(myPos + Vector2.left, myPos);
            CheckNode(myPos + Vector2.right, myPos);
            listIndex++;
            if (listIndex < nodesList.Count)
            {
                myPos = nodesList[listIndex].position;
            }
        }
        if (myPos == targetPos)
        {
            nodesList.Reverse();
            for (int i = 0; i < nodesList.Count; i++)
            {
                if (myPos == nodesList[i].position)
                {
                    if (nodesList[i].parent == startPos)
                    {
                        return myPos;
                    }
                    else
                    {
                        myPos = nodesList[i].parent;
                    }
                }
            }
        }
        return startPos;
    }

    private void CheckNode(Vector2 checkPoint, Vector2 parent)
    {
        Vector2 size = Vector2.one * 0.5f;
        Collider2D hit = Physics2D.OverlapBox(checkPoint, size, 0, walkableMask);
        if (!hit)
        {
            nodesList.Add(new Node(checkPoint, parent));
        }
    }
}
