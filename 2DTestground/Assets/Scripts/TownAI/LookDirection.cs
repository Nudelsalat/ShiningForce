using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Assets.Scripts.TownAI {
    class LookDirection : MonoBehaviour {
        public DirectionType Direction = DirectionType.down;

        private Animator _animator;

        void Awake() {
            _animator = transform.GetComponent<Animator>();
            _animator.SetInteger("moveDirection", (int)Direction);
        }
        void FixedUpdate() {
            if (_animator == null || (Player.IsInDialogue || Player.InWarp || Player.InputDisabledInDialogue 
                                      || Player.InputDisabledInEvent || Player.PlayerIsInMenu != EnumMenuType.none)) {
                return;
            }
            _animator.SetInteger("moveDirection", (int)Direction);
        }
    }
}
