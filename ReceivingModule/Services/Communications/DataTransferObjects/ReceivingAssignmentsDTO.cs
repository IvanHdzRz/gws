//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public class ReceivingAssignmentsDTO : VersionedDTO
    {
        [JsonProperty(PropertyName = "assignment")]
        [JsonConverter(typeof(SingleOrArrayConverter<ReceivingAssignmentDTO>))]
        public IList<ReceivingAssignmentDTO> Assignments { get; set; }
    }
}
