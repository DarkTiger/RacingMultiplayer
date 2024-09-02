using UnityEngine;
using System.Collections;

public class CameraOrbitScript : MonoBehaviour 
{
	public Transform target;
   
	// Camera distance variables
	public float Distance = 5.0f;
	public float DistanceMin = 1.0f;
	public float DistanceMax = 15.0f;
	float startingDistance = 5.0f;
	float desiredDistance = 5.0f;

	// Mouse variables
	float mouseX = 0.0f;
	float mouseY = 0.0f;
	public float X_MouseSensitivity = 5.0f;
	public float Y_MouseSensitivity = 5.0f;
	public float MouseWheelSensitivity = 5.0f;

	// Axis limit variables
	public float Y_MinLimit = 15.0f;
	public float Y_MaxLimit = 70.0f;   
	public float DistanceSmooth = 0.025f;
	float velocityDistance = 0.0f; 
	Vector3 desiredPosition = Vector3.zero;   
	public float X_Smooth = 0.05f;
	public float Y_Smooth = 0.1f;

	// Velocity variables
	float velX = 0.0f;
	float velY = 0.0f;
	float velZ = 0.0f;
	Vector3 position = Vector3.zero;

  
	void Start() 
	{
		Distance = Vector3.Distance(target.transform.position, gameObject.transform.position);
        if (Distance > DistanceMax)
        {
            DistanceMax = Distance;
            startingDistance = Distance;
            Reset();
        }
	}

		
	void LateUpdate()
	{          
        if (target == null || Cursor.visible)
        {
            return;
        }

		HandlePlayerInput();
 		CalculateDesiredPosition();
 		UpdatePosition();
	}


	void HandlePlayerInput()
	{
		// mousewheel deadZone
		float deadZone = 0.01f; 
 
		//if (Input.GetMouseButton(0))
		//{

            mouseX += Input.GetAxis("Mouse X") * X_MouseSensitivity;
            mouseY -= Input.GetAxis("Mouse Y") * Y_MouseSensitivity;

            mouseY = ClampAngle(mouseY, Y_MinLimit, Y_MaxLimit);
        
            //}
	 
		// this is where the mouseY is limited - Helper script
		
 
		// get Mouse Wheel Input
		if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
		{
		   desiredDistance = Mathf.Clamp(Distance - (Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity), 
													 DistanceMin, DistanceMax);
		}
	}

  
	void CalculateDesiredPosition()
	{
        //Debug.Log(Distance);
        Distance = Mathf.SmoothDamp(Distance, desiredDistance, ref velocityDistance, DistanceSmooth);
        
    	desiredPosition = CalculatePosition(mouseY, mouseX, Distance);
        //Debug.Log(Distance + " " + desiredPosition.x + " " + desiredPosition.y + " " + desiredPosition.z);
	}


	Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
	{
		Vector3 direction = new Vector3(0, 0, -distance);
		Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
		return target.position + (rotation * direction);
	}

  	
	void UpdatePosition()
	{
		float posX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
		float posY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
        float posZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
		position = new Vector3(posX, posY, posZ);
 		transform.position = position;
 		transform.LookAt(target);
	}

	
	void Reset() 
	{
		mouseX = 0;
		mouseY = 0;
		Distance = startingDistance;
		desiredDistance = Distance;
	}

	
    float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360.0f || angle > 360.0f)
        {
            if (angle < -360.0f)
            {
                angle += 360.0f;
            }

            if (angle > 360.0f)
            {
                angle -= 360.0f;
            }
        }

        return Mathf.Clamp(angle, min, max);
    }
}



