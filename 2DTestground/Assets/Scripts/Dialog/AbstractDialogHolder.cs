using Assets.Scripts.GameData.Trigger;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

namespace Assets.Scripts.Dialog {
    public abstract class AbstractDialogHolder : MonoBehaviour, IEventTrigger {
        public bool DespawnAfterUse = false;

        private LayerMask _playerCollision;
        private Animator _animator;

        private bool _isInSpace;

        public void EventTrigger() {
            TriggerDialogue();
        }

        // Start is called before the first frame update
        public abstract void TriggerDialogue();

        void Awake() {
            _animator = GetComponent<Animator>();
            _playerCollision = LayerMask.GetMask("Player");
        }

        public virtual void Update() {
            if (_isInSpace && Input.GetButtonUp("Interact") && !Player.IsInDialogue && !Player.InWarp
                && !Player.InputDisabledInDialogue && !Player.InputDisabledInEvent && Player.PlayerIsInMenu == EnumMenuType.none) {
                FindPlayerDirection();
                TriggerDialogue();
                if (DespawnAfterUse) {
                    Destroy(gameObject);
                }
            }
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (collider.CompareTag("InteractionPointer")) {
                _isInSpace = true;
            }
        }

        void OnTriggerExit2D(Collider2D collider) {
            if (collider.CompareTag("InteractionPointer")) {
                _isInSpace = false;
            }
        }

        private void FindPlayerDirection() {
            if (_animator == null) {
                return;
            }
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
                    return;
                }
            }
        }
    }
}
