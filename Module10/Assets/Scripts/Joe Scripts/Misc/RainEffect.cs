using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainEffect : MonoBehaviour
{
    private GameObject playerGameObj;

    void Update()
    {
        if(playerGameObj == null)
        {
            playerGameObj = GameObject.FindGameObjectWithTag("Player");
        }

        transform.position = new Vector3(playerGameObj.transform.position.x, playerGameObj.transform.position.y + 10.0f, playerGameObj.transform.position.z);
    }
}
