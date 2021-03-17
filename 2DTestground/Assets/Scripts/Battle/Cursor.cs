using System;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Cursor : MonoBehaviour {
    public Transform MovePoint;
    public float MoveSpeed = 5f;
    public LayerMask Collider;
    public LayerMask StairCollider;
    public static bool IsInDialogue = false;
    public static bool InputDisabledInDialogue = false;
    public static bool InputDisabledInEvent = false;
    public static EnumMenuType PlayerIsInMenu = EnumMenuType.none;

    private Vector2 _movement;
    private Animator _animator;
    private Tilemap _terrainTileMap;

    // Start is called before the first frame update
    void Start() {
        _terrainTileMap = GameObject.Find("Terrain").GetComponent<Tilemap>();
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

    private void HandleInput() {
        if (InputDisabledInDialogue || InputDisabledInEvent || PlayerIsInMenu != EnumMenuType.none) {
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
            if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, 0f, 0f), .2f,
                Collider)) {
                return;
            } else if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(_movement.x, 0f, 0f), .2f,
                StairCollider)) {
                MovePoint.position += new Vector3(_movement.x * 2, 2f, 0f);
                return;
            }
            MovePoint.position += new Vector3(_movement.x, 0f, 0f);
            _animator.speed = 2;
            CheckTile();
        } else if (Math.Abs(Mathf.Abs(_movement.y) - 1f) < 0.01f) {
            
            if (Physics2D.OverlapCircle(MovePoint.position + new Vector3(0f, _movement.y, 0f), .2f,
                Collider)) {
                return;
            }
            MovePoint.position += new Vector3(0f, _movement.y, 0f);
            _animator.speed = 2;
            CheckTile();
        }
    }

    private void CheckTile() {
        var test = _terrainTileMap.WorldToCell(MovePoint.position);
        var sprite = _terrainTileMap.GetSprite(test);
        Debug.Log("Your on tile:" + sprite.name);
    }
}

