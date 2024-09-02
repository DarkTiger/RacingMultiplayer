using UnityEngine;
using System.Collections;

public class AssignCameraToNameObject : MonoBehaviour 
{
    private Camera cam = null;
    private GameObject objName = null;
    private bool cameraIsSet = false;

	void Start() 
    {
        cam = GetComponent<Camera>();   
    }

    void Update()
    {
        if (!cameraIsSet)
        {
            objName = GetComponent<CameraOrbitScript>().target.gameObject;

            if (objName != null)
            {
                //Debug.Log("objName è diverso da null");
                SetCameraToNameObj();
            }
        }
    }

    void SetCameraToNameObj()
    {
        if (cam != null)
        {
            //Debug.Log("cam != null");
            //objName.GetComponent<SetOnPlayer>().playerCam = gameObject;//cam;
            cameraIsSet = true;
        }
    }
}
