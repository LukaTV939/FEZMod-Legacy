using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Effects.Structures;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MonoMod;
using FezEngine.Mod;
using FezGame.Mod;

namespace FezGame.Components {
    public class patch_SpeechBubble : SpeechBubble, patch_ISpeechBubbleManager {
        
        [MonoModIgnore]
		private const int TextBorder = 4;

		[MonoModIgnore]
		private string textString;
        [MonoModIgnore]
		private string originalString;

		private RenderTarget2D text;
        [MonoModIgnore]
		private SpriteFont zuishFont;
        [MonoModIgnore]
		private SpriteBatch spriteBatch;
        [MonoModIgnore]
		private GlyphTextRenderer GTR;
        [MonoModIgnore]
		private float distanceFromCenterAtTextChange;
        [MonoModIgnore]
		private Vector3 oldCamPos;
        [MonoModIgnore]
		private RenderTarget2D bTexture;
        [MonoModIgnore]
		private Vector3 lastUsedOrigin;
        [MonoModIgnore]
		private Vector3 origin;
        [MonoModIgnore]
		private bool show;
        [MonoModIgnore]
		private bool changingText;
        [MonoModIgnore]
		private Group textGroup;
        [MonoModIgnore]
		private Group scalableMiddle;
        [MonoModIgnore]
		private readonly Mesh canvasMesh;
        [MonoModIgnore]
		private readonly Mesh textMesh;
        [MonoModIgnore]
		private float sinceShown;
        [MonoModIgnore]
		private Vector2 scalableMiddleSize;
        [MonoModIgnore]
		private readonly Color TextColor = Color.White;
        [MonoModIgnore]
		private Group scalableTop;
        [MonoModIgnore]
		private Group bGroup;
        [MonoModIgnore]
		private Group tailGroup;
        [MonoModIgnore]
		private Group swGroup;
        [MonoModIgnore]
		private Group seGroup;
        [MonoModIgnore]
		private Group nwGroup;
        [MonoModIgnore]
		private Group neGroup;
        [MonoModIgnore]
		private Group scalableBottom;

		public IGameCameraManager CameraManager { [MonoModIgnore] get; [MonoModIgnore] set; }
		public IContentManagerProvider CMProvider { [MonoModIgnore] get; [MonoModIgnore] set; }
		public SpeechFont Font { [MonoModIgnore] get; [MonoModIgnore] set; }
		public IFontManager FontManager { [MonoModIgnore] get; [MonoModIgnore] set; }
		public IGameStateManager GameState { [MonoModIgnore] get; [MonoModIgnore] set; }
		public bool Hidden { [MonoModIgnore] get; }
		public ILevelManager LevelManager { [MonoModIgnore] get; [MonoModIgnore] set; }
		public Vector3 Origin { [MonoModIgnore] get; [MonoModIgnore] set; }
        
        private Color textColor;
        public Color ColorFG {
            get {
                return textColor;
            }
            set {
                textColor = value;
                if (!FezMath.AlmostEqual(lastUsedOrigin, origin, 0.0625f) && sinceShown >= 1 && !changingText) {
                    OnTextChanged(false);
                }
            }
        }
        private Color textSecondaryColor;
        public Color ColorSecondaryFG {
            get {
                return textSecondaryColor;
            }
            set {
                textSecondaryColor = value;
                if (!FezMath.AlmostEqual(lastUsedOrigin, origin, 0.0625f) && sinceShown >= 1 && !changingText) {
                    OnTextChanged(false);
                }
            }
        }
        private Color lastColorBG;
        public Color ColorBG { get; set; }
        
        private string textSpeaker;
        public string Speaker {
            get {
                return textSpeaker;
            }
            set {
                textSpeaker = value;
                if (!FezMath.AlmostEqual(lastUsedOrigin, origin, 0.0625f) && sinceShown >= 1 && !changingText) {
                    OnTextChanged(false);
                }
            }
        }
        
        private Texture2D tailGroupTexture, neGroupTexture, nwGroupTexture, seGroupTexture, swGroupTexture, scalableGroupTexture;
        
        public patch_SpeechBubble(Game game)
            : base(game) {
            //no-op
        }
        
        public extern void orig_Initialize(); 
        public override void Initialize() {
            orig_Initialize();
            
            textColor = Color.White;
            textSecondaryColor = new Color(1f, 1f, 1f, 0.7f);
            ColorBG = Color.Black;
        }
        
        protected extern void orig_LoadContent();
        protected override void LoadContent() {
            orig_LoadContent();
            
            //Currently requires custom assets, thus default color is black.
            
            tailGroupTexture = (Texture2D) tailGroup.Texture;
            neGroupTexture = (Texture2D) neGroup.Texture;
            nwGroupTexture = (Texture2D) nwGroup.Texture;
            seGroupTexture = (Texture2D) seGroup.Texture;
            swGroupTexture = (Texture2D) swGroup.Texture;
            scalableGroupTexture = CMProvider.Global.Load<Texture2D>("Other Textures/FullWhite");
        }
        
