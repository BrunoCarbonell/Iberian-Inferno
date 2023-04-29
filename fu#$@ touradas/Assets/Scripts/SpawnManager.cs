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

[System.Serializable]

public class Spawn
{
    public GameObject enemy;
    public Transform spawn;
    public Spawn(GameObject enemy, Transform spawn)
    {
        this.enemy = enemy;
        this.spawn = spawn;
    }
    public Spawn(Transform spawn)
    {
        this.spawn = spawn;
    }
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] Wave[] waves;
    public Transform[] spawnPoints;
    public Transform[] healSpawnPoints;
    [SerializeField]private List<Spawn> spawns = new List<Spawn>();

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
        foreach(Transform t in spawnPoints)
        {
            spawns.Add(new Spawn(t));
        }
        foreach(Wave wave in waves)
        {
            foreach(TypeOfEnemies ty in wave.typeOfEnemies)
            {
                ty.used = ty.maximun;
            }
        }
        nextHealSpawnTime = Time.time + Random.Range(currentWave.healSpawnInterval.x, currentWave.healSpawnInterval.y);
    }

    private void Update()
    {

        if (gM.state == GameState.PAUSE)
        {
            anim.speed = 0;
            return;
        }
        else
            anim.speed = 1;


        if (currentWaveNumber>=0)
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
            int randSpawn = Random.Range(0, spawnPoints.Length);

            if (currentWave.typeOfEnemies[rand].haveMaximun)
            {
                int ocupiedSpot = 0;
                var tmpRand = rand;
                foreach(Spawn s in spawns)
                {
                    if(s.enemy != null)
                        ocupiedSpot++;
                }

                if (ocupiedSpot == spawns.Count)
                {
                    while(tmpRand == rand)
                    {
                        rand = Random.Range(0, currentWave.typeOfEnemies.Length);
                    }
                }
                else
                {
                    while (currentWave.typeOfEnemies[rand].haveMaximun && currentWave.typeOfEnemies[rand].used <= 0)
                    {
                        rand = Random.Range(0, currentWave.typeOfEnemies.Length);
                    }
                    currentWave.typeOfEnemies[rand].used--;

                    if (spawns[randSpawn].enemy != null)
                    {
                        while (spawns[randSpawn].enemy != null)
                        {
                            randSpawn = Random.Range(0, spawnPoints.Length);

                        }
                    }
                }               
            }

            GameObject randomEnemy = currentWave.typeOfEnemies[rand].enemyType;
            Transform randomPoint = spawnPoints[randSpawn];
            GameObject tmp = Instantiate(randomEnemy, randomPoint.position, Quaternion.identity);
            if (currentWave.typeOfEnemies[rand].haveMaximun)
                spawns[randSpawn].enemy = tmp;
    
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
