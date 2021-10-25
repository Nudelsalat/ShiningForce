﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverviewCameraMovement : MonoBehaviour {
    public Transform PlayerObject;
    public float CameraOffsetx;
    public float CameraOffsety;
    public float MoveSpeed;
    private float _initialSpeed;

    public static OverviewCameraMovement Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start() {
        _initialSpeed = MoveSpeed;
        transform.position = new Vector3(PlayerObject.transform.position.x, PlayerObject.transform.position.y, 
            transform.position.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!(Mathf.Abs(transform.position.x - PlayerObject.position.x) <= CameraOffsetx)) {
            transform.position = Vector3.MoveTowards(transform.position, 
                new Vector3(PlayerObject.position.x, transform.position.y, transform.position.z), MoveSpeed * Time.deltaTime);
        }

        if (!(Mathf.Abs(transform.position.y - PlayerObject.position.y) <= CameraOffsety)) {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(transform.position.x, PlayerObject.position.y, transform.position.z), MoveSpeed * Time.deltaTime);
        }

        if ((Mathf.Abs(transform.position.x - PlayerObject.position.x) >= CameraOffsetx + 2) ||
                   (Mathf.Abs(transform.position.y - PlayerObject.position.y) >= CameraOffsety + 1)) {
            var cursor = PlayerObject.GetComponent<Cursor>();
            var player = PlayerObject.GetComponent<Player>();
            if (cursor != null) {
                MoveSpeed = cursor.MoveSpeed*0.95f;
            } else if (player != null) {
                MoveSpeed = player.MoveSpeed;
            } else {
                MoveSpeed = _initialSpeed * 1.2f;
            }
        } else {
            MoveSpeed = _initialSpeed;
        }
    }

    public void SetPlayerObject(GameObject playerToFollow) {
        PlayerObject = playerToFollow.gameObject.transform;
        transform.position = new Vector3(PlayerObject.position.x, PlayerObject.position.y, transform.position.z);
    }
}