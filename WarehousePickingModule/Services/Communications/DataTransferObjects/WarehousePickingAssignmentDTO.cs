//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Newtonsoft.Json;
    using GuidedWork;

    public class WarehousePickingAssignmentDTO : VersionedDTO
    {
        [JsonProperty(PropertyName = "@xsi.type")]
        public string XsiType { get; set; }

        public int Sequence { get; set; }

        public string Aisle { get; set; }

        public string SlotId { get; set; }

        public string TripId { get; set; }

        public int SubCenterId { get; set; }

        public string CheckDigit { get; set; }

        public int PickQuantity { get; set; }

        public int PickedQuantity { get; set; }

        public int LastPickedQuantity { get; set; }

        public bool ShortedIndicator { get; set; }

        public string StoreNumber { get; set; }

        public string DoorNumber { get; set; }

        public string CheckPattern { get; set; }

        public string RouteNumber { get; set; }

        public long WorkerId { get; set; }

        public int Status { get; set; }

        public ProductDTO Product { get; set; }

        public LocationDTO Location { get; set; }
    }
}
