﻿using UnityEngine;

public class RoamWithinBox : MonoBehaviour {
    public float moveEveryXSeconds = 1;
    public float moveSpeed = 2f;
    public GameObject _path;


    private LayerMask _playerCollision;
    private bool _moveing = false;
    private float _moveEveryXSeconds;
    private Vector3 _nextPoint;
    private Animator _animator;

    // Start is called before the first frame update
    void Awake() {
        _animator = GetComponent<Animator>();
        _moveEveryXSeconds = Random.Range(moveEveryXSeconds * 0.75f, moveEveryXSeconds * 1.25f);
        _nextPoint = transform.position;
        _playerCollision = LayerMask.GetMask("Player");
    }

    // Update is called once per frame
    void Update() {
        if (Player.IsInDialogue) {
            return;
        }

        if (!(Vector3.Distance(transform.position, _nextPoint) <= 0.005f)) {
            transform.position = Vector3.MoveTowards(transform.position, _nextPoint, moveSpeed * Time.deltaTime);
        }

        if (_moveing) {
            HandleMovement();
            _moveing = false;
        }
        else {
            _moveEveryXSeconds -= Time.deltaTime;
            if (_moveEveryXSeconds <= 0) {
                _moveEveryXSeconds = Random.Range(moveEveryXSeconds * 0.75f, moveEveryXSeconds * 1.25f);
                _moveing = true;
            }
        }
    }

    void HandleMovement() {
        var currentPos = _nextPoint;
        var moveDirection = 2;
        var cycleCount = 0;
        do {
            currentPos = _nextPoint;
            moveDirection = Random.Range(0, 4); // 0 = up; 1 = left; 2 = down; 3 = right MAX -> Excludes...
            switch (moveDirection) {
                case 0:
                    currentPos.y += 1;
                    break;
                case 1:
                    currentPos.x -= 1;
                    break;
                case 2:
                    currentPos.y -= 1;
                    break;
                case 3:
                    currentPos.x += 1;
                    break;
            }

            //otherwise it might soft-lock, if the player traps an NPC.
            cycleCount++;
            if (cycleCount >= 10) {
                return;
            }

        } while ((!_path.GetComponent<Collider2D>().OverlapPoint(currentPos) ||
                 Physics2D.OverlapCircle(currentPos, .2f, _playerCollision)));

        _animator.SetInteger("moveDirection", moveDirection);
        _nextPoint = currentPos;
    }
}