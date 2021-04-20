using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.Scripts.EditorScripts {

    public class PaletteSwapNoShader : MonoBehaviour {
        public Texture2D NewTexture;
        public int SkinID;

        private  SpriteRenderer _spriteRenderer;
        private Texture2D _characterTexture2D;

        private Sprite _sprite;

        void Awake() {

            //UpdateCharacterTexture();
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (NewTexture.height - 1 <= SkinID) {
                return;
            }

            _characterTexture2D = CopyTexture2D(_spriteRenderer.sprite.texture, SkinID);
            var _swapShader = Shader.Find("Custom/SwapTwo");

            var _newMat = new Material(_swapShader);
            _spriteRenderer.material = _newMat;
            _spriteRenderer.material.SetTexture("_MainTex2", _characterTexture2D);
        }

        //CopiedTexture is the original Texture  which you want to copy.
        public Texture2D CopyTexture2D(Texture2D copiedTexture, int paletteValue) {
            var originalColors = NewTexture.GetPixels(0, NewTexture.height-1, NewTexture.width,1);
            var newColors = NewTexture.GetPixels(0, paletteValue, NewTexture.width, 1);
            //Create a new Texture2D, which will be the copy.
            Texture2D texture = new Texture2D(copiedTexture.width, copiedTexture.height);
           
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;


            int y = 0;
            while (y < texture.height) {
                int x = 0;
                while (x < texture.width) {
                    var i = CompareColor(copiedTexture.GetPixel(x, y), originalColors);
                    if (i > -1) {
                        texture.SetPixel(x, y, newColors[i]);
                    } else {
                        var pixel = copiedTexture.GetPixel(x, y);
                        texture.SetPixel(x, y, copiedTexture.GetPixel(x, y));
                    }
                    ++x;
                }
                ++y;
            }

            texture.name = "temp";
            texture.Apply();
            
            return texture;
        }

        private int CompareColor(Color color, Color[] listColor) {
            for (int i = 0; i < listColor.Length; i++) {
                if (Math.Abs(color.r - listColor[i].r) < 0.0001 && Math.Abs(color.g - listColor[i].g) < 0.0001 &&
                    Math.Abs(color.b - listColor[i].b) < 0.0001 && Math.Abs(color.a - listColor[i].a) < 0.0001) {
                    return i;
                }
            }
            return -1;
        }

        public void LateUpdate() {
            //_spriteRenderer.sprite = _sprite;
        }

        public void UpdateCharacterTexture() {
            
            var characterTexture2D = CopyTexture2D(_spriteRenderer.sprite.texture, 0);
            
            var tempName = _spriteRenderer.sprite.name;
            _sprite = Sprite.Create(characterTexture2D, 
                new Rect(0, 0, characterTexture2D.width, characterTexture2D.height), new Vector2(.5f, .5f), 24);

            _sprite.name = tempName;
        }
    }
}