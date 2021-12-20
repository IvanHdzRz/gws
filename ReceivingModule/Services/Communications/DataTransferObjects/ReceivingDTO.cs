//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Newtonsoft.Json;

    public class ReceivingDTO
    {
        [JsonProperty(PropertyName = "assignments")]
        public ReceivingAssignmentsDTO ReceivingAssignments { get; set; }
    }
}
