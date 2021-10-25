using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle {
    public class AreaOfEffectColliderManager : MonoBehaviour {
        private List<Collider2D> _triggerList = new List<Collider2D>();

        public List<Collider2D> GetAllCurrentCollider(LayerMask layerMask) {
            var result = _triggerList.Where(x => layerMask == (layerMask | 1<<x.gameObject.layer));
            return result.ToList();
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!_triggerList.Contains(other)) {
                _triggerList.Add(other);
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (_triggerList.Contains(other)) {
                _triggerList.Remove(other);
            }
        }
    }
}