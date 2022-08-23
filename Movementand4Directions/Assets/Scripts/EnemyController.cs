using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int whichEnemy = 0;
    public int qEnemy = 0;
    public bool run;
    [SerializeField] public Player player;
    [SerializeField] private float qTime = 0.3f;
    [SerializeField] private float rTime = 1f;
    
    // Start is called before the first frame update
    void Start()
    {
        run = false;
        StartCoroutine(Queue());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (run == true)
        {
            StartCoroutine(Decider());
        }
    }

    private IEnumerator Decider()
    {
        run = false;
        whichEnemy = qEnemy;
        yield return new WaitForSeconds(rTime);
        StartCoroutine(Queue());
    }

    private IEnumerator Queue()
    {
        while(qEnemy == whichEnemy) 
        {
            qEnemy = Random.Range(1, 9);
        }
            
        yield return new WaitForSeconds(qTime);
        run = true;
    }
}
