using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;


[NetworkSettings(channel = 0, sendInterval = 5f)]
public class SetOnPlayer : NetworkBehaviour 
{
    public Transform target;
    public Camera playerCam;
    private bool nameIsSet = false;
    public string targetName = "";

    void Start()
    {
        //GameObject objPlayerCam = GameObject.Find("Camera");//GameObject.FindGameObjectWithTag("PlayerCam");
        //playerCam = new Camera();
        playerCam = GameObject.FindGameObjectWithTag("PlayerCam").GetComponent<Camera>(); //target.gameObject.GetComponent<Camera>(); //objPlayerCam.GetComponent<Camera>();
    }
    
	void Update() 
    {
        if (target != null)
        {
            Vector3 tmpPos = target.localPosition;
            tmpPos.y += 2f;
            transform.position = tmpPos;

            SetTextRotation();

            if (!nameIsSet)
            {
                targetName = target.name;
                transform.name = targetName + " Name";

                TextMesh[] textMeshes = GetComponentsInChildren<TextMesh>();
                foreach (TextMesh textMesh in textMeshes)
                {
                    textMesh.text = targetName;
                }
                
                nameIsSet = true;
            }
        }
	}

    void SetTextRotation()
    {
        if (playerCam != null)
        {
            transform.LookAt(playerCam.transform.position);
            transform.Rotate(new Vector3(0, 180, 0));
        }
    }  
}
