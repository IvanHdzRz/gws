//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class Login
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
    }

    public class Company
    {
        public int Id { get; set; }
        public string DatabaseName { get; set; }
    }

    public class Warehouse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class Zone
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PickRoute
    {
        public int ProductId { get; set; }
        public int LineId { get; set; }
        public int LocationId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public float QtyToPick { get; set; }
        public string LocationName { get; set; }
        public IList<string> BatchNumbers { get; set; }
        public IList<string> SerialNumbers { get; set; }
        public int OrderId { get; set; }
        public string UOM { get; set; }
        public string CustomerName { get; set; }
        //Jelo do we need a product checkdigit too?
        public string LocationCheckDigit { get; set; }
        public string ProductCheckDigit { get; set; }
        public byte[] GetImageDataFromEmbededResource()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string fileName = ProductCode + ".png";

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(fileName, StringComparison.Ordinal))
                {
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }
            }

            return null;
        }
    }
}
