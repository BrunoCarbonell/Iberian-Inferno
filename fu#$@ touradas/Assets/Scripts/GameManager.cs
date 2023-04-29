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
    public GameObject pauseMenu;

    private void Update()
    {

        switch (state)
        {
            case GameState.PAUSE:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0;
                break;
            case GameState.PLAY:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1;
                break;
            case GameState.GAMEOVER:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 1;
                break;
            case GameState.VICTORY:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 1;
                break;

        }

        
    }

    public void Pause()
    {
        if(state == GameState.PAUSE)
        {
            state = GameState.PLAY;
            pauseMenu.SetActive(false);
        }
        else if(state == GameState.PLAY)
        {
            state = GameState.PAUSE;
            pauseMenu.SetActive(true);
        }
    }


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


