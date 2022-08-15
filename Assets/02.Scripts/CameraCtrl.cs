using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    KeyCode leftMouse = KeyCode.Mouse0, rightMouse = KeyCode.Mouse1, middleMouse = KeyCode.Mouse2;

    public float CameraHeight = 1.75f, CameraMaxDistance = 25;
    float CameraMaxTilt = 90;
    [Range(0, 4)]
    public float CameraSpeed = 2;
    float currentPan, currentTilt = 10, currentDistance = 5;
    [HideInInspector]
    public bool autoRunReset = false;

    float panAngle, panOffset;
    bool camXAdjust, camYAdjust;
    float rotationXCushion = 3, rotationXSpeed = 0;
    float yRotMin = 0, yRotMax = 20, rotationYSpeed = 0;

    public CameraState cameraState = CameraState.CameraNone;

    [Range(0.25f, 1.75f)]
    public float cameraAdjsutSpeed = 1;
    public CameraMoveState camMoveState = CameraMoveState.OnlyWhileMoving;

    PlayerMove player;
    public Transform tilt;
    Camera mainCam;

    private void Start()
    {
        player = FindObjectOfType<PlayerMove>();
        player.mainCam = this;
        mainCam = Camera.main;

        transform.position = player.transform.position + Vector3.up * CameraHeight;
        transform.rotation = player.transform.rotation;

        tilt.eulerAngles = new Vector3(currentTilt, transform.eulerAngles.y, transform.eulerAngles.z);
        mainCam.transform.position += tilt.forward * -currentDistance;
    }

    private void Update()
    {
        if (!Input.GetKey(leftMouse) && !Input.GetKey(rightMouse) && !Input.GetKey(middleMouse))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cameraState = CameraState.CameraNone;
        }
        else if (Input.GetKey(leftMouse) && !Input.GetKey(rightMouse) && !Input.GetKey(middleMouse))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraState = CameraState.CameraRotate;
        }
        else if (!Input.GetKey(leftMouse) && Input.GetKey(rightMouse) && !Input.GetKey(middleMouse))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraState = CameraState.CameraSteer;
        }
        else if ((Input.GetKey(leftMouse) && Input.GetKey(rightMouse)) || Input.GetKey(middleMouse))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraState = CameraState.CameraRun;
        }

        CameraInputs();
    }

    private void LateUpdate()
    {
        panAngle = Vector3.SignedAngle(transform.forward, player.transform.forward, Vector3.up);

        switch(camMoveState)
        {
            case CameraMoveState.OnlyWhileMoving:
                if (player.moveDir.magnitude > 0 || player.hr != 0)
                {
                    CameraXAdjust();
                    CameraYAdjust();
                }
                break;

            case CameraMoveState.OnlyHorizontalWhileMoving:
                if (player.moveDir.magnitude > 0 || player.hr != 0)
                    CameraXAdjust();
                break;

            case CameraMoveState.AlwaysAdjust:
                CameraXAdjust();
                CameraYAdjust();
                break;

            case CameraMoveState.NeverAdjust:
                CameraNeverAdjust();
                break;
        }

        CameraTransforms();
    }

    void CameraInputs()
    {
        if (cameraState != CameraState.CameraNone)
        {
            if (!camYAdjust && (camMoveState == CameraMoveState.AlwaysAdjust || camMoveState == CameraMoveState.OnlyWhileMoving))
                camYAdjust = true;

            if (cameraState == CameraState.CameraRotate)
            {
                if (!camXAdjust && camMoveState != CameraMoveState.NeverAdjust)
                    camXAdjust = true;

                if (player.steer)
                    player.steer = false;

                currentPan += Input.GetAxis("Mouse X") * CameraSpeed;
            }
            else if (cameraState == CameraState.CameraSteer || cameraState == CameraState.CameraRun)
            {
                if (!player.steer)
                {
                    Vector3 playerReset = player.transform.eulerAngles;
                    playerReset.y = transform.eulerAngles.y;

                    player.transform.eulerAngles = playerReset;

                    player.steer = true;
                }
            }

            currentTilt -= Input.GetAxis("Mouse Y") * CameraSpeed;
            currentTilt = Mathf.Clamp(currentTilt, -CameraMaxTilt, CameraMaxTilt);
        }
        else
        {
            if (player.steer)
                player.steer = false;
        }

        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * 2;
        currentDistance = Mathf.Clamp(currentDistance, 0, CameraMaxDistance);
    }

    void CameraXAdjust()
    {
        if (cameraState != CameraState.CameraRotate)
        {
            if (camXAdjust)
            {
                rotationXSpeed += Time.deltaTime * cameraAdjsutSpeed;

                if (Mathf.Abs(panAngle) > rotationXCushion)
                    currentPan = Mathf.Lerp(currentPan, currentPan + panAngle, rotationXSpeed);
                else
                    camXAdjust = false;
            }
            else
            {
                if (rotationXSpeed > 0)
                    rotationXSpeed = 0;

                currentPan = player.transform.eulerAngles.y;
            }
        }
    }

    void CameraYAdjust()
    {
        if (cameraState == CameraState.CameraNone)
        {
            if (camYAdjust)
            {
                rotationYSpeed += (Time.deltaTime / 2) * cameraAdjsutSpeed;

                if (currentTilt >= yRotMax || currentTilt <= yRotMin)
                    currentTilt = Mathf.Lerp(currentTilt, yRotMax / 2, rotationYSpeed);
                else if (currentTilt < yRotMax && currentTilt > yRotMin)
                    camYAdjust = false;
            }
            else
            {
                if (rotationYSpeed > 0)
                    rotationYSpeed = 0;
            }
        }
    }

    void CameraNeverAdjust()
    {
        switch (cameraState)
        {
            case CameraState.CameraSteer:
            case CameraState.CameraRun:
                if (panOffset != 0)
                    panOffset = 0;

                currentPan = player.transform.eulerAngles.y;
                break;

            case CameraState.CameraNone:
                currentPan = player.transform.eulerAngles.y - panOffset;
                break;

            case CameraState.CameraRotate:
                panOffset = panAngle;
                break;
        }
    }

    void CameraTransforms()
    {
        if (cameraState == CameraState.CameraRun)
        {
            player.autoRun = true;

            if (!autoRunReset)
                autoRunReset = true;
        }
        else
        {
            if (autoRunReset)
            {
                player.autoRun = false;
                autoRunReset = false;
            }
        }

        transform.position = player.transform.position + Vector3.up * CameraHeight;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentPan, transform.eulerAngles.z);
        tilt.eulerAngles = new Vector3(currentTilt, tilt.eulerAngles.y, tilt.eulerAngles.z);
        mainCam.transform.position = transform.position + tilt.forward * -currentDistance;
    }

    public enum CameraState { CameraNone, CameraRotate, CameraSteer, CameraRun }

    public enum CameraMoveState { OnlyWhileMoving, OnlyHorizontalWhileMoving, AlwaysAdjust, NeverAdjust }
}