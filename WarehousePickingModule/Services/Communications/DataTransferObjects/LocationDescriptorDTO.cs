//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    public class LocationDescriptorDTO : VersionedDTO
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string Spoken { get; set; }

        public int DescOrder { get; set; }

        public bool UsedForInterleaving { get; set; }
    }
}
