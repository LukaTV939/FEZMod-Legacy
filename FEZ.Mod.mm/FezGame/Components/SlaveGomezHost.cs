﻿using System;
using FezGame.Components.Actions;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Common;


namespace FezGame.Components {
    public class SlaveGomezHost : DrawableGameComponent {

        public static SlaveGomezHost Instance;

        [ServiceDependency]
        public IPlayerManager PlayerManager { private get; set; }
        [ServiceDependency]
        public IGameCameraManager CameraManager { private get; set; }
        [ServiceDependency]
        public IGameStateManager GameState { private get; set; }
        [ServiceDependency]
        public ILevelManager LevelManager { private get; set; }
        [ServiceDependency]
        public ILightingPostProcess LightingPostProcess { private get; set; }
        [ServiceDependency]
        public ISoundManager SoundManager { private get; set; }

        private GomezEffect effect;
        private Mesh playerMesh;

        public Vector3 Position;
        public Quaternion Rotation;
        public float Opacity = 1f;
        public bool Background = false;
        protected AnimatedTexture Animation;
        public ActionType Action = ActionType.Idle;
        protected ActionType PrevAction;
        public Matrix TextureMatrix;
        public float EffectBackground;
        public Vector3 Scale;
        public bool NoMoreFez = false;

        public bool ForceSilhouette = false;
        public bool DrawStars = false;
        public Texture2D StarsTexture;

        protected NetworkGomezData networkData;

        public SlaveGomezHost(Game game) 
            : base(game) {
            playerMesh = new Mesh {
                SamplerState = SamplerState.PointClamp
            };
            UpdateOrder = 11;
            DrawOrder = 8;
            Instance = this;
        }

        public void DoDraw() {
            if (GameState.Loading || PlayerManager.Hidden || GameState.InMap || FezMath.AlmostEqual(PlayerManager.GomezOpacity, 0f)) {
                return;
            }

            playerMesh.Position = Position;
            playerMesh.Rotation = Rotation;
            playerMesh.Material.Opacity = Opacity;

            if (!Action.SkipSilhouette()) {
                GraphicsDevice.PrepareStencilRead(CompareFunction.Greater, StencilMask.NoSilhouette);
                playerMesh.DepthWrites = false;
                playerMesh.AlwaysOnTop = true;
                effect.Silhouette = true;
                playerMesh.Draw();
            }

            if (!Background) {
                GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, StencilMask.Hole);
                playerMesh.AlwaysOnTop = true;
                playerMesh.DepthWrites = false;
                effect.Silhouette = false || ForceSilhouette;
                playerMesh.Draw();
            }

            GraphicsDevice.PrepareStencilWrite(StencilMask.Gomez);
            playerMesh.AlwaysOnTop = PlayerManager.Action.NeedsAlwaysOnTop();
            playerMesh.DepthWrites = true;
            effect.Silhouette = false || ForceSilhouette;
            playerMesh.Draw();

            if (DrawStars) {
                GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, StencilMask.Gomez);

                float viewScale = GraphicsDevice.GetViewScale() * 0.5f;
                float m11 = CameraManager.Radius / ((float)StarsTexture.Width / 16f) / viewScale;
                float m22 = (float)((double)CameraManager.Radius / (double)CameraManager.AspectRatio / ((double)StarsTexture.Height / 16.0)) / viewScale;
                Matrix textureMatrix = new Matrix(m11, 0.0f, 0.0f, 0.0f, 0.0f, m22, 0.0f, 0.0f, (float)(-(double)m11 / 2.0), (float)(-(double)m22 / 2.0), 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
                GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                ServiceHelper.Get<ITargetRenderingManager>().DrawFullscreen(StarsTexture, textureMatrix, Color.White);

                GraphicsDevice.PrepareStencilRead(CompareFunction.Always, StencilMask.None);
            } else {
                GraphicsDevice.PrepareStencilWrite(StencilMask.None);
            }
        }

        public override void Draw(GameTime gameTime) {
            if (GameState.StereoMode && !GameState.FarawaySettings.InTransition) {
                return;
            }
            DoDraw();
        }

        public override void Initialize() {
            playerMesh.AddFace(new Vector3(1f), new Vector3(0f, 0.25f, 0f), FaceOrientation.Front, true, true);
            LevelManager.LevelChanged += delegate {
                effect.ColorSwapMode = ((LevelManager.WaterType != LiquidType.Sewer) ? ((LevelManager.WaterType != LiquidType.Lava) ? ((!LevelManager.BlinkingAlpha) ? ColorSwapMode.None : ColorSwapMode.Cmyk) : ColorSwapMode.VirtualBoy) : ColorSwapMode.Gameboy);
            };
            LightingPostProcess.DrawGeometryLights += PreDraw;
            base.Initialize();
        }

        protected override void LoadContent() {
            playerMesh.Effect = effect = new GomezEffect();
            StarsTexture = ServiceHelper.Get<IContentManagerProvider>().Global.Load<Texture2D>("Other Textures/black_hole/Stars");
        }

        private void PreDraw() {
            if (GameState.Loading || PlayerManager.Hidden || GameState.InFpsMode) {
                return;
            }
            effect.Pass = LightingEffectPass.Pre;
            if (!PlayerManager.FullBright) {
                GraphicsDevice.PrepareStencilWrite(StencilMask.Level);
            } else {
                GraphicsDevice.PrepareStencilWrite(StencilMask.None);
            }
            playerMesh.Draw();
            GraphicsDevice.PrepareStencilWrite(StencilMask.None);
            effect.Pass = LightingEffectPass.Main;
        }

        public override void Update(GameTime gameTime) {
            if (NetworkGomezClient.Instance != null) {
                NetworkGomezClient.Instance.Action = NetworkGomezClient.Instance.Action ?? UpdateNetGomez;
            }

            NetworkGomezData networkData = this.networkData;
            if (networkData != null) {
                Position = networkData.Position;
                Rotation = networkData.Rotation;
                Opacity = networkData.Opacity;
                Background = networkData.Background;
                Action = networkData.Action;
                TextureMatrix = networkData.TextureMatrix;
                //EffectBackground = networkData.EffectBackground;
                Scale = networkData.Scale;
                NoMoreFez = networkData.NoMoreFez;

                ForceSilhouette = false;
                DrawStars = false;

                if (networkData.InCutscene || networkData.InMap || networkData.InMenuCube || networkData.Paused) {
                    DrawStars = true;
                }

                if (networkData.Viewpoint != CameraManager.Viewpoint) {
                    Rotation = CameraManager.Rotation;
                    Opacity = 0.75f;
                }

                if (networkData.Level != LevelManager.Name) {
                    DrawStars = true;
                }
            }

            playerMesh.FirstGroup.TextureMatrix.Set(TextureMatrix);
            effect.Background = EffectBackground;
            playerMesh.Scale = Scale;
            effect.NoMoreFez = NoMoreFez;

            if (Action != PrevAction) {
                Animation = PlayerManager.GetAnimation(Action);
            }
            PrevAction = Action;
            if (Animation == null) {
                return;
            }
            effect.Animation = Animation.Texture;
        }


        public void UpdateNetGomez(object obj) {
            NetworkGomezData networkData = obj as NetworkGomezData;
            if (networkData == null || (this.networkData != null && networkData.DataId <= this.networkData.DataId)) {
                return;
            }
            this.networkData = networkData;
        }

    }
}

