//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;
    using Newtonsoft.Json;
    using Retail;

    public class OrderPickingAssignmentsDTO : VersionedDTO
    {
        [JsonProperty(PropertyName = "assignment")]
        [JsonConverter(typeof(SingleOrArrayConverter<OrderPickingAssignmentDTO>))]
        public IList<OrderPickingAssignmentDTO> Assignments { get; set; }
    }
}
