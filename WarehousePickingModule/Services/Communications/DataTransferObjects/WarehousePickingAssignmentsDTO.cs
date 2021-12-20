//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWork;

    public class WarehousePickingAssignmentsDTO : VersionedDTO
    {
        [JsonProperty(PropertyName = "assignment")]
        [JsonConverter(typeof(SingleOrArrayConverter<WarehousePickingAssignmentDTO>))]
        public IList<WarehousePickingAssignmentDTO> Assignments { get; set; }
    }
}
