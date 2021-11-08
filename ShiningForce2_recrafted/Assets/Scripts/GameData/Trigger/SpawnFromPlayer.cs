using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {
    public class SpawnFromPlayer : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;
        public Transform PointToMove;
        public float Duration = 0.5f;

        public void EventTrigger() {
            StartCoroutine(DoSpawnFromPlayer());
        }

        IEnumerator DoSpawnFromPlayer() {
            Player.Instance.SetInputDisabledInEvent();
            transform.localScale = new Vector3(0, 0, 1);
            var startPoint = transform.position = Player.Instance.transform.position;
            var elapsedTime = 0.0f;
            while (elapsedTime < Duration) {
                elapsedTime += Time.deltaTime;
                var percent = elapsedTime / Duration;
                transform.position = Vector3.Lerp(startPoint, PointToMove.position, percent);
                transform.localScale = new Vector3(percent,percent,1);
                yield return null;
            }
            transform.localScale = new Vector3(1,1,1);
            transform.position = PointToMove.position;
            Player.Instance.UnsetInputDisabledInEvent();
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
            SetAsTriggered();
        }
    }
}
