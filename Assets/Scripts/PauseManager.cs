using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class PauseManager : MonoBehaviour
{
    public GameObject player;
    public bool autoFindPlayer;
    public DeathActivator playerDeath;
    public bool autoFindPlayerDeath;
    public LoadingScreenController startingLoading;

    public GameObject pauseMenu;

    public SaveManager save;

    private bool pauseActive;

    private bool isGamePausable;
    
    void Start()
    {
        if(autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if(autoFindPlayerDeath)
        {
            playerDeath = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<DeathActivator>();
        }

        pauseMenu.SetActive(false);
        isGamePausable = false;
    }
    void LateUpdate()
    {
        //Debug.Log("Avviato? " + startingLoading.IsStartingBlackScreenEnded() + " Morto? " + !player.GetComponent<DeathActivator>().IsPlayerDead());

        if(startingLoading.IsStartingBlackScreenEnded() && !playerDeath.IsPlayerDead())
        {
            isGamePausable = true;
        }

        //Debug.Log("Si può mettere in pausa? " + isGamePausable);

        if(Input.GetKeyDown(KeyCode.Escape) && isGamePausable)
        {
            if(pauseActive)
            {
                pauseMenu.SetActive(false);
                pauseActive = false;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
            }
            else
            {
                pauseMenu.SetActive(true);
                pauseActive = true;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
            }
        }

        if(pauseActive && playerDeath.IsPlayerDead())
        {
            pauseMenu.SetActive(false);
            pauseActive = false;
        }
    }

    public void CallSave()
    {
        Debug.Log("Salvataggio in corso");        
        save.Save();
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
