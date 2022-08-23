using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private EnemyController controller;
    [SerializeField] private int enemyID;
    private bool isChosen;
    private bool isHit;
    private Color color;
    private Renderer rend;

    
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        color = Color.white;
        rend.material.SetColor("_Color", color);
        isHit = false;
        player = controller.player;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rend.material.SetColor("_Color", color);
        if (!controller.run)
        {
            if (controller.whichEnemy == enemyID)
            {
                isChosen = true;
                Chosen();
            }
            else
            {
                color = Color.white;
                isChosen = false;
                isHit = false;
            }
        }
        
        if (controller.run)
        {
            isHit = false;
        }

    }

    private void Chosen()
    {
        color = Color.blue;
        if (isHit)
        {
            color = Color.red;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isChosen && player.isAttacking)
        {
            isHit = true;
            Debug.Log("hit");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isChosen && player.isAttacking)
        {
            isHit = true;
            Debug.Log("hit");
        }
    }

}
