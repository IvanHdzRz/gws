//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;
    using Newtonsoft.Json;
    using Retail;

    public class OrderPickingAssignmentDTO : VersionedDTO
    {
        [JsonProperty(PropertyName = "@xsi.type")]
        public string XsiType { get; set; }

        public int Status { get; set; }

        public int CompletionOrder { get; set; }

        public long WorkerId { get; set; }

        public OrderInfoDTO OrderInfo { get; set; }

        public ProductDTO Product { get; set; }

        public LocationDTO Location { get; set; }

        public int Quantity { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<SubstitutionDTO>))]
        public IList<SubstitutionDTO> Substitutions { get; set; }

        public string Notes { get; set; }
    }
}
