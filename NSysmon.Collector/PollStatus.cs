﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSysmon.Collector
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public enum PollStatus
    {
        Unknown = 0,
        Success = 1,
        Fail = 2,
    }
}
