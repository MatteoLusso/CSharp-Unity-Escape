using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class DeathActivator : MonoBehaviour
{
    public Canvas canvas;
    public bool autoFindCanvas;
    private GameObject canvasMenu;
    private Button backToMenu;
    private Button restart;
    private Button exit;
    public GameObject player;
    public GameObject playerPad;
    public bool autofindPlayerPad;
    private Slider loadingSlider;
    private Text loadingText;
    private Rigidbody playerRB;
    private Rigidbody playerPadRB;
    private Collider playerCL;
    private Collider playerPadCL;

    private LoadingScreenController screen;
    private bool loading;
    private bool playerDead;

    public bool IsPlayerDead()
    {
        return playerDead;
    }

    void Start()
    {
        if(autofindPlayerPad)
        {
            playerPad = GameObject.FindGameObjectWithTag("Pad");
        }
        if(autoFindCanvas)
        {
            canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        }
        screen = canvas.GetComponent<LoadingScreenController>();

        loadingText = canvas.transform.Find("LoadingText").GetComponent<Text>();
        loadingSlider = canvas.transform.Find("LoadingSlider").GetComponent<Slider>();

        canvasMenu = canvas.transform.Find("CanvasMenu").gameObject;
        exit = canvasMenu.transform.Find("ExitButton").GetComponent<Button>();
        restart = canvasMenu.transform.Find("RestartButton").GetComponent<Button>();
        backToMenu = canvasMenu.transform.Find("MenuButton").GetComponent<Button>();


        playerRB = this.GetComponent<Rigidbody>();
        playerCL = this.GetComponent<Collider>();
        playerPadCL = playerPad.GetComponent<Collider>();
        playerPadRB = playerPad.GetComponent<Rigidbody>();

        playerRB.isKinematic = true;
        playerCL.isTrigger = true;
        playerPadCL.enabled = false;

        loadingSlider.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);

        loading = false;

        playerDead = false;
    }

    IEnumerator DeathRoutine()
    {
        bool endGame = true;

        playerDead = true;

        player.GetComponent<FirstPersonController>().enabled = false;

        screen.black.gameObject.SetActive(true);

        while(endGame)
        {
            if(screen.black.color.a < 1.0f)
            {
                screen.black.color = new Vector4(screen.black.color.r , screen.black.color.g, screen.black.color.b, screen.black.color.a + (Time.deltaTime * screen.fadeSpeed));
                //Debug.Log(screen.black.color.a);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;


                if(!loading)
                {
                    canvasMenu.SetActive(true);
                }

                exit.onClick.AddListener(ExitGame);
                backToMenu.onClick.AddListener(BackToMenu);
                restart.onClick.AddListener(RestartGame);
            }

            yield return new WaitForEndOfFrame();

        }


    }

    private void BackToMenu()
    {
        if(!loading)
        {
            StartCoroutine(LoadNewScene("Menu"));
        }
    }

    private void RestartGame()
    {
        if(!loading)
        {
            StartCoroutine(LoadNewScene("Main"));
        }
    }

    IEnumerator LoadNewScene(string sceneName) {

        loading = true;

        canvasMenu.SetActive(false);

        loadingSlider.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        while (!async.isDone)
        {
            float progress = async.progress / 0.9f;
            loadingSlider.value = progress;

            Debug.Log(progress + " " + async.progress);
            yield return new WaitForEndOfFrame();
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("KillCollider"))
        {
            Debug.Log("Giocatore ucciso da: " + other.tag);

            playerRB.isKinematic = false;
            playerCL.isTrigger = false;

            playerPad.GetComponent<PadController>().PlayerIsDead(true);
            playerPad.GetComponent<AudioSource>().enabled = false;
            playerPadCL.enabled = true;
            playerPad.GetComponent<Rigidbody>().isKinematic = false;
            player.GetComponent<CharacterController>().enabled = false;

            StartCoroutine("DeathRoutine");
        }
    }

    public void ExitGame()
    {
        StopAllCoroutines();
        Application.Quit();
    }
}
