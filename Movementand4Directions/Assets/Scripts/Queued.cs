using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queued : MonoBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private int enemyID;
    private bool isChosen;
    private Color color;
    private Renderer rend;


    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        color = Color.white;
        rend.material.SetColor("_Color", color);
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
            if (controller.qEnemy == enemyID)
            {
                isChosen = true;
                Chosen();
            }
            else
            {
                isChosen = false;
                color = Color.white;
            }
        }
    }

    private void Chosen()
    {
        color = Color.blue;
    }
}
