using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FreeCamera : MonoBehaviour
{
    public GameObject rememberedParent;


    public float rotSpeed = 1;
    public float moveSpeed = 5;

    public int count = 0;

    public int smoothingFrames;

    private float rotX, rotY;
    private float[] smoothX;
    private float[] smoothY;

    public bool free;

    private Vector3[] avgMove;

    Vector3 offsetWhenFreed;
    Quaternion rotWhenFreed;
    public GameObject hud;

    private void Start()
    {
        smoothX = new float[smoothingFrames];
        smoothY = new float[smoothingFrames];
        avgMove = new Vector3[smoothingFrames];

        for (int i = 0; i < smoothingFrames; i++)
        {
            smoothX[i] = 0;
            smoothY[i] = 0;
            avgMove[i] = Vector3.zero;
        }

        rotX = transform.eulerAngles.x;
        rotY = transform.eulerAngles.y;

        hud = GameObject.FindGameObjectWithTag("GNECanvas");
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if(free)
            {
                hud.SetActive(true);
                transform.parent = rememberedParent.transform;
                //transform.localPosition= Vector3.zero;
                //transform.localRotation= Quaternion.identity;

                transform.localPosition = offsetWhenFreed;
                transform.localRotation = rotWhenFreed;

                //transform.position += new Vector3(0, 2, -2) * rememberedParent.transform.rotation;
            }
            else
            {
                hud.SetActive(false);
                offsetWhenFreed = transform.localPosition;
                rotWhenFreed = transform.localRotation;


                rememberedParent = transform.parent.gameObject;
                transform.parent = null;
            }

            free = !free;
        }

        if (!free) return;

        count++;
        if (count == smoothingFrames) count = 0;

        moveSpeed += Input.GetAxis("Mouse ScrollWheel") * 5;

        #region Absorb Mouse Movement
        smoothX[count] = Input.GetAxis("Mouse X") * GameManager.Instance.mouseSensitivity;
        smoothY[count] = Input.GetAxis("Mouse Y") * GameManager.Instance.mouseSensitivity;
        #endregion



        rotX -= Average(smoothY) * rotSpeed;
        rotY += Average(smoothX) * rotSpeed;
        rotX %= 360;
        rotY %= 360;
        transform.eulerAngles = new Vector3(rotX, rotY, 0);



        Vector3 input = new Vector3(
            (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)).normalized;

        avgMove[count] = input;


        transform.Translate(smoothMovement() * moveSpeed * Time.deltaTime);
    }


    private float Average(float[] array)
    {
        float total = 0;
        for (int i = 0; i < array.Length; i++)
        {
            total += array[i];
        }
        return total / array.Length;
    }

    private Vector3 smoothMovement()
    {
        float x = 0, y = 0, z = 0;

        for (int i = 0; i < avgMove.Length; i++)
        {
            x += avgMove[i].x;
            y += avgMove[i].y;
            z += avgMove[i].z;

        }

        x /= avgMove.Length;
        y /= avgMove.Length;
        z /= avgMove.Length;

        return new Vector3(x, y, z);
    }

}
