﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fadeout : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player") || collider.CompareTag("Force") || collider.CompareTag("Enemies")) {
            var renderer = this.GetComponent<SpriteRenderer>();
            renderer.color = new Color(1f, 1f, 1f, 0.2f);
        }
    }
    private void OnTriggerExit2D(Collider2D collider) {
        if (collider.CompareTag("Player") || collider.CompareTag("Force") || collider.CompareTag("Enemies")) {
            var renderer = this.GetComponent<SpriteRenderer>();
            renderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }
}