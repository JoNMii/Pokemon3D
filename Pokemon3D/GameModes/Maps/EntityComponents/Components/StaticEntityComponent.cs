﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon3D.GameModes.Maps.EntityComponents.Components
{
    class StaticEntityComponent : EntityComponent
    {
        public StaticEntityComponent(EntityComponentDataCreationStruct parameters) : base(parameters)
        { }

        public override void OnComponentAdded()
        {
           //todo
        }

        public override void OnComponentRemove()
        {
            //todo
        }
    }
}
