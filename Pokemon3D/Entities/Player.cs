﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pokemon3D.Common.Animations;
using Pokemon3D.Common.Extensions;
using Pokemon3D.Common.Input;
using Pokemon3D.Entities.System;
using Pokemon3D.Entities.System.Components;
using Pokemon3D.Rendering;
using Pokemon3D.Rendering.Data;
using System;
using Pokemon3D.Content;
using static GameProvider;

namespace Pokemon3D.Entities
{
    internal class Player
    {
        private readonly InputSystem _inputSystem;
        private readonly Entity _playerEntity;
        private readonly Entity _cameraEntity;

        private readonly ModelEntityComponent _modelEntityComponent;

        private PlayerMovementMode _movementMode;
        private MouseState _mouseState;

        private Vector3 _cameraTargetPosition = new Vector3(0, 1, 3);

        public float Speed { get; set; }
        public float RotationSpeed { get; set; }

        public Vector3 Position
        {
            get { return _playerEntity.Position; }
            set { _playerEntity.Position = value; }
        }

        public Camera Camera { get; private set; }

        public PlayerMovementMode MovementMode
        {
            get { return _movementMode; }
            set
            {
                if (_movementMode != value)
                {
                    _movementMode = value;
                    OnMovementModeChanged();
                }
            }
        }

