﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon3D.GameCore;

namespace Pokemon3D.UI
{
    /// <summary>
    /// Manages a Pokémon sprite sheet.
    /// </summary>
    internal class PokemonSpriteSheet
    {
        private const int FRAME_LENGTH = 2;

        private Texture2D[] _frames;
        private int _currentFrame = 0;
        private int _frameDelay = FRAME_LENGTH;

        public PokemonSpriteSheet(Texture2D spriteSheet, int frameWidth, int frameHeight)
        {
            ExtractFrames(spriteSheet, frameWidth, frameHeight);
        }

        private void ExtractFrames(Texture2D spriteSheet, int frameWidth, int frameHeight)
        {
            List<Texture2D> frames = new List<Texture2D>();

            int pixelCountPerFrame = frameWidth * frameHeight;
            bool foundEmptyFrame = false;

            for (int y = 0; y < spriteSheet.Height; y += frameHeight)
            {
                if (y + frameHeight <= spriteSheet.Height && !foundEmptyFrame)
                {
                    for (int x = 0; x < spriteSheet.Width; x += frameWidth)
                    {
                        if (x + frameWidth <= spriteSheet.Width && !foundEmptyFrame)
                        {
                            var frameData = new Color[pixelCountPerFrame];
                            spriteSheet.GetData(0, new Rectangle(x, y, frameWidth, frameHeight), frameData, 0, pixelCountPerFrame);

                            if (frameData.All(c => c.A == 0))
                            {
                                foundEmptyFrame = true;
                            }
                            else
                            {
                                Texture2D frame = new Texture2D(GameProvider.GameInstance.GraphicsDevice, frameWidth, frameHeight);
                                frame.SetData(frameData);
                                frames.Add(frame);
                            }
                        }
                    }
                }
            }

            _frames = frames.ToArray();
        }

        public void Update()
        {
            _frameDelay--;
            if (_frameDelay <= 0)
            {
                _frameDelay = FRAME_LENGTH;
                _currentFrame++;
                if (_currentFrame >= _frames.Length)
                    _currentFrame = 0;
            }
        }

        public Texture2D CurrentFrame => _frames[_currentFrame];
    }
}
