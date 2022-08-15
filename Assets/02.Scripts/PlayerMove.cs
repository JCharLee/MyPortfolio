using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Transform playerTr;
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotSpeed = 100f;
    [SerializeField] private float jumpForce = 5f;
    private float h, v;
    [HideInInspector]
    public Vector3 moveDir;
    [HideInInspector]
    public float hr;
    private bool isJump = false;
    [HideInInspector]
    public bool steer;
    public bool autoRun;

    [HideInInspector]
    public CameraCtrl mainCam;


    private void Awake()
    {
        playerTr = GetComponent<Transform>();
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
            autoRun = !autoRun;

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        if (v != 0 && !mainCam.autoRunReset)
            autoRun = false;

        if (autoRun)
        {
            v += 1;

            v = Mathf.Clamp(v, -1, 1);
        }

        if (steer)
        {
            h += Input.GetAxis("Horizontal2");
            h = Mathf.Clamp(h, -1, 1);
        }
        if (steer)
            hr = Input.GetAxis("Mouse X") * mainCam.CameraSpeed * 2f;
        else
            hr = Input.GetAxis("Horizontal2");

        moveDir = (Vector3.forward * v) + (Vector3.right * h);
        playerTr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        playerTr.Rotate(Vector3.up * hr * rotSpeed * Time.deltaTime);

        if (h != 0 || v != 0)
            anim.SetBool("IsMove", true);
        else
            anim.SetBool("IsMove", false);
        anim.SetFloat("SpeedX", h);
        anim.SetFloat("SpeedY", v);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJump)
            {
                isJump = true;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("GROUND"))
            isJump = false;
    }
}