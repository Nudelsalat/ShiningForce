using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour {
    public AudioClip audioClip;

    private void OnTriggerEnter2D(Collider2D collider) {
        if (!collider.tag.Equals("Player")) {
            return;
        }
        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }
        Destroy(gameObject);
    }
}
