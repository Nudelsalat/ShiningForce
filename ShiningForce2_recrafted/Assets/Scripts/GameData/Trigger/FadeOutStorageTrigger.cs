using System.Collections;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public class FadeOutStorageTrigger : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;
        public SpriteRenderer SpriteRenderer;

        public void EventTrigger() {
            Player.Instance.SetInputDisabledInEvent();
            StartCoroutine(FadeOut());
        }

        IEnumerator FadeOut() {
            var spriteColor = SpriteRenderer.color;
            while (spriteColor.a > 0) {
                spriteColor.a -= Time.deltaTime * 0.25f;
                SpriteRenderer.color = spriteColor;
                yield return null;
            }
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }

            Player.Instance.UnsetInputDisabledInEvent();
            SetAsTriggered();
        }
    }
}
