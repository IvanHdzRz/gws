//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Newtonsoft.Json;

    public class WarehousePickingDTO
    {
        [JsonProperty(PropertyName = "assignments")]
        public WarehousePickingAssignmentsDTO WarehousePickingAssignments { get; set; }
    }
}
