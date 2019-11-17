using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    Rigidbody rb;

    [Range(10, 100)]
    public float speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            rb.AddRelativeForce(Vector3.forward * Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.A))
            rb.AddRelativeForce(Vector3.left * Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.S))
            rb.AddRelativeForce(Vector3.back * Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.D))
            rb.AddRelativeForce(Vector3.right * Time.deltaTime * speed);

        transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0));
    }
}