        protected extern void orig_UnloadContent();
        protected override void UnloadContent() {
            orig_UnloadContent();
            if (!(scalableMiddle.Texture is RenderTarget2D)) {
                return;
            }
            TextureExtensions.Unhook(tailGroup.Texture); tailGroup.Texture.Dispose();
            TextureExtensions.Unhook(neGroup.Texture); neGroup.Texture.Dispose();
            TextureExtensions.Unhook(nwGroup.Texture); nwGroup.Texture.Dispose();
            TextureExtensions.Unhook(seGroup.Texture); seGroup.Texture.Dispose();
            TextureExtensions.Unhook(swGroup.Texture); swGroup.Texture.Dispose();
            TextureExtensions.Unhook(scalableMiddle.Texture); scalableMiddle.Texture.Dispose();
        }
        
        private extern void orig_OnTextChanged(bool update);
        private void OnTextChanged(bool update) {
            //Holy decompiler code.
            float num = 2;
            string a = textString;
            textString = originalString;
            
            SpriteFont spriteFont = (Font != SpeechFont.Pixel) ? zuishFont : FontManager.Big;
            SpriteFont spriteFontSpeaker = (Font != SpeechFont.Pixel) ? zuishFont : FontManager.Small;
            if (Font == SpeechFont.Zuish) {
                textString = textString.Replace(" ", "  ");
            }
            float fontScale = (!Culture.IsCJK || Font != SpeechFont.Pixel) ? 1 : FontManager.SmallFactor;
            float num3 = 0;
            if (Font != SpeechFont.Zuish) {
                float num4 = (!update) ? 0.85f : 0.9f;
                float num5 = (float) GraphicsDevice.Viewport.Width / (1280 * GraphicsDevice.GetViewScale());
                num3 = (Origin - CameraManager.InterpolatedCenter).Dot(CameraManager.Viewpoint.RightVector());
                float num6 = (GraphicsDevice.DisplayMode.Width >= 1280) ? (Math.Max(-num3 * 16 * CameraManager.PixelsPerTrixel + 1280 * num5 / 2 * num4, 50) / (CameraManager.PixelsPerTrixel / 2)) : (Math.Max(-num3 * 16 * CameraManager.PixelsPerTrixel + 640 * num4, 50) * 0.6666667f);
                if (GameState.InMap) {
                    num6 = 500;
                }
                num6 = Math.Max(num6, 70);
                List<GlyphTextRenderer.FilledInGlyph> list;
                string text = GTR.FillInGlyphs(textString, out list);
                if (Culture.IsCJK) {
                    fontScale /= 2f;
                }
                StringBuilder stringBuilder = new StringBuilder(WordWrap.Split(text, spriteFont, num6 / fontScale));
                if (Culture.IsCJK) {
                    fontScale *= 2f;
                }
                bool flag2 = true;
                int num7 = 0;
                for (int i = 0; i < stringBuilder.Length; i++) {
                    if (flag2 && stringBuilder[i] == '^') {
                        for (int j = i; j < i + list[num7].Length; j++) {
                            if (stringBuilder[j] == '\r' || stringBuilder[j] == '\n') {
                                stringBuilder.Remove(j, 1);
                                j--;
                            }
                        }
                        stringBuilder.Remove(i, list[num7].Length);
                        stringBuilder.Insert(i, list[num7].OriginalGlyph);
                        num7++;
                    } else {
                        flag2 = (stringBuilder[i] == ' ' || stringBuilder[i] == '\r' || stringBuilder[i] == '\n');
                    }
                }
                textString = stringBuilder.ToString();
                if (!update) {
                    distanceFromCenterAtTextChange = num3;
                }
            }
            if (update && (a == textString || Math.Abs(distanceFromCenterAtTextChange - num3) < 1.5f)) {
                textString = a;
                return;
            }
            if (Culture.IsCJK && Font == SpeechFont.Pixel) {
                float viewScale = GraphicsDevice.GetViewScale();
                if (viewScale < 1.5f) {
                    spriteFont = FontManager.Small;
                } else {
                    spriteFont = FontManager.Big;
                    fontScale /= 2f;
                }
                fontScale *= 2f;
            }
            bool flag3;
            Vector2 value = GTR.MeasureWithGlyphs(spriteFont, textString, fontScale, out flag3);
            if (!Culture.IsCJK && flag3) {
                spriteFont.LineSpacing += 8;
                bool flag4 = flag3;
                value = GTR.MeasureWithGlyphs(spriteFont, textString, fontScale, out flag3);
                flag3 = flag4;
            }
            float scaleFactor = 1;
            if (Culture.IsCJK && Font == SpeechFont.Pixel) {
                scaleFactor = 2f;
            }
            scalableMiddleSize = value + Vector2.One * 4 * 2 * scaleFactor + Vector2.UnitX * 4 * 2 * scaleFactor;
            if (Font == SpeechFont.Zuish) {
                scalableMiddleSize += Vector2.UnitY * 2;
            }
            int width = (int) scalableMiddleSize.X;
            int height = (int) scalableMiddleSize.Y;
            if (Culture.IsCJK && Font == SpeechFont.Pixel) {
                fontScale *= 2f;
                width *= 2;
                height *= 2;
            }
            Vector2 vector = scalableMiddleSize;
            if (text != null) {
                text.Unhook();
                text.Dispose();
            }
            text = new RenderTarget2D(GraphicsDevice, width, height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
            GraphicsDevice.SetRenderTarget(text);
            GraphicsDevice.PrepareDraw();
            GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1, 0);
            Vector2 value2 = (!Culture.IsCJK) ? Vector2.Zero : new Vector2(8f);
            if (Culture.IsCJK) {
                spriteBatch.BeginLinear();
            } else {
                spriteBatch.BeginPoint();
            }
            if (Font == SpeechFont.Pixel) {
                GTR.DrawString(spriteBatch, spriteFont, textString, (vector / 2 - value / 2 + value2).Round(), textColor, fontScale);
            } else {
                spriteBatch.DrawString(spriteFont, textString, vector / 2 - value / 2, textColor, 0, Vector2.Zero, scalableMiddleSize / vector, SpriteEffects.None, 0);
            }
            if (textSpeaker != null) {
                GTR.DrawString(spriteBatch, FontManager.Small, textSpeaker, new Vector2(0.1f, 0f), textSecondaryColor, fontScale / 2f);
            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            if (Font == SpeechFont.Zuish) {
                float x = scalableMiddleSize.X;
                scalableMiddleSize.X = scalableMiddleSize.Y;
                scalableMiddleSize.Y = x;
            }
            if (Culture.IsCJK && Font == SpeechFont.Pixel) {
                scalableMiddleSize /= num;
            }
            scalableMiddleSize /= 16f;
            scalableMiddleSize -= Vector2.One;
            textMesh.SamplerState = ((!Culture.IsCJK || Font != SpeechFont.Pixel) ? SamplerState.PointClamp : SamplerState.AnisotropicClamp);
            textGroup.Texture = text;
            oldCamPos = CameraManager.InterpolatedCenter;
            lastUsedOrigin = Origin;
            if (!Culture.IsCJK && flag3) {
                spriteFont.LineSpacing -= 8;
            }
            
            recolorBG();
        }
        
        private void recolorBG(Group group, Texture2D tex) {
            RenderTarget2D rt;
            if (group.Texture is RenderTarget2D) {
                rt = (RenderTarget2D) group.Texture;
            } else {
                group.Texture = rt = new RenderTarget2D(GraphicsDevice, tex.Width, tex.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
            }
            
            RenderTargetBinding[] prevRT = GraphicsDevice.GetRenderTargets();
            
            GraphicsDevice.SetRenderTarget(rt);
            GraphicsDevice.PrepareDraw();
            GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1, 0);
            
            spriteBatch.BeginPoint();
            spriteBatch.Draw(tex, new Rectangle(0, 0, tex.Width, tex.Height), ColorBG);
            
            spriteBatch.End();
            if (prevRT.Length > 0) {
                if (prevRT[0].RenderTarget is RenderTarget2D) {
                    GraphicsDevice.SetRenderTarget((RenderTarget2D) prevRT[0].RenderTarget);
                } else {
                    GraphicsDevice.SetRenderTarget((RenderTargetCube) prevRT[0].RenderTarget, prevRT[0].CubeMapFace);
                }
            } else {
                GraphicsDevice.SetRenderTarget(null);
            }
        }
        
        private void recolorBG() {
            //FIXME flicker
            if (lastColorBG != ColorBG) {
                lastColorBG = ColorBG;
                
                //Instead of simply drawing the texture tinted.. nope, recolor the texture!
                recolorBG(tailGroup, tailGroupTexture);
                recolorBG(neGroup, neGroupTexture);
                recolorBG(nwGroup, nwGroupTexture);
                recolorBG(seGroup, seGroupTexture);
                recolorBG(swGroup, swGroupTexture);
                recolorBG(scalableMiddle, scalableGroupTexture);
                scalableBottom.Texture = scalableTop.Texture = scalableMiddle.Texture;
            }
        }
        
        public extern void orig_Draw(GameTime gameTime);
        public override void Draw(GameTime gameTime) {
            recolorBG();
            
            orig_Draw(gameTime);
            
            if (FezMath.AlmostEqual(sinceShown, 0f) && !show && !changingText) {
                Speaker = null;
            }
        }
        
    }
}