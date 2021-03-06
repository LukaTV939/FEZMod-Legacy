﻿using FezEngine.Structure;
using Microsoft.Xna.Framework;
using FezGame.Mod;

namespace FezGame.Components {
    public class DotHost {

        public SoundEmitter eIdle;

        public extern void orig_Draw(GameTime gameTime);
        public void Draw(GameTime gameTime) {
            //Fixes NPE as eIdle may be null.
            if (Fez.LongScreenshot) {
                if (this.eIdle != null) {
                    eIdle.VolumeFactor = 0f;
                }
                return;
            }
            if (FEZMod.CreatingThumbnail) {
                return;
            }
            orig_Draw(gameTime);
        }

    }
}

