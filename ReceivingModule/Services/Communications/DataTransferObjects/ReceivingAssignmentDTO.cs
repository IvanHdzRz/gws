//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using GuidedWork;
    using System.Linq;

    public class ReceivingAssignmentDTO : AssignmentDTO
    {
        public LocationDTO FrontOfStoreLocation { get; set; }

        //implicit conversion operator from ReceivingAssignmentDTO to ReceivingWorkItem
        public static implicit operator ReceivingWorkItem(ReceivingAssignmentDTO dto)
        {
            var lwi = new ReceivingWorkItem
            {
                ID = dto.Id,
                ProductIdentifier = dto.Product.ProductIdentifier,
                ProductNumbersString = string.Join(" ", dto.Product.AcceptedIdentifiers?.ToArray() ?? Enumerable.Empty<string>()),
                ProductName = dto.Product.Name,
                ProductDestinationText = dto.FrontOfStoreLocation.DescriptiveText,
                ProductImageName = $"{dto.Product.ProductIdentifier}.png",
                WorkerID = dto.WorkerId,
                Sequence = dto.CompletionOrder,
                RequestedQuantity = dto.Product.StockOnHand,
                ReceivedQuantity = 0,
                LastReceivedQuantity = 0,
                Damaged = false
            };

            if (dto.Status == 1)
            {
                lwi.Completed = false;
                lwi.InProgress = true;
            }
            else if (dto.Status == 2)
            {
                lwi.Completed = true;
                lwi.InProgress = true;
            }
            else
            {
                lwi.Completed = false;
                lwi.InProgress = false;
            }

            return lwi;
        }

        public static implicit operator Product(ReceivingAssignmentDTO dto)
        {
            var product = new Product();

            product.ID = dto.Product.Id;
            product.Description = dto.Product.Description;
            product.Name = dto.Product.Name;
            product.ProductIdentifier = dto.Product.ProductIdentifier;

            return product;
        }
    }
}