        public Player(World world)
        {
            _inputSystem = GameInstance.GetService<InputSystem>();
            _playerEntity = world.EntitySystem.CreateEntity(true);

            var mesh = new Mesh(GameInstance.GraphicsDevice, Primitives.GenerateQuadForYBillboard());
            var diffuseTexture = GameInstance.Content.Load<Texture2D>(ResourceNames.Textures.DefaultGuy);
            var material = new Material
            {
                DiffuseTexture = diffuseTexture,
                UseTransparency = true,
                TexcoordScale = diffuseTexture.GetTexcoordsFromPixelCoords(32, 32),
                IsUnlit = true
            };
            _modelEntityComponent = _playerEntity.AddComponent(new ModelEntityComponent(_playerEntity, mesh, material, true));

            _cameraEntity = world.EntitySystem.CreateEntity();
            _cameraEntity.SetParent(_playerEntity);

            var cameraComponent = _cameraEntity.AddComponent(new CameraEntityComponent(_cameraEntity, new Skybox(GameInstance)
            {
                Scale = 50,
                Texture = GameInstance.Content.Load<Texture2D>(ResourceNames.Textures.skybox_texture)
            }));
            cameraComponent.FarClipDistance = 50.0f;
            Camera = cameraComponent.Camera;

            Speed = 2.0f;
            RotationSpeed = 2f;

            var forward = Animation.CreateDiscrete(0.65f, new[]
            {
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 0),
                diffuseTexture.GetTexcoordsFromPixelCoords(32, 0),
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 0),
                diffuseTexture.GetTexcoordsFromPixelCoords(64, 0),
            }, t => _modelEntityComponent.Material.TexcoordOffset = t, true);
            var left = Animation.CreateDiscrete(0.65f, new[]
            {
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 32),
                diffuseTexture.GetTexcoordsFromPixelCoords(32, 32),
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 32),
                diffuseTexture.GetTexcoordsFromPixelCoords(64, 32),
            }, t => _modelEntityComponent.Material.TexcoordOffset = t, true);
            var right = Animation.CreateDiscrete(0.65f, new[]
            {
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 96),
                diffuseTexture.GetTexcoordsFromPixelCoords(32, 96),
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 96),
                diffuseTexture.GetTexcoordsFromPixelCoords(64, 96),
            }, t => _modelEntityComponent.Material.TexcoordOffset = t, true);
            var backward = Animation.CreateDiscrete(0.65f, new[]
            {
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 64),
                diffuseTexture.GetTexcoordsFromPixelCoords(32, 64),
                diffuseTexture.GetTexcoordsFromPixelCoords(0, 64),
                diffuseTexture.GetTexcoordsFromPixelCoords(64, 64),
            }, t => _modelEntityComponent.Material.TexcoordOffset = t, true);

            _playerEntity.AddComponent(new FigureMovementAnimationComponent(_playerEntity, forward, backward, left, right));

            _movementMode = PlayerMovementMode.ThirdPerson;
            _cameraEntity.Position = _cameraTargetPosition;

            var colliderComponent = new CollisionEntityComponent(_playerEntity, new Vector3(0.35f, 0.6f, 0.35f),
                new Vector3(0.0f, 0.3f, 0.0f), "Player")
            {
                ResolvesPosition = true,
            };
            _playerEntity.AddComponent(colliderComponent);

            _playerEntity.Position = new Vector3(5, 0, 8);
        }

        public void Update(GameTime gameTime)
        {
            var currentMouseState = Mouse.GetState();

            var movementDirection = Vector3.Zero;
            if (_inputSystem.Left(InputDetectionType.HeldDown, DirectionalInputTypes.WASD | DirectionalInputTypes.LeftThumbstick))
            {
                movementDirection.X = -1.0f;
            }
            else if (_inputSystem.Right(InputDetectionType.HeldDown, DirectionalInputTypes.WASD | DirectionalInputTypes.LeftThumbstick))
            {
                movementDirection.X = 1.0f;
            }

            if (_inputSystem.Up(InputDetectionType.HeldDown, DirectionalInputTypes.WASD | DirectionalInputTypes.LeftThumbstick))
            {
                movementDirection.Z = 1.0f;
            }
            else if (_inputSystem.Down(InputDetectionType.HeldDown, DirectionalInputTypes.WASD | DirectionalInputTypes.LeftThumbstick))
            {
                movementDirection.Z = -1.0f;
            }

            switch (MovementMode)
            {
                case PlayerMovementMode.FirstPerson:
                    HandleFirstPersonMovement(gameTime.GetSeconds(), currentMouseState, movementDirection);
                    break;
                case PlayerMovementMode.ThirdPerson:
                    HandleThirdPersonMovement(gameTime.GetSeconds(), currentMouseState, movementDirection);
                    break;
                case PlayerMovementMode.GodMode:
                    HandleGodModeMovement(gameTime.GetSeconds(), currentMouseState, movementDirection);
                    break;
            }

            _mouseState = currentMouseState;

            if (MovementMode != PlayerMovementMode.GodMode)
            {
                if (Math.Abs(_cameraTargetPosition.Z - _cameraEntity.Position.Z) > float.Epsilon)
                {
                    _cameraEntity.Position = new Vector3(_cameraEntity.Position.X, _cameraEntity.Position.Y, MathHelper.SmoothStep(_cameraEntity.Position.Z, _cameraTargetPosition.Z, 0.2f));
                }
                if (Math.Abs(_cameraTargetPosition.Y - _cameraEntity.Position.Y) > float.Epsilon)
                {
                    _cameraEntity.Position = new Vector3(_cameraEntity.Position.X, MathHelper.SmoothStep(_cameraEntity.Position.Y, _cameraTargetPosition.Y, 0.2f), _cameraEntity.Position.Z);
                }
            }
        }

        private void HandleGodModeMovement(float elapsedTime, MouseState mouseState, Vector3 movementDirection)
        {
            var speedFactor = _inputSystem.Keyboard.IsKeyDown(Keys.LeftShift) ? 2.0f : 1.0f;
            var step = Speed * elapsedTime * speedFactor;

            if (_mouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Pressed)
            {
                var differenceX = mouseState.X - _mouseState.X;
                var differenceY = mouseState.Y - _mouseState.Y;

                _cameraEntity.RotateX(-differenceY * 0.1f * elapsedTime);
                _cameraEntity.RotateY(-differenceX * 0.1f * elapsedTime);
            }

            if (movementDirection.LengthSquared() > 0.0f)
            {
                _cameraEntity.Translate(Vector3.Normalize(movementDirection) * step);
            }
            if (_inputSystem.Keyboard.IsKeyDown(Keys.Space))
            {
                _cameraEntity.Position += Vector3.UnitY * step;
            }
        }

        private void HandleThirdPersonMovement(float elapsedTime, MouseState mouseState, Vector3 movementDirection)
        {
            if (movementDirection.LengthSquared() > 0.0f)
            {
                _playerEntity.Translate(Vector3.Normalize(movementDirection) * Speed * elapsedTime);
            }

            if (_inputSystem.Left(InputDetectionType.HeldDown, DirectionalInputTypes.ArrowKeys | DirectionalInputTypes.RightThumbstick))
            {
                _playerEntity.RotateY(RotationSpeed * elapsedTime);
            }
            else if (_inputSystem.Right(InputDetectionType.HeldDown, DirectionalInputTypes.ArrowKeys | DirectionalInputTypes.RightThumbstick))
            {
                _playerEntity.RotateY(-RotationSpeed * elapsedTime);
            }
        }

        private void HandleFirstPersonMovement(float elapsedTime, MouseState mouseState, Vector3 movementDirection)
        {
            if (movementDirection.LengthSquared() > 0.0f)
            {
                _playerEntity.Translate(Vector3.Normalize(movementDirection) * Speed * elapsedTime);
            }

            if (_inputSystem.Keyboard.IsKeyDown(Keys.Left))
            {
                _playerEntity.RotateY(RotationSpeed * elapsedTime);
            }
            else if (_inputSystem.Keyboard.IsKeyDown(Keys.Right))
            {
                _playerEntity.RotateY(-RotationSpeed * elapsedTime);
            }
            if (_inputSystem.Keyboard.IsKeyDown(Keys.Up))
            {
                _cameraEntity.RotateX(RotationSpeed * elapsedTime);
            }
            else if (_inputSystem.Keyboard.IsKeyDown(Keys.Down))
            {
                _cameraEntity.RotateX(-RotationSpeed * elapsedTime);
            }
        }

        private void OnMovementModeChanged()
        {
            switch (MovementMode)
            {
                case PlayerMovementMode.FirstPerson:
                    _playerEntity.IsActive = true;

                    _cameraEntity.SetParent(_playerEntity);
                    _cameraEntity.EulerAngles = Vector3.Zero;
                    _cameraTargetPosition = new Vector3(0, 0.6f, 0);
                    break;
                case PlayerMovementMode.ThirdPerson:
                    _playerEntity.IsActive = true;
                    _cameraEntity.SetParent(_playerEntity);
                    _cameraEntity.EulerAngles = Vector3.Zero;
                    _cameraTargetPosition = new Vector3(0, 1, 3);

                    break;
                case PlayerMovementMode.GodMode:
                    _cameraEntity.SetParent(null);
                    _playerEntity.IsActive = false;
                    _cameraEntity.Position = _playerEntity.GlobalPosition + new Vector3(0, 1, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
