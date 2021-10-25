using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData.Trigger;
using Assets.Scripts.Menus;
using UnityEngine;

namespace Assets.Scripts.TownAI {
    public class Merchant : MonoBehaviour, IEventTrigger {
        public List<GameItem> ItemsToSell;

        private MerchantMenu _merchantMenu;

        void Start() {
            _merchantMenu = MerchantMenu.Instance;
        }

        public void EventTrigger() {
            _merchantMenu.OpenMerchantWindow(ItemsToSell);
        }
    }
}
