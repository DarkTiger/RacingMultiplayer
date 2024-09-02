using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour 
{
    public Transform target;
    public float distance = 10;
    public float height = 10;
    public float heightDamping = 2;
    public float rotationDamping = 0.6f;
    	
	
	void LateUpdate() 
    {
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;
        
        float nextRotationAngle; //= target.eulerAngles.y;
        
        Vector3 MV = target.GetComponent<Rigidbody>().velocity;

        if (MV == Vector3.zero)
        {
            nextRotationAngle = currentRotationAngle;
        }
        else
        {
            nextRotationAngle = Quaternion.LookRotation(MV).eulerAngles.y;
        }

        float nextHeight = target.position.y + height;
       
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, nextRotationAngle, rotationDamping * Time.deltaTime);
        
        currentHeight = Mathf.Lerp(currentHeight, nextHeight, heightDamping * Time.deltaTime);
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;

        Vector3 tmpPos = transform.position;
        tmpPos.y = currentHeight;

        transform.position = tmpPos;
        transform.LookAt(target);
	}
}
