﻿namespace Pokemon3D.Scripting.Adapters
{
    /// <summary>
    /// Different types of API callbacks.
    /// </summary>
    internal enum CallbackType
    {
        /// <summary>
        /// For the delegate type <see cref="DHasMember"/>.
        /// </summary>
        HasMember,
        /// <summary>
        /// For the delegate type <see cref="DSetMember"/>.
        /// </summary>
        SetMember,
        /// <summary>
        /// For the delegate type <see cref="DGetMember"/>.
        /// </summary>
        GetMember,
        /// <summary>
        /// For the delegate type <see cref="DExecuteMethod"/>.
        /// </summary>
        ExecuteMethod,
        /// <summary>
        /// For the delegate type <see cref="DScriptPipeline"/>.
        /// </summary>
        ScriptPipeline
    }
}
