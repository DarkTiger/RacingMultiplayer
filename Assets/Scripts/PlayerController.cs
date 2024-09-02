using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;


public class PlayerController : NetworkBehaviour 
{
    //public GameObject dbHandler;
    public int ID = 0;
    public Camera playerCam;

    //public Text secondsText;
    //public float updateFrequencySeconds = 1;
    private Rigidbody rb;
    //public float sensibility = 10;
    private bool canJump = false;
    private InputField inputTextChat;


    //private float seconds;
    //private float lastSecond = 0;
    
    /*private MySqlConnection conn = null;
    private MySqlCommand cmd = null;
    private MySqlDataReader dReader = null;*/


	void Start () 
    {
        rb = GetComponent<Rigidbody>();
        inputTextChat = GameObject.Find("InputTextChat").GetComponent<InputField>();
        /*conn = dbHandler.GetComponent<DatabaseHandler>().conn;
        cmd = dbHandler.GetComponent<DatabaseHandler>().cmd;*/
    }


    void FixedUpdate() 
    {
        if (isLocalPlayer && !inputTextChat.isFocused && !Cursor.visible)
        {
            if (Input.GetKey(KeyCode.W))
            {
                rb.AddForce(playerCam.transform.forward * 12);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                rb.AddForce(-playerCam.transform.forward * 12);
            }

            if (Input.GetKey(KeyCode.A))
            {
                rb.AddForce(-playerCam.transform.right * 12);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                rb.AddForce(playerCam.transform.right * 12);
            }

            if (Input.GetKey(KeyCode.Space))
            {
                if (canJump)
                {
                    rb.AddExplosionForce(200, transform.position, 10);
                }
            }
        }
        
        //UpdateDB();
    }

    //void OnCollisionStay(Collision other)
    //{
           //canJump = true;
    //}

    void OnCollisionStay(Collision other)
    {
      //if (collision.gameObject.tag.Equals("Wall"))
      //{
            Vector3 hit = other.contacts[0].normal;
            float angle = Vector3.Angle(hit, Vector3.up);
                    
            if (angle >= -40f && angle <= 40f)//Mathf.Approximately(angle, 0)) //DOWN
            {
                //Debug.Log("Down");
                canJump = true;
            }
            /*if (Mathf.Approximately(angle, 180)) //UP
            {
                Debug.Log("Up");
            }
            if (Mathf.Approximately(angle, 90)) //SIDES
            {
                Vector3 cross = Vector3.Cross(Vector3.forward, hit);
                if (cross.y > 0) //LEFT
                {
                    Debug.Log("Left");
                }
                else //RIGHT
                {
                    Debug.Log("Right");
                }
            }*/
        //}
    }

    void OnCollisionExit(Collision other)
    {
        canJump = false;
    }
    
    
    /*void UpdateDB()
    {
        try
        {
            if (ID == 0)
            {
                string query = "INSERT INTO PlayersStatus (";
                query += "Name,";
                query += "PosX,";
                query += "PosY,";
                query += "PosZ,";
                query += "RotX,";
                query += "RotY,";
                query += "RotZ";
                query += ") VALUES (";
                query += "" + 0 + ",";
                query += "" + 0 + ",";
                query += "" + 0 + ",";
                query += "" + 0 + ",";
                query += "" + 0 + ",";
                query += "" + 0 + ",";
                query += "" + 0;
                query += "); ";
                query += "SELECT LAST_INSERT_ID();";
                cmd.CommandText = query;
                ID = Convert.ToInt32(cmd.ExecuteScalar());
                //Debug.Log(ID);
            }
            else
            {
                if (seconds >= (lastSecond + updateFrequencySeconds))
                {
                    string query = "UPDATE PlayersStatus ";
                    query += "SET Name = " + 0 + ", ";
                    query += "PosX = " + Mathf.Floor(transform.position.x) + ", ";
                    query += "PosY = " + Mathf.Floor(transform.position.y) + ", ";
                    query += "PosZ = " + Mathf.Floor(transform.position.z) + ", ";
                    query += "RotX = " + Mathf.Floor(transform.eulerAngles.x) + ", ";
                    query += "RotY = " + Mathf.Floor(transform.eulerAngles.y) + ", ";
                    query += "RotZ = " + Mathf.Floor(transform.eulerAngles.z) + " ";
                    query += "WHERE ID = " + ID;
                    cmd.CommandText = query;
                    Debug.Log(query);
                    cmd.ExecuteScalar();

                    lastSecond = seconds;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);       
        }
    }*/

    void DisconnectPlayer()
    {
        
    }
}
