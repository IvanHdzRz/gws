///////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;
    using Newtonsoft.Json;

    public class LocationDTO : VersionedDTO
    {
        public string DescriptiveText { get; set; }

        public string CompactDescriptiveText { get; set; }

        public string DescriptiveSpoken { get; set; }

        public long DefinitionId { get; set; }

        public string StockingStagingName { get; set; }

        public string VerificationCode { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<LocationDescriptorDTO>))]
        public List<LocationDescriptorDTO> Descriptors { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<SiteDTO>))]
        public List<SiteDTO> Sites { get; set; }
    }
}
