﻿#pragma warning disable 436
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FezEngine.Mod;
using MonoMod;

namespace FezEngine.Tools {
    public static class patch_SettingsManager {

        #if FNA
        //FIXME FEZ 1.12
        [MonoModLinkTo("FezEngine.Tools.SettingsManager", "FEZ112viewScaleOrSomethingRIPME")]
        #endif
        private static float viewScale;

        #if FNA
        public static extern void orig_SetupViewport(this GraphicsDevice device);
        public static void SetupViewport(this GraphicsDevice device) {
            //FEZ 1.12 disables letterboxing (at least during the beta)
            orig_SetupViewport(device);
        #else
        public static void SetupViewport(this GraphicsDevice device, bool forceLetterbox = false) {
            int backBufferWidth = device.PresentationParameters.BackBufferWidth;
            int backBufferHeight = device.PresentationParameters.BackBufferHeight;
            /*if (!forceLetterbox) {
                RenderTargetBinding[] renderTargets = device.GetRenderTargets();
                if (renderTargets.Length > 0 && renderTargets[0].RenderTarget is Texture2D) {
                    return;
                }
            }*/
            device.ScissorRectangle = new Rectangle(0, 0, backBufferWidth, backBufferHeight);
            device.Viewport = new Viewport {
                X = 0,
                Y = 0,
                Width = backBufferWidth,
                Height = backBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            };
        #endif
            
            if (FEZModEngine.EnablePPHD) {
                viewScale = 1f;
            } else {
                viewScale = ((float) device.Viewport.Width / 1280f + (float) device.Viewport.Height / 720f) / 2f;
            }
        }
        
    }
}