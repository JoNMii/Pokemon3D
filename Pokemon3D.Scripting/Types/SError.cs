﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokemon3D.Scripting.Types
{
    class SError : SProtoObject
    {
        internal override string TypeOf()
        {
            return LITERAL_TYPE_ERROR;
        }
    }
}