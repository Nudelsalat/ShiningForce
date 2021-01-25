using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform MovePoint;
    public float MoveSpeed = 5f;
    public LayerMask Collider;
    public LayerMask StairCollider;
    public static bool IsInDialogue = false;
    public static bool InputDisabled = false;
    public static bool InputDisabledInDialogue = false;
    public GameObject _interactionSelector;
    
    private Vector2 _movement;
    private Animator _animator;

    // Start is called before the first frame update
    void Start() {
        _animator = GetComponent<Animator>();
        MovePoint.parent = null;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private void FixedUpdate() {
        HandleMovement();
    }

    private void HandleDialogue() {
        if (Input.GetButtonDown("Interact")) {
            FindObjectOfType<DialogManager>().DisplayNextSentence();
        }
    }

    private void HandleInput() {
        if (InputDisabled || InputDisabledInDialogue) {
            _movement.x = _movement.y = 0;
            return;
        } else if (IsInDialogue) {
            _movement.x = _movement.y = 0;
            HandleDialogue();
            return;
        }   

        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");
    }

    private void HandleMovement() {
        transform.position = Vector3.MoveTowards(transform.position, MovePoint.position, MoveSpeed * Time.deltaTime);
        if (!(Vector3.Distance(transform.position, MovePoint.position) <= 0.005f)) {
            return;
        }

        _animator.speed = 1;

        // order is important:
        // still moves player towards movePoint, but does not modify movePoint
        if (IsInDialogue) {
            return;
        }

        if (Math.Abs(Mathf.Abs(_movement.x) - 1f) < 0.01f) {
            if (_movement.x > 0) {
                _interactionSelector.transform.position = new Vector3(transform.position.x + 0.75f,
                    transform.position.y - 0.25f, transform.position.z);
                _animator.Play("swordsMan_right_move");
            } else if (_movement.x < 0) {

                _interactionSelector.transform.position = new Vector3(transform.position.x - 0.75f,
                    transform.position.y - 0.25f, transform.position.z);
                _animator.Play("swordsMan_left_move");
            }

            if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, 0f, 0f), .2f,
                Collider)) {
                return;
            } else if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, 0f, 0f), .2f,
                StairCollider)) {
                MovePoint.position += new Vector3(_movement.x*2, 2f, 0f);
                return;
            }

            MovePoint.position += new Vector3(_movement.x, 0f, 0f);


            _animator.speed = 2;
        } else if (Math.Abs(Mathf.Abs(_movement.y) - 1f) < 0.01f) {
            if (_movement.y > 0) {
                _interactionSelector.transform.position = new Vector3(transform.position.x,
                    transform.position.y + 0.5f, transform.position.z);
                _animator.Play("swordsMan_back_move");
            } else if (_movement.y < 0) {
                _interactionSelector.transform.position = new Vector3(transform.position.x,
                    transform.position.y - 1f, transform.position.z);
                _animator.Play("swordsMan_front_move");
            }

            if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(0f, _movement.y, 0f), .2f,
                Collider)) {
                return;
            }
            MovePoint.position += new Vector3(0f, _movement.y, 0f);
            _animator.speed = 2;
        }
    }
}
