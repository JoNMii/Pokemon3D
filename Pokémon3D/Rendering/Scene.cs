﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pokémon3D.Rendering
{
    class Scene
    {
        private readonly List<SceneNode> _allNodes;
        private readonly List<Camera> _allCameras; 
        private readonly GraphicsDevice _device;
        private readonly BasicEffect _defaultEffect;

        public Scene(Game game)
        {
            _device = game.GraphicsDevice;
            _defaultEffect = new BasicEffect(game.GraphicsDevice);
            _defaultEffect.TextureEnabled = true;
            _allNodes = new List<SceneNode>();
            _allCameras = new List<Camera>();
        }

        public SceneNode CreateSceneNode()
        {
            var sceneNode = new SceneNode();
            _allNodes.Add(sceneNode);
            return sceneNode;
        }

        public Camera CreateCamera()
        {
            var camera = new Camera(_device.Viewport);
            _allCameras.Add(camera);
            _allNodes.Add(camera);
            return camera;
        }

        public void Update(float elapsedTime)
        {
            foreach (var sceneNode in _allNodes)
            {
                sceneNode.Update();
            }
        }

        public void Draw()
        {
            foreach (var camera in _allCameras)
            {
                DrawSceneForCamera(camera);
            }
        }

        private void DrawSceneForCamera(Camera camera)
        {
            _defaultEffect.View = camera.ViewMatrix;
            _defaultEffect.Projection = camera.ProjectionMatrix;
            
            foreach (var sceneNode in _allNodes)
            {
                if (sceneNode.Mesh == null) continue;
                if (sceneNode.Material == null) throw new InvalidOperationException("Render Scene Node needs a material.");

                _defaultEffect.World = sceneNode.World;
                _defaultEffect.Texture = sceneNode.Material.DiffuseTexture;

                foreach (var pass in _defaultEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    sceneNode.Mesh.Draw();
                }
            }
        }
    }
}
