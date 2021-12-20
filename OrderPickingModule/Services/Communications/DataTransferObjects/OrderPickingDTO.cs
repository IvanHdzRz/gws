//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Newtonsoft.Json;

    public class OrderPickingDTO
    {
        [JsonProperty(PropertyName = "assignments")]
        public OrderPickingAssignmentsDTO OrderPickingAssignments { get; set; }
    }
}
