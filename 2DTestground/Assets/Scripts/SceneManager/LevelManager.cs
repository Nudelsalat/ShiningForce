using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.SceneManager {
    public static class LevelManager {
        private static string lastLevelString = "";
        private static int lastLevelInt = -1;

        public static void setLastLevelString(string level) {
            lastLevelString = level;
        }

        public static void setLastLevelInt(int level) {
            lastLevelInt = level;
        }


        public static string getLastNameLevelString() {
            return lastLevelString;
        }
        public static int getLastNameLevelInt() {
            return lastLevelInt;
        }
    }
}
