using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[System.Serializable]

public class Wave
{
    public string waveName;
    public int noOfEnemies;
    public GameObject[] typeOfEnemies;
    public Vector2 spawnInterval;
    
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] Wave[] waves;
    public Transform[] spawnPoints;

    private Wave currentWave;
    [SerializeField]private int currentWaveNumber;
    private bool canSpawn = true;
    private float nextSpawnTime;
    private GameManager gM;
    private Animator anim;
    public TextMeshProUGUI wName;
    private bool canAnimate = false;


    void Start()
    {
        anim = GetComponent<Animator>();
        gM = GameObject.FindObjectOfType<GameManager> ();
        currentWave = waves[0];
        currentWaveNumber = -1;
        wName.text = waves[currentWaveNumber + 1].waveName;
        anim.SetTrigger("Wave1");
    }

    private void Update()
    {


        currentWave = waves[currentWaveNumber];
       if(currentWaveNumber>=0)
            SpawnWave();

        if (gM.enemyList.Count == 0 && currentWaveNumber+1 != waves.Length && canAnimate)
        {
            wName.text = waves[currentWaveNumber+1].waveName;
            anim.SetTrigger("WaveComplete");
            canAnimate = false;
        }
        if(currentWaveNumber == waves.Length)
        {
            anim.SetTrigger("Victory");
            gM.state = GameState.VICTORY;
        } 

        if(gM.state == GameState.GAMEOVER)
        {
            anim.SetTrigger("GameOver");
        }
        
        
    }


    void SpawnWave()
    {

        if (canSpawn && nextSpawnTime < Time.time)
        {
            GameObject randomEnemy = currentWave.typeOfEnemies[Random.Range(0, currentWave.typeOfEnemies.Length)];
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject tmp = Instantiate(randomEnemy, randomPoint.position, Quaternion.identity);
            gM.enemyList.Add(tmp);
            currentWave.noOfEnemies--;
            nextSpawnTime = Time.time + Random.Range(currentWave.spawnInterval.x, currentWave.spawnInterval.y);
            if(currentWave.noOfEnemies == 0)
            {
                canSpawn = false;
                canAnimate = true;
            }
        }
        
    }

    public void SpawnNextWave()
    {
        currentWaveNumber++;
        canSpawn = true;
        
    }



}
