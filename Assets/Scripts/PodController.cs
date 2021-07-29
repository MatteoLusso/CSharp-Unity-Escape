using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PodController : MonoBehaviour
{
    public SettingsManager settings;
    public bool autoFindSettings;
    public LoadManager loadManager;
    public bool autoFindLoadManager;
    public GameObject button1;
    public GameObject button2;
    public GameObject door;
    public Text openDoor1;
    public Text openDoor2;
    public Text closeDoor1;
    public Text closeDoor2;
    public Text offline1;
    public Text offline2;
    public Text online1;
    public Text online2;

    public GameObject launcher;
    public Text podReady;
    public Text podNotReady;

    public Text doorNotClosed;
    public int offlineBeep;
    public float offlineSpeed;
    public float activationDistance;
    public Transform player;
    public bool autoFindPlayer;

    public GameObject pad;
    public bool autoFindPad;

    public DeathActivator deathActivator;
    public bool autoDeathActivator;

    private Animation doorAnimations;
    private bool doorLocked;
    private bool doorOpen;

    // Start is called before the first frame update
    void Start()
    {
        if(autoFindSettings)
        {
            settings = GameObject.FindGameObjectWithTag("Settings").gameObject.GetComponent<SettingsManager>();
        }
        if(autoFindLoadManager)
        {
            loadManager = GameObject.FindGameObjectWithTag("Load Manager").gameObject.GetComponent<LoadManager>();
        }
        if(autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if(autoFindPad)
        {
            pad = GameObject.FindGameObjectWithTag("Pad");
        }
        if(autoDeathActivator)
        {
            deathActivator = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<DeathActivator>();
        }

        doorAnimations = door.GetComponent<Animation>();

        doorLocked = true;
        doorOpen = false;
        closeDoor1.enabled = false;
        openDoor1.enabled = true;
        closeDoor2.enabled = false;
        openDoor2.enabled = true;

        podReady.enabled = false;
        podNotReady.enabled = true;
        doorNotClosed.enabled = false;

        offline1.enabled = true;
        online1.enabled = false;

        offline2.enabled = true;
        online2.enabled = false;
    }

    void Update()
    {
        if(doorLocked)
        {
            if(settings.loadGame)
            {
                //Debug.Log("Carico partita? " +  settings.loadGame);
                //Debug.Log("L'energia è stata deviata? "+ loadManager.GetSavedData().isPowerDeviate);
                if(loadManager.GetSavedData().isPowerDeviate)
                {
                    UnlockPodDoor();
                    //Debug.Log("Nel salvataggio è aperta, quindi apro la porta");
                }
            }

            if((button1.transform.position - player.transform.position).magnitude <= activationDistance && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine("AccessNegate", offline1);
            }
            if((button2.transform.position - player.transform.position).magnitude <= activationDistance && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine("AccessNegate", offline2);
            }
        }
        else
        {
            offline1.enabled = false;
            online1.enabled = true;

            offline2.enabled = false;
            online2.enabled = true;

            if((button1.transform.position - player.transform.position).magnitude <= activationDistance && Input.GetKeyDown(KeyCode.E) && !doorAnimations.isPlaying)
            {
                StopCoroutine("AccessNegate");

                if(!doorOpen)
                {
                    closeDoor1.enabled = true;
                    openDoor1.enabled = false;
                    closeDoor2.enabled = true;
                    openDoor2.enabled = false;

                    door.GetComponent<Animation>().Play("PodDoorOpening");
                    doorOpen = true;
                }
                else
                {
                    closeDoor1.enabled = false;
                    openDoor1.enabled = true;
                    closeDoor2.enabled = false;
                    openDoor2.enabled = true;

                    door.GetComponent<Animation>().Play("PodDoorClosing");
                    doorOpen = false;
                }
            }

            if((button2.transform.position - player.transform.position).magnitude <= activationDistance && Input.GetKeyDown(KeyCode.E) && !doorAnimations.isPlaying)
            {
                StopCoroutine("AccessNegate");

                if(!doorOpen)
                {
                    door.GetComponent<Animation>().Play("PodDoorOpening");
                    doorOpen = true;
                }
                else
                {
                    door.GetComponent<Animation>().Play("PodDoorClosing");
                    doorOpen = false;
                }
            }
        }

        if((launcher.transform.position - player.transform.position).magnitude <= activationDistance && Input.GetKeyDown(KeyCode.E))
        {
            if(!pad.GetComponent<PadController>().AuxiliaryEnergyFound())
            {
                StartCoroutine("AccessNegate", podNotReady);
            }
            else
            {
                if(!doorOpen)
                {
                    StopCoroutine("AccessNegate");

                    podReady.enabled = true;
                    podNotReady.enabled = false;
                    doorNotClosed.enabled = false;

                    this.GetComponent<AudioSource>().Play();

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    deathActivator.StartCoroutine("DeathRoutine");
                }
                else
                {
                    podReady.enabled = false;
                    podNotReady.enabled = false;
                    doorNotClosed.enabled = true;

                    StartCoroutine("AccessNegate", doorNotClosed);
                }

            }
        }
    }

    public bool IsPodDoorActive()
    {
        return !doorLocked;
    }

    IEnumerator AccessNegate(Text offline)
    {
        for(int i = 0; i <= offlineBeep; i++)
        {
            if(i % 2 == 0)
            {
                offline.enabled = false;
                //Debug.Log("Off");
            }
            else
            {
                offline.enabled = true;
                //Debug.Log("On");
            }

            yield return new WaitForSeconds(offlineSpeed);
        }

        offline.enabled = true;
    }

    public void UnlockPodDoor()
    {
        doorLocked = false;
    }
}
