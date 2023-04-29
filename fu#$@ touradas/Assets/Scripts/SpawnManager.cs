using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[System.Serializable]
public class TypeOfEnemies
{
    public GameObject enemyType;
    public bool haveMaximun;
    public int maximun;
    public int used;
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public int noOfEnemies;
    public TypeOfEnemies[] typeOfEnemies;
    public Vector2 spawnInterval;
    public int numberOfHeals;
    public Vector2 healSpawnInterval;
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] Wave[] waves;
    public Transform[] spawnPoints;
    public Transform[] healSpawnPoints;

    private Wave currentWave;
    [SerializeField]private int currentWaveNumber;
    private bool canSpawn = true;
    private float nextSpawnTime;
    private float nextHealSpawnTime;
    private GameManager gM;
    private Animator anim;
    public TextMeshProUGUI wName;
    private bool canAnimate = false;
    public GameObject healPrefab;


    void Start()
    {
        anim = GetComponent<Animator>();
        gM = GameObject.FindObjectOfType<GameManager> ();
        currentWave = waves[0];
        currentWaveNumber = -1;
        wName.text = waves[currentWaveNumber + 1].waveName;
        anim.SetTrigger("Wave1");
        foreach(Wave wave in waves)
        {
            foreach(TypeOfEnemies ty in wave.typeOfEnemies)
            {
                ty.used = ty.maximun;
            }
        }
    }

    private void Update()
    {

        if(currentWaveNumber>=0)
            currentWave = waves[currentWaveNumber];

        if (currentWaveNumber >= 0)
        {
            SpawnWave();
            SpawnHeal();
        }

        if (gM.enemyList.Count == 0 && currentWaveNumber+1 != waves.Length && canAnimate)
        {
            wName.text = waves[currentWaveNumber+1].waveName;
            anim.SetTrigger("WaveComplete");
            canAnimate = false;
        }
        if(currentWaveNumber == waves.Length-1 && gM.enemyList.Count == 0)
        {
            anim.SetTrigger("Victory");
            gM.state = GameState.VICTORY;
        } 

        if(gM.state == GameState.GAMEOVER)
        {
            anim.SetTrigger("GameOver");
        }
        
        
    }


    void SpawnHeal()
    {
        if(canSpawn && nextHealSpawnTime < Time.time && currentWave.numberOfHeals > 0)
        {
            //nextHealSpawnTime = Time.time;
            Transform randomPoint = healSpawnPoints[Random.Range(0, healSpawnPoints.Length)];
            GameObject tmp = Instantiate(healPrefab, randomPoint.position, Quaternion.identity);
            currentWave.numberOfHeals--;
            nextHealSpawnTime = Time.time + Random.Range(currentWave.healSpawnInterval.x, currentWave.healSpawnInterval.y);
        }
    }

    void SpawnWave()
    {

        if (canSpawn && nextSpawnTime < Time.time)
        {
            var rand = Random.Range(0, currentWave.typeOfEnemies.Length);

            if (currentWave.typeOfEnemies[rand].haveMaximun)
            {
                while (currentWave.typeOfEnemies[rand].haveMaximun && currentWave.typeOfEnemies[rand].used <= 0)
                {
                    rand = Random.Range(0, currentWave.typeOfEnemies.Length);
                }
                currentWave.typeOfEnemies[rand].used--;
            }
               
            
            GameObject randomEnemy = currentWave.typeOfEnemies[rand].enemyType;
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
