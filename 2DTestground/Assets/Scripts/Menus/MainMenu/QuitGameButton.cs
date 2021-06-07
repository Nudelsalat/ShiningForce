using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Menus.MainMenu {
    public class QuitGameButton : MonoBehaviour {

        public void QuitGame() {
            Application.Quit();
        }
    }
}
