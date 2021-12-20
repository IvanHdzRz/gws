//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using SQLite;

    public class LocationDescriptor
    {
        [PrimaryKey]
        public long ID { get; set; }
        public long LocationID { get; set; }
        public string Name { get; set; }
        public string Spoken { get; set; }
        public string Value { get; set; }
        public int DescOrder { get; set; }
    }

    public class ProductLocation
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public long ProductID { get; set; }
        public long LocationID { get; set; }
    }
}