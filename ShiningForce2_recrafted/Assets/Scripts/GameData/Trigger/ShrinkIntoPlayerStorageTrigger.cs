using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameData.Trigger {
    public class ShrinkIntoPlayerStorageTrigger : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;
        public GameObject Unit;
        public float Duration = 0.5f;

        private Transform _currentTransform;
        public void EventTrigger() {
            if (Unit != null) {
                _currentTransform = Unit.transform;
            }
            else {
                _currentTransform = transform;
            }
            StartCoroutine(ShrinkIntoPlayer());
        }

        IEnumerator ShrinkIntoPlayer() {
            Player.Instance.SetInputDisabledInEvent();
            var startPoint = _currentTransform.position;
            var elapsedTime = 0.0f;
            while (elapsedTime < Duration) {
                elapsedTime += Time.deltaTime;
                var percent = elapsedTime / Duration;
                _currentTransform.position = Vector3.Lerp(startPoint, Player.Instance.transform.position, percent);
                _currentTransform.localScale = new Vector3(1-percent,1-percent,1);
                yield return null;
            }
            _currentTransform.localScale = new Vector3(0,0,0);
            _currentTransform.position = Player.Instance.transform.position;
            Player.Instance.UnsetInputDisabledInEvent();
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
            SetAsTriggered();
        }
    }
}
