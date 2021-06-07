using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {
    public class ShrinkIntoPlayerAndDestroyStorageTrigger : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;
        public float Duration = 0.5f;

        public void EventTrigger() {
            StartCoroutine(ShrinkIntoPlayer());
        }

        IEnumerator ShrinkIntoPlayer() {
            Player.Instance.SetInputDisabledInEvent();
            var startPoint = transform.position;
            var elapsedTime = 0.0f;
            while (elapsedTime < Duration) {
                elapsedTime += Time.deltaTime;
                var percent = elapsedTime / Duration;
                transform.position = Vector3.Lerp(startPoint, Player.Instance.transform.position, percent);
                transform.localScale = new Vector3(1-percent,1-percent,1);
                yield return null;
            }
            transform.localScale = new Vector3(0,0,0);
            transform.position = Player.Instance.transform.position;
            Player.Instance.UnsetInputDisabledInEvent();
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
            SetAsTriggered();
        }
    }
}
