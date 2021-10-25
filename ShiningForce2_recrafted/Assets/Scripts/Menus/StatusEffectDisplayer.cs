using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Assets.Scripts.Menus {
    public class StatusEffectDisplayer : Singleton<StatusEffectDisplayer> {

        public void SetAllStatusEffectsOfCharacter(GameObject statusEffectField, EnumStatusEffect statusEffect) {
            StopAllCoroutines();
            StartCoroutine(CycleThroughStatusEffects(statusEffectField, statusEffect));
        }
        private List<Sprite> SetAllStatusEffectsAsSprites(EnumStatusEffect statusEffectsAsEnum) {
            var spriteAtlas = Resources.Load<SpriteAtlas>(Constants.SpriteAtlasStatusEffects);
            var spriteList = new List<Sprite>();
            if (statusEffectsAsEnum == EnumStatusEffect.none) {
                spriteList.Add(spriteAtlas.GetSprite(Enum.GetName(typeof(EnumStatusEffect), EnumStatusEffect.none)));
                return spriteList;
            } else if (statusEffectsAsEnum.HasFlag(EnumStatusEffect.dead)) {
                spriteList.Add(spriteAtlas.GetSprite(Enum.GetName(typeof(EnumStatusEffect), EnumStatusEffect.dead)));
                return spriteList;
            }

            // Get all names of set Enum from statusEffectsAsEnum
            var stringList = Enum.GetValues(typeof(EnumStatusEffect)).Cast<EnumStatusEffect>()
                .Where(x => statusEffectsAsEnum.HasFlag(x)).Select(x => Enum.GetName(typeof(EnumStatusEffect), x))
                .ToList();

            foreach (var spriteName in stringList) {
                if (spriteName.Equals("none")) {
                    continue;
                }
                spriteList.Add(spriteAtlas.GetSprite(spriteName));
            }

            return spriteList;
        }

        IEnumerator CycleThroughStatusEffects(GameObject statusEffectField, EnumStatusEffect statusEffects) {
            var spriteList = SetAllStatusEffectsAsSprites(statusEffects);
            do {
                foreach (var sprite in spriteList) {
                    statusEffectField.GetComponent<Image>().sprite = sprite;
                    yield return new WaitForSeconds(1f);
                }
            } while (true);
        }
    }
}
