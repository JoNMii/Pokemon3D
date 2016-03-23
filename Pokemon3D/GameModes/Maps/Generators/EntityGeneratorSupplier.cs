﻿using System.Collections.Generic;

namespace Pokemon3D.GameModes.Maps.Generators
{
    class EntityGeneratorSupplier
    {
        private Dictionary<string, EntityGenerator> _generators;

        public EntityGeneratorSupplier()
        {
            _generators = new Dictionary<string, EntityGenerator>()
                {
                    { "simple", new SimpleEntityGenerator() },
                    { "texturedcube", new TexturedCubeEntityGenerator() }
                };
        }

        public EntityGenerator GetGenerator(string identifier)
        { 
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return _generators["simple"];
            }
            else
            {
                identifier = identifier.ToLowerInvariant();
                if (_generators.ContainsKey(identifier))
                {
                    return _generators[identifier];
                }
                else
                {
                    return _generators["simple"];
                }
            }
        }
    }
}
