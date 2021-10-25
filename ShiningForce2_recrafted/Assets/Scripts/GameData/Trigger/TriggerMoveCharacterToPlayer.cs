using System.Collections.Generic;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.GameData.Trigger {
    public class TriggerMoveCharacterToPlayer : MonoBehaviour, IEventTrigger {
        public GameObject Unit;
        public MonoBehaviour FollowUpEvent;
        public int MoveSpeed = 2;
        public DirectionType OrientationAfterMove = DirectionType.none;

        private Animator _animator;
        private bool _triggered = false;
        private Queue<Vector3> _path;
        private Vector3 _next;

        void Start() {
            _animator = Unit.GetComponent<Animator>();
            _next = Unit.transform.position;
        }
        void Update() {
            if (_triggered) {
                Unit.transform.position =
                    Vector3.MoveTowards(Unit.transform.position, _next, MoveSpeed * Time.deltaTime);
                if (!(Vector3.Distance(Unit.transform.position, _next) <= 0.0005f)) {
                    return;
                }
                //don't get to the last position
                if (_path.Count == 1) {
                    _triggered = false;
                    if (FollowUpEvent != null) {
                        FollowUpEvent.Invoke("EventTrigger", 0);
                    }
                    this.enabled = false;
                    Player.Instance.UnsetInputDisabledInEvent();
                    _animator.SetInteger("moveDirection", (int)OrientationAfterMove);
                    return;
                }
                
                _next = _path.Dequeue();
                HandleAnimation();
            }
        }

        public void EventTrigger() {
            Player.Instance.SetInputDisabledInEvent();
            this.enabled = true;
            var terrainTileMap = GameObject.Find("Terrain").GetComponent<Tilemap>();
            var movementGrid = new MovementGrid(terrainTileMap);
            var unpassableTiles = TerrainEffects.GetImpassableTerrainNameOfMovementType(EnumMovementType.foot);
            var movebaleSquares = movementGrid.GetWholeTerrainTileMapWithoutCertainSprites(unpassableTiles, new LayerMask());
            var pathfinder = new Pathfinder(movebaleSquares, FixGridPosition(Unit.transform.position), 
                FixGridPosition(Player.Instance.transform.position));
            var result = pathfinder.GetShortestPath();
            if (result != null) {
                _path = new Queue<Vector3>(result);
                _triggered = true;
                _next = Unit.transform.position;
            }
            else {
                this.enabled = false;
                Player.Instance.UnsetInputDisabledInEvent();
            }
        }

        private Vector3 FixGridPosition(Vector3 position) {
            var x = position.x >= 0 ? (int)position.x + 0.5f : (int)position.x - 0.5f;
            var y = position.y >= 0 ? (int)position.y + 0.75f : (int)position.y - 0.25f;
            return new Vector3(x, y);

        }

        private void HandleAnimation() {
            var direction = DirectionType.none;
            var diff = _next - Unit.transform.position;
            if (diff.x > 0) {
                direction = DirectionType.right;
            } else if (diff.x < 0) {
                direction = DirectionType.left;
            } else if (diff.y > 0) {
                direction = DirectionType.up;
            } else if (diff.y < 0) {
                direction = DirectionType.down;
            }
            _animator.SetInteger("moveDirection", (int)direction);
        }
    }
}
