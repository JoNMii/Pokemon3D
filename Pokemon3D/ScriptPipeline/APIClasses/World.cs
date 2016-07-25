﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pokemon3D.Scripting.Types;
using Pokemon3D.Scripting;
using Pokemon3D.Scripting.Adapters;
using Pokemon3D.Screens.Overworld;
using Pokemon3D.Screens;

namespace Pokemon3D.ScriptPipeline.APIClasses
{
    [APIClass(ClassName = "world")]
    class World : APIClass
    {
        public static SObject getEntity(ScriptProcessor processor, SObject[] parameters)
        {
            object[] netObjects;
            if (EnsureTypeContract(parameters, new Type[] { typeof(string) }, out netObjects))
            {
                var entity = new EntityWrapper();
                entity.id = (string)netObjects[0];

                return ScriptInAdapter.Translate(processor, entity);
            }

            return ScriptInAdapter.GetUndefined(processor);
        }

        public static SObject load(ScriptProcessor processor, SObject[] parameters)
        {
            object[] netObjects;
            if (EnsureTypeContract(parameters, new Type[] { typeof(string), typeof(double), typeof(double), typeof(double) }, out netObjects))
            {
                OverworldScreen screen = (OverworldScreen)GameCore.GameProvider.GameInstance.GetService<ScreenManager>().CurrentScreen;

                screen.ActiveWorld.AddMap(netObjects[0] as string, (double)netObjects[1], (double)netObjects[2], (double)netObjects[3]);
            }

            return ScriptInAdapter.GetUndefined(processor);
        }
    }
}
