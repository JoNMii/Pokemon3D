﻿namespace Pokemon3D.Scripting.Types.Prototypes
{
    internal class BooleanPrototype : Prototype
    {
        public BooleanPrototype() : base("Boolean")
        {
            Constructor = new PrototypeMember("constructor", new SFunction(constructor));
        }

        protected override SProtoObject CreateBaseObject()
        {
            return new SBool();
        }

        private static SObject constructor(ScriptProcessor processor, SObject instance, SObject This, SObject[] parameters)
        {
            var obj = (SBool)instance;

            if (parameters[0] is SBool)
                obj.Value = ((SBool)parameters[0]).Value;
            else
                obj.Value = parameters[0].ToBool(processor).Value;

            return obj;
        }
    }
}
