using System;
using UnityEngine;

namespace Assets.Scripts.EditorScripts {

    public class PaletteSwapNoShader  {

        //CopiedTexture is the original Texture  which you want to copy.
        public static Texture2D CopyTexture2D(Texture2D copiedTexture, Texture2D colorPalette, int paletteValue, FilterMode filterMode) {
            var originalColors = colorPalette.GetPixels(0, colorPalette.height-1, colorPalette.width,1);
            var newColors = colorPalette.GetPixels(0, paletteValue, colorPalette.width, 1);
            //Create a new Texture2D, which will be the copy.
            Texture2D texture = new Texture2D(copiedTexture.width, copiedTexture.height) {
                filterMode = filterMode, wrapMode = TextureWrapMode.Clamp
            };


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

        private static int CompareColor(Color color, Color[] listColor) {
            for (int i = 0; i < listColor.Length; i++) {
                if (Math.Abs(color.r - listColor[i].r) < 0.0001 && Math.Abs(color.g - listColor[i].g) < 0.0001 &&
                    Math.Abs(color.b - listColor[i].b) < 0.0001 && Math.Abs(color.a - listColor[i].a) < 0.0001) {
                    return i;
                }
            }
            return -1;
        }

        /*
        public void LateUpdate() {
            _spriteRenderer.sprite = _sprite;
        }

        public void UpdateCharacterTexture() {
            
            var characterTexture2D = CopyTexture2D(_spriteRenderer.sprite.texture, 0);
            
            var tempName = _spriteRenderer.sprite.name;
            _sprite = Sprite.Create(characterTexture2D, 
                new Rect(0, 0, characterTexture2D.width, characterTexture2D.height), new Vector2(.5f, .5f), 24);

            _sprite.name = tempName;
        }
        */
    }
}