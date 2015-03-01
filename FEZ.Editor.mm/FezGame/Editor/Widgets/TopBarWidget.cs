﻿using Common;
using System;
using System.Collections.Generic;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using FezGame.Components;

namespace FezGame.Editor.Widgets {
    public class TopBarWidget : EditorWidget {

        public TopBarWidget(Game game) 
            : base(game) {
        }

        public override void Initialize() {
            //TODO Add buttons
        }

        public override void Update(GameTime gameTime) {
            Size.X = GraphicsDevice.Viewport.Width;
            Size.Y = 24;

            //TODO Rearrange buttons
        }

    }
}

