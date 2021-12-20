//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Newtonsoft.Json;

    public class AssignmentDTO : VersionedDTO
    {
        [JsonProperty(PropertyName = "@xsi.type")]
        public string XsiType { get; set; }

        public int CompletionOrder { get; set; }

        public int Status { get; set; }

        public LocationDTO Location { get; set; }

        public long LocationId { get; set; }

        public string Type { get; set; }

        public long WorkerId { get; set; }

        public long ProductId { get; set; }

        public ProductDTO Product { get; set; }
    }
}
