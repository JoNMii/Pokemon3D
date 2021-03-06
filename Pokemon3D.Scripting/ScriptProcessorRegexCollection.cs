﻿namespace Pokemon3D.Scripting
{
    public partial class ScriptProcessor
    {
        private const string REGEX_NUMRIGHTDOT = "^[0-9]+(E[-+][0-9]+)?$";
        private const string REGEX_NUMLEFTDOT = @"^[-]?\d+$";
        private const string REGEX_LAMBDA = @"^([a-zA-Z][a-zA-Z0-9]*([ ]*[,][ ]*[a-zA-Z][a-zA-Z0-9]*)*|\(\))[ ]*=>.+$";
        private const string REGEX_FUNCTION = @"^function[ ]*\(";
        private const string REGEX_CATCHCONDITION = @"^[a-zA-Z]\w*[ ]+if[ ]+.+$";
    }
}
