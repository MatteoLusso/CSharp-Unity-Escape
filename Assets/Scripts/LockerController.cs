using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerController : MonoBehaviour
{
    public Transform player;
    public bool autoFindPlayer;

    public Camera playerCamera;
    public bool autoFindCamera;

    public GameObject door;
    public float maxDistanceLockerInteraction;

    private bool doorClosed;
    private Animation doorAnimations;

    private int layersMask;
    private bool importantObjectFound;

    void Start()
    {

        layersMask = LayerMask.GetMask("LockerDoors");
        layersMask |= LayerMask.GetMask("ImportantObjects");
        //layersMask = ~layersMask;
        
        if(autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if(autoFindCamera)
        {
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        doorAnimations = door.GetComponent<Animation>();

        doorClosed = true;

        importantObjectFound = false;
    }

    // Update is called once per frame
    void Update()
    {
        /*if(this.transform.Find("Important").gameObject.activeSelf)
        {
            Debug.Log("In questo armadietto è stato trovato l'oggetto importante? " + importantObjectFound);
        }*/
        if(Input.GetKeyDown(KeyCode.E) && (this.transform.position - player.position).magnitude < maxDistanceLockerInteraction && !doorAnimations.isPlaying)
        {
            Ray lockingDirection = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0.0f));
            RaycastHit hitPoint;
            if(Physics.Raycast(lockingDirection, out hitPoint, Mathf.Infinity, layersMask))
            {
                if(hitPoint.transform.CompareTag("LockerDoor"))
                {
                    if(doorClosed)
                    {
                        doorAnimations.Play("LockerOpening");
                        doorClosed = false;
                    }
                    else
                    {
                        doorAnimations.Play("LockerClosing");
                        doorClosed = true;
                    }
                }
                else if(!doorClosed)
                {
                    if(hitPoint.transform.CompareTag("Important") && !GameObject.Find("Pad").GetComponent<PadController>().AuxiliaryEnergyFound())
                    {
                        importantObjectFound = true;
                        //Debug.Log("oggetto trovato " + importantObjectFound);
                        hitPoint.transform.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public bool ImportantObjectFound()
    {
        return importantObjectFound;
    }
}
