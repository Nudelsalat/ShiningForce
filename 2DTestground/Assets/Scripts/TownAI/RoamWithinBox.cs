using Assets.Scripts.Dialog;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

public class RoamWithinBox : AbstractDialogHolder {
    public MonoBehaviour FollowUpEvent;

    public float MoveEveryXSeconds = 2;
    public float MoveSpeed = 1f;
    public GameObject Path;


    private LayerMask _playerCollision;
    private bool _moveing = false;
    private float _moveEveryXSeconds;
    private GameObject _nextPoint;
    private DirectionType _moveDirection = DirectionType.down;
    private DirectionType _playerDirection;
    private Animator _animator;

    // Start is called before the first frame update
    void Awake() {
        _animator = GetComponent<Animator>();
        _moveEveryXSeconds = Random.Range(MoveEveryXSeconds * 0.75f, MoveEveryXSeconds * 1.25f);
        _playerCollision = LayerMask.GetMask("Player");

        _nextPoint = new GameObject("BlockPathForPlayer");
        _nextPoint.transform.SetParent(this.transform.parent);
        _nextPoint.layer = LayerMask.NameToLayer("Collision");
        _nextPoint.AddComponent<BoxCollider2D>();
        _nextPoint.GetComponent<BoxCollider2D>().size = new Vector2(0.5f,0.5f);
        _nextPoint.transform.position = transform.position;
    }

    // Update is called once per frame
    public override void Update() {
        base.Update();

        if (Player.IsInDialogue || Player.InWarp || Player.InputDisabledInDialogue
            || Player.InputDisabledInEvent || Player.PlayerIsInMenu != EnumMenuType.none) {
            return;
        }
        if (!(Vector3.Distance(transform.position, _nextPoint.transform.position) <= 0.005f)) {
            transform.position = Vector3.MoveTowards(transform.position, _nextPoint.transform.position,
                MoveSpeed * Time.deltaTime);
            _animator.SetInteger("moveDirection", (int)_moveDirection);
        }

        if (_moveing) {
            HandleMovement();
            _moveing = false;
        }
        else {
            _moveEveryXSeconds -= Time.deltaTime;
            if (_moveEveryXSeconds <= 0) {
                _moveEveryXSeconds = Random.Range(MoveEveryXSeconds * 0.75f, MoveEveryXSeconds * 1.25f);
                _moveing = true;
            }
        }
    }

    void FixedUpdate() {
        if (Player.IsInDialogue || Player.InWarp || Player.InputDisabledInDialogue
            || Player.InputDisabledInEvent || Player.PlayerIsInMenu != EnumMenuType.none) {
            if (_playerDirection != DirectionType.none) {
                _animator.SetInteger("moveDirection", (int)_playerDirection);
            }
            return;
        }
        if (!(Vector3.Distance(transform.position, _nextPoint.transform.position) <= 0.005f)) {
            transform.position = Vector3.MoveTowards(transform.position, _nextPoint.transform.position, MoveSpeed * Time.deltaTime);
            _playerDirection = DirectionType.none;
            _animator.SetInteger("moveDirection", (int)_moveDirection);
        }

        if (_moveing) {
            HandleMovement();
            _moveing = false;
        } else {
            _moveEveryXSeconds -= Time.deltaTime;
            if (_moveEveryXSeconds <= 0) {
                _moveEveryXSeconds = Random.Range(MoveEveryXSeconds * 0.75f, MoveEveryXSeconds * 1.25f);
                _moveing = true;
            }
        }
    }

    void HandleMovement() {
        Vector3 currentPos;
        var cycleCount = 0;
        DirectionType moveDirection;
        do {
            currentPos = _nextPoint.transform.position;
            moveDirection = (DirectionType)Random.Range(0, 4); // 0 = up; 1 = left; 2 = down; 3 = right MAX -> Excludes...
            switch (moveDirection) {
                case DirectionType.up:
                    currentPos.y += 1;
                    break;
                case DirectionType.left:
                    currentPos.x -= 1;
                    break;
                case DirectionType.down:
                    currentPos.y -= 1;
                    break;
                case DirectionType.right:
                    currentPos.x += 1;
                    break;
            }

            //otherwise it might soft-lock, if the player traps an NPC.
            cycleCount++;
            if (cycleCount >= 10) {
                return;
            }

        } while ((!Path.GetComponent<Collider2D>().OverlapPoint(currentPos) ||
                 Physics2D.OverlapCircle(currentPos, .2f, _playerCollision)));

        _moveDirection = moveDirection;
        _animator.SetInteger("moveDirection", (int)_moveDirection);
        _nextPoint.transform.position = currentPos;
    }

    public override void TriggerDialogue() {
        FindPlayerDirection();
        if (FollowUpEvent != null) {
            FollowUpEvent.Invoke("EventTrigger", 0);
        }
    }

    private void FindPlayerDirection() {
        var currentPos = transform.position;
        for (var direction = 0; direction < 4; direction++) {
            var lookingForPlayer = currentPos;
            switch (direction) {
                case 0:
                    lookingForPlayer.y += 1;
                    break;
                case 1:
                    lookingForPlayer.x -= 1;
                    break;
                case 2:
                    lookingForPlayer.y -= 1;
                    break;
                case 3:
                    lookingForPlayer.x += 1;
                    break;
            }
            if (Physics2D.OverlapCircle(lookingForPlayer, .2f, _playerCollision)) {
                _animator.SetInteger("moveDirection", direction);
                _playerDirection = (DirectionType)direction;
                return;
            }
        }
    }
}
