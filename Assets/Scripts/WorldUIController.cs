using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldUIController : MonoBehaviour
{
    public Text toShow;
    public GameObject target;

    public Transform player;
    public bool autoFindPlayer;
    public Camera mainCamera;

    public bool useRaycast;
    public bool autoFindMainCamera;

    public float activationDistance;

    private int layerMask;

    void Start()
    {
        if(autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        } 
        if(autoFindMainCamera)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        } 

        layerMask = 1 << target.layer;
        //layerMask = ~layerMask;

        toShow.enabled = false;
    }

    void Update()
    {
        if((player.position - this.transform.position).magnitude <= activationDistance)
        {
            this.transform.rotation = Quaternion.LookRotation(player.forward, Vector3.up);

            if(useRaycast)
            {
                Ray lockingDirection = mainCamera.ScreenPointToRay(new Vector3 (Screen.width / 2, Screen.height / 2, 0.0f));
                RaycastHit hitPoint;
                if(Physics.Raycast(lockingDirection, out hitPoint, Mathf.Infinity, layerMask))
                {
                    if(hitPoint.transform.name == target.name)
                    {
                        toShow.enabled = true;
                    }
                    else
                    {
                        toShow.enabled = false;
                    }
                }
                else
                {
                    toShow.enabled = false;
                }
            }
            else
            {
                toShow.enabled = true;
            }
        }
        else
        {
            toShow.enabled = false;
        }
    }
}
