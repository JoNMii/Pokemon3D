﻿namespace Pokemon3D.Scripting.Types.Prototypes
{
    internal class NumberPrototype : Prototype
    {
        public NumberPrototype() : base("Number")
        {
            Constructor = new PrototypeMember("constructor", new SFunction(constructor));
        }

        protected override SProtoObject CreateBaseObject()
        {
            return new SNumber();
        }

        private static SObject constructor(ScriptProcessor processor, SObject instance, SObject This, SObject[] parameters)
        {
            var obj = (SNumber)instance;

            if (parameters[0] is SNumber)
                obj.Value = ((SNumber)parameters[0]).Value;
            else
                obj.Value = parameters[0].ToNumber(processor).Value;

            return obj;
        }
    }
}
