using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { MENU, PAUSE, PLAY, GAMEOVER, VICTORY};

public class GameManager : MonoBehaviour
{
    public List<GameObject> enemyList = new List<GameObject> ();
    public GameState state;
    public Image hpBar;



    public void UpdateHpBar(float atualHP, float maxHp)
    {

        var tmp = (atualHP / maxHp);
        hpBar.fillAmount = tmp;
    }

    public void Reset()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    } 

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}


