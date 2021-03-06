﻿using FezGame.Services;
using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FezGame.Mod;
using MonoMod;
using FezEngine.Mod;
using FezGame.Structure;

namespace FezGame.Components {
    public class PlayerCameraControl {

        public IGameStateManager GameState { [MonoModIgnore] get { return null; } }
        public IInputManager InputManager { [MonoModIgnore] get { return null; } }
        public IMouseStateManager MouseState { [MonoModIgnore] get { return null; } }
        public IGameCameraManager CameraManager { [MonoModIgnore] get { return null; } }
        public GraphicsDevice GraphicsDevice { [MonoModIgnore] get { return null; } }
        public IPlayerManager PlayerManager { [MonoModIgnore] get {return null; } }
		public ILevelManager LevelManager { [MonoModIgnore] get {return null; } }
        
        [MonoModIgnore] private extern void FollowGomez();
        
        [MonoModIgnore] private extern void TrackBeforeRotation();

        public extern void orig_Update(GameTime gameTime);
        public void Update(GameTime gameTime) {
            if (!FEZMod.EnableFEZometric) {
                orig_Update(gameTime);
                return;
            }

            if (!FEZMod.DisableInventory && InputManager.OpenInventory == FezButtonState.Pressed) {
                FEZMod.FEZometric = false;
            }

            //Modified decompiled code. Hhnnng.
            //TODO reimplement / rename
            if (FEZMod.FEZometric && MouseState.LeftButton.State != MouseButtonStates.Dragging && InputManager.FreeLook != Vector2.Zero && !GameState.InMap) {
                Vector3 vector3 = Vector3.Transform(CameraManager.Direction, Matrix.CreateFromAxisAngle(Vector3.Up, 1.570796f));
                Vector3 to1 = Vector3.Transform(CameraManager.Direction, Matrix.CreateFromAxisAngle(vector3, -1.570796f));
                Vector2 vector2 = InputManager.FreeLook / (CameraManager.Viewpoint != Viewpoint.Perspective ? 5f : 3f);
                float step = 0.05f;
                Vector3 to2 = FezMath.Slerp(FezMath.Slerp(CameraManager.Direction, vector3, vector2.X), to1, vector2.Y);
                if (!CameraManager.ActionRunning) {
                    CameraManager.AlterTransition(FezMath.Slerp(CameraManager.Direction, to2, step));
                } else {
                    CameraManager.Direction = FezMath.Slerp(CameraManager.Direction, to2, step);
                }
                return;
            }
            
            if (FEZMod.FEZometric && CameraManager.Viewpoint == Viewpoint.Perspective) {
                return;
            }

            orig_Update(gameTime);
        }

    }
}

