//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;
    using Newtonsoft.Json;
    using SQLite;

    public class Hierarchy
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Identifier { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public long ProductId { get; set; }
        public string Spoken { get; set; }
        public bool IsLowestLevel { get; set; }
    }

    public class ProductDTO : VersionedDTO
    {
        public string ProductIdentifier { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public IList<string> AcceptedIdentifiers { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public IList<string> WrongLevelIdentifiers { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public IList<string> LowestLevelIdentifiers { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DisplayPrice { get; set; }

        public int StockOnHand { get; set; }

        public string Size { get; set; }

        public string Spoken { get; set; }

        public int? InTransit { get; set; }

        public int? LastReceived { get; set; }

        public string LastReceivedDate { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Hierarchy>))]
        public IList<Hierarchy> Hierarchy { get; set; }
    }
}
