﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RequirementsScheduler.BLL.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResultType
    {
        STOP1_1,
        STOP1_2,
        STOP1_3,
        STOP1_4
    }

    public class ResultInfo
    {
        public ResultType? Type { get; set; }

        public int OfflineResolvedConflictAmount { get; set; }
        public int OnlineResolvedConflictAmount { get; set; }
        public int OnlineUnResolvedConflictAmount { get; set; }
        
        public bool IsResolvedOnCheck3InOnline { get; set; }

        public bool IsStop3OnOnline { get; set; }

        public float DeltaCmax { get; set; }

        public TimeSpan OfflineExecutionTime { get; set; }
        public TimeSpan OnlineExecutionTime { get; set; }
    }
}
