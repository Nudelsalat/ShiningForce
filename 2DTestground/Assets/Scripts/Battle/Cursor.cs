using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.GameData;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Cursor : MonoBehaviour {
    public Transform MovePoint;
    public float MoveSpeed = 8f;
    public LayerMask Collider;
    public LayerMask BattleCollider;
    public static bool IsInDialogue = false;
    public static bool InputDisabledInDialogue = false;
    public static bool InputDisabledInEvent = false;
    public EnumMenuType PlayerIsInMenu = EnumMenuType.none;

    private Vector2 _movement;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Tilemap _terrainTileMap;
    private Queue<Vector3> _setPath;
    private LandeffectUi _landEffect;
    private QuickInfoUi _quickInfo;
    private AudioClip _audioClipMovementNoise;
    private Transform _areaOfEffectSpawnPoint;
    private GameObject _areaOfEffect;
    private bool _isMoveInBattleSquares = false;
    private bool _unitReached = false;
    private bool _endTurn = false;
    private bool _clearControlUnitAfterMovement = false;
    private bool _movementNoise = false;
    private float _movementNoiseInterval = 0.2f;
    private Unit _currentUnit = null;
    private float _initialSpeed;

    private AudioManager _audioManager;
    private BattleController _battleController;

    public static Cursor Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioClipMovementNoise = Resources.Load<AudioClip>(Constants.SoundMovement);
        _areaOfEffectSpawnPoint = transform.Find("SpawnAreaOfEffect").GetComponent<Transform>();
    }

    // Start is called before the first frame update
    void Start() {
        _audioManager = AudioManager.Instance;
        _battleController = BattleController.Instance;
        _terrainTileMap = GameObject.Find("Terrain").GetComponent<Tilemap>();
        _landEffect = LandeffectUi.Instance;
        _quickInfo = QuickInfoUi.Instance;
        _isMoveInBattleSquares = false;
        _initialSpeed = MoveSpeed;
        MovePoint.parent = null;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private void FixedUpdate() {
        HandleMovement();
    }

    public void EndBattle() {
        _landEffect.CloseLandEffect();
        _quickInfo.CloseQuickInfo();
    }

    public void SetMoveWithinBattleSquares() {
        _isMoveInBattleSquares = true;
    }
    public void UnsetMoveWithinBattleSquares() {
        _isMoveInBattleSquares = false;
    }

    public void SetAttackArea(Vector3 target, EnumAreaOfEffect areaOfEffect) {
        _spriteRenderer.color = Constants.Invisible;
        var path = Constants.PrefabPrefixAreaOfEffect + Enum.GetName(typeof(EnumAreaOfEffect), areaOfEffect);
        var spawnItem = Resources.Load(path) as GameObject;
        _areaOfEffect = Instantiate(spawnItem, new Vector3(0, 0, _areaOfEffectSpawnPoint.position.z),
            _areaOfEffectSpawnPoint.rotation);
        _areaOfEffect.transform.SetParent(_areaOfEffectSpawnPoint, false);
        MovePoint.position = target;
    }

    public void ClearAttackArea() {
        _spriteRenderer.color = Constants.Visible;
        Destroy(_areaOfEffect);
    }

    public List<Unit> GetTargetsInAreaOfEffect(LayerMask layerMask) {
        if (_areaOfEffect == null) {
            return null;
        } 
        var colliderManager = _areaOfEffect.GetComponent<AreaOfEffectColliderManager>();

        if (colliderManager.name.Contains(Enum.GetName(typeof(EnumAreaOfEffect), EnumAreaOfEffect.AllAllies))) {
            //TODO GET all allies of Unit -> via Battle Controller
        }
        var colliderList = colliderManager.GetAllCurrentCollider(layerMask);
        var results = new List<Unit>();
        foreach (var colliderEntry in colliderList) {
            var unit = colliderEntry.GetComponent<Unit>();
            if (unit != null) {
                results.Add(unit);
            }
        }

        return results;
    }

    public bool IsTargetSelected() {
        if (_areaOfEffect == null) {
            Debug.LogError("_areaOfEffect of Cursor is null, whilest checking if target " +
                           "is selected. this SHOULD not happen...");
            return false;
        }
        return Vector3.Distance(transform.position, MovePoint.position) <= 0.0005f;
    }

    public bool CheckIfCursorIsOverUnit(out Unit unit, LayerMask layerMask) {
        var overlappedObject = Physics2D.OverlapCircle(transform.position, 0.2f, layerMask);

        if (overlappedObject == null) {
            unit = null;
            return false;
        }

        unit = overlappedObject.GetComponent<Unit>();
        return unit != null;
    }

    public void ReturnToUnit(Vector3 origPosition) {
        var position = MovePoint.position;
        var pathfinder = new Pathfinder(null, position, origPosition);
        var result = pathfinder.GetShortestPath();
        _setPath = new Queue<Vector3>(result);
        MoveSpeed = 20f;
    }

    public void ReturnToPosition(List<GameObject> walkableSprites, Vector3 origPosition) {
        var walkablePoints = new List<Vector3>();
        if (walkableSprites == null) {
            walkablePoints = null;
        } else {
            walkablePoints.AddRange(walkableSprites.Select(x => x.transform.position));
            //cursor and unit has Offset...
            walkablePoints = walkablePoints.Select(x => { x.y += 0.25f;
                return x;
            }).ToList();
        }

        var position = MovePoint.position;
        var pathfinder = new Pathfinder(walkablePoints, position,origPosition);
        var result = pathfinder.GetShortestPath();
        _setPath = new Queue<Vector3>(result);
        MoveSpeed = 20f;
        ClearControlUnit();
    }

    public void SetControlUnit(Unit unit) {
        _spriteRenderer.color = Constants.Invisible;
        _currentUnit = unit;
        //BUGFIX: could move unity on blocked terrain, as MovePoint was a square ahead while unit was selected
        MovePoint.position = _currentUnit.transform.position;
    }

    public void EndTurn() {
        _endTurn = true;
        ClearControlUnit();
    }

    public void ClearControlUnit(bool immediately = false) {
        if (immediately) {
            DoClearControlUnit();
            return;
        }
        _clearControlUnitAfterMovement = true;
    }

    public void DoClearControlUnit() {
        _spriteRenderer.color = Constants.Visible;
        //reset unit to look down
        _currentUnit?.SetAnimatorDirection(DirectionType.down);
        _currentUnit?.SetUnitFlicker();
        MoveSpeed = _initialSpeed;
        _clearControlUnitAfterMovement = false;
        if (_endTurn) {
            _endTurn = false;
            _currentUnit?.ClearUnitFlicker();
            _battleController.NextUnit();
        }
        _currentUnit = null;
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
        transform.position =
            Vector3.MoveTowards(transform.position, MovePoint.position, MoveSpeed * Time.deltaTime);
        if (_currentUnit) {
            _currentUnit.transform.position =
                Vector3.MoveTowards(_currentUnit.transform.position, MovePoint.position, MoveSpeed * Time.deltaTime);
        }

        if (!(Vector3.Distance(transform.position, MovePoint.position) <= 0.0005f)) {
            if (_currentUnit && !_movementNoise &&
                !(Vector3.Distance(_currentUnit.transform.position, MovePoint.position) <= 0.0005f)) {
                StartCoroutine("MovementNoise");
            }
            return;
        }
        

        if (DoPredefinedMovement()) {
            return;
        }

        if (_clearControlUnitAfterMovement) {
            DoClearControlUnit();
        }

        // order is important:
        // still moves player towards movePoint, but does not modify movePoint
        if (IsInDialogue) {
            return;
        }
        
        if (Math.Abs(Mathf.Abs(_movement.x) - 1f) < 0.01f) {
            if (CheckForCollider(MovePoint.position + new Vector3(_movement.x, 0f, 0f))) {
                StopMovementAnimationAndNoise();
                return;
            }
            MovePoint.position += new Vector3(_movement.x, 0f, 0f);
            _animator.speed = 2;
            _currentUnit?.SetAnimatorSpeed(2);
            CheckTile();
        } else if (Math.Abs(Mathf.Abs(_movement.y) - 1f) < 0.01f) {
            if (CheckForCollider(MovePoint.position + new Vector3(0f, _movement.y, 0f))) {
                StopMovementAnimationAndNoise();
                return;
            }
            MovePoint.position += new Vector3(0f, _movement.y, 0f);
            _animator.speed = 2;
            _currentUnit?.SetAnimatorSpeed(2);
            CheckTile();
        } else {
            StopMovementAnimationAndNoise();
        }
        HandleAnimation();
    }

    private void StopMovementAnimationAndNoise() {
        _animator.speed = 1;
        if (_currentUnit) {
            _currentUnit?.SetAnimatorSpeed(1);
        }
        StopCoroutine("MovementNoise");
        _movementNoise = false;
    }

    private bool DoPredefinedMovement() {
        if (_setPath != null && _setPath.Any()) {
            _movementNoiseInterval = 0.1f;
            var newPosition = _setPath.Dequeue();
            SetAnimationDirection(MovePoint.position, newPosition);
            MovePoint.position = newPosition;
            _unitReached = false;
            return true;
        }
        //Need to wait until uint ACTUALLY reached the destination,
        //therefor we don't check for _setPath.Size == 1...
        if (!_unitReached) {
            _movementNoiseInterval = 0.2f;
            SetControlUnit(_battleController.GetCurrentUnit());
            _quickInfo.ShowQuickInfo(_battleController.GetCurrentUnit().GetCharacter());
            MoveSpeed = _initialSpeed;
            _unitReached = true;
        }

        return false;
    }
    private void HandleAnimation() {
        var direction = DirectionType.none;
        if (_movement.x > 0) {
            direction = DirectionType.right;
        } else if (_movement.x < 0) {
            direction = DirectionType.left;
        } else if (_movement.y > 0) {
            direction = DirectionType.up;
        } else if (_movement.y < 0) {
            direction = DirectionType.down;
        }
        _animator.SetInteger("moveDirection", (int)direction);
        _currentUnit?.SetAnimatorDirection(direction);
    }

    private void SetAnimationDirection(Vector3 currentPos, Vector3 nextPos) {
        var direction = DirectionType.none;
        if (currentPos.x < nextPos.x) {
            direction = DirectionType.right;
        } else if (currentPos.x > nextPos.x) {
            direction = DirectionType.left;
        } else if (currentPos.y < nextPos.y) {
            direction = DirectionType.up;
        } else if (currentPos.y > nextPos.y) {
            direction = DirectionType.down;
        }
        _animator.SetInteger("moveDirection", (int)direction);
        _currentUnit?.SetAnimatorDirection(direction);
    }

    private bool CheckForCollider(Vector3 pointToCheck) {
        if (_isMoveInBattleSquares) {
            return !Physics2D.OverlapCircle(pointToCheck, .2f, BattleCollider);
        }
        else {
            return Physics2D.OverlapCircle(pointToCheck, .2f, Collider);
        }
    }

    public int GetLandEffect() {
        var test = _terrainTileMap.WorldToCell(MovePoint.position);
        var sprite = _terrainTileMap.GetSprite(test);
        return TerrainEffects.GetLandEffect(sprite.name);
    }

    private void CheckTile() {
        _landEffect.ShowLandEffect(GetLandEffect());
    }

    IEnumerator MovementNoise() {
        _movementNoise = true;
        while (true) {
            _audioManager.PlaySFX(_audioClipMovementNoise);
            yield return new WaitForSeconds(_movementNoiseInterval);
        }
    }
}

