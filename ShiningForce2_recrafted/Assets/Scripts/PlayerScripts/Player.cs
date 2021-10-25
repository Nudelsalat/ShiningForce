using System;
using System.Collections;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform MovePoint;
    public float MoveSpeed = 5f;
    public LayerMask Collider;
    public LayerMask StairColliderLeftUp;
    public LayerMask StairColliderRightUp;
    public static bool IsInDialogue = false;
    public static bool InputDisabledInDialogue = false;
    public static bool InputDisabledAiBattle = false;
    public static bool InputDisabledInAttackPhase = false;
    public static bool InputDisabledInEvent { get; private set; }

    public static bool InWarp = false;
    public static bool _doUnsetInputDisabledInEvent = false;
    public static EnumMenuType PlayerIsInMenu = EnumMenuType.none;
    public GameObject _interactionSelector;
    
    private Vector2 _movement;
    private Animator _animator;

    public static Player Instance;

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
        _animator = GetComponent<Animator>();
        MovePoint.parent = null;
        MovePoint.position = this.transform.position;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private void FixedUpdate() {
        HandleMovement();
    }
    public void SetPosition(Vector3 position) {
        transform.position = position;
        MovePoint.position = position;
    }

    public void SetInputDisabledInEvent() {
        InputDisabledInEvent = true;
        _doUnsetInputDisabledInEvent = false;
    }

    public void UnsetInputDisabledInEvent() {
        var isSetActive = gameObject.activeSelf;
        gameObject.SetActive(true);
        _doUnsetInputDisabledInEvent = true;
        StartCoroutine(WaitTillEndOfFrame(isSetActive));
    }

    private void HandleInput() {
        if (InputDisabledInDialogue || Player.InWarp || InputDisabledInEvent || InputDisabledInAttackPhase || PlayerIsInMenu != EnumMenuType.none) {
            _movement.x = _movement.y = 0;
            return;
        } else if (IsInDialogue) {
            _movement.x = _movement.y = 0;
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
                _animator.SetInteger("moveDirection", 3);
            } else if (_movement.x < 0) {

                _interactionSelector.transform.position = new Vector3(transform.position.x - 0.75f,
                    transform.position.y - 0.25f, transform.position.z);
                _animator.SetInteger("moveDirection", 1);
            }

            if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, 0f, 0f), .2f,
                Collider)) {
                return;
            } else if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(0f, 0f, 0f), .2f,
                StairColliderLeftUp)) {
                if (_movement.x > 0 && Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, -1f, 0f),
                        .2f, StairColliderLeftUp)) {
                    MovePoint.position += new Vector3(_movement.x, -1f, 0f);
                    return;
                } else if (_movement.x < 0 && Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, 1f, 0f),
                               .2f, StairColliderLeftUp)) {
                    MovePoint.position += new Vector3(_movement.x, 1f, 0f);
                    return;
                }
            } else if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(0f, 0f, 0f), .2f,
                StairColliderRightUp)) {
                if (_movement.x > 0 && Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, 1f, 0f), 
                        .2f, StairColliderRightUp)) {
                    MovePoint.position += new Vector3(_movement.x, 1f, 0f);
                    return;
                } else if (_movement.x < 0 && Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, -1f, 0f), 
                               .2f, StairColliderRightUp)) {
                    MovePoint.position += new Vector3(_movement.x, -1f, 0f);
                    return;
                }
            }

            MovePoint.position += new Vector3(_movement.x, 0f, 0f);


            _animator.speed = 2;
        } else if (Math.Abs(Mathf.Abs(_movement.y) - 1f) < 0.01f) {
            if (_movement.y > 0) {
                _interactionSelector.transform.position = new Vector3(transform.position.x,
                    transform.position.y + 0.5f, transform.position.z);
                _animator.SetInteger("moveDirection", 0);
            } else if (_movement.y < 0) {
                _interactionSelector.transform.position = new Vector3(transform.position.x,
                    transform.position.y - 1f, transform.position.z);
                _animator.SetInteger("moveDirection", 2);
            }

            if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(0f, _movement.y, 0f), .2f,
                Collider)) {
                return;
            }
            MovePoint.position += new Vector3(0f, _movement.y, 0f);
            _animator.speed = 2;
        }
    }

    IEnumerator WaitTillEndOfFrame(bool wasActive) {
        yield return new WaitForEndOfFrame();
        if (_doUnsetInputDisabledInEvent) {
            InputDisabledInEvent = false;
            gameObject.SetActive(wasActive);
        }
    }
}

