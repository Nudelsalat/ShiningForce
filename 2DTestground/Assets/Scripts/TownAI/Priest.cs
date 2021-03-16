using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Menus;
using UnityEngine;

namespace Assets.Scripts.TownAI {
    public class Priest : MonoBehaviour {
        public Sprite PriestSprite;
        private PriestMenu _priestMenu;

        void Start() {
            _priestMenu = PriestMenu.Instance;
        }

        void EventTrigger() {
            _priestMenu.OpenPriestWindow(PriestSprite);
        }
    }
}
