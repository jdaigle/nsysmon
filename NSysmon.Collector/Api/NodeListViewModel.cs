﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Collector.Api
{
    public class NodeListViewModel : RazorViewModelBase
    {
        public List<NodeStatusViewModel> Nodes { get; set; }
    }
}
