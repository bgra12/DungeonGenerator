using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed; 

    LayerMask obstacleMask;
    Vector2 targetPos;
    Transform gfx;
    float flipX;
    bool isMoving;

    void Start()
    {
        gfx = GetComponentInChildren<SpriteRenderer>().transform;
        flipX = gfx.localScale.x;
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
    }

    void Update()
    {
        float horz = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0)
        {
            if (Mathf.Abs(horz) > 0)
            {
                gfx.localScale = new Vector2(flipX * horz, gfx.localScale.y);
            }
            if (!isMoving)
            {
                if (Mathf.Abs(horz) > 0)
                {
                    targetPos = new Vector2(transform.position.x + horz, transform.position.y);
                }
                else if (Mathf.Abs(vert) > 0)
                {
                    targetPos = new Vector2(transform.position.x, transform.position.y + vert);
                }
                //check for collisions
                Vector2 hitSize = Vector2.one * 0.8f;
                Collider2D hit = Physics2D.OverlapBox(targetPos, hitSize, 0, obstacleMask);
                if (!hit)
                {
                    StartCoroutine(MoveCharacterCo());
                }

            }
        }
    }

    IEnumerator MoveCharacterCo()
    {
        isMoving = true;
        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }
}
