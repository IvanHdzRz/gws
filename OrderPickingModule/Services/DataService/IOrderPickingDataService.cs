//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWork;
    using Retail;

    public interface IOrderPickingDataService
    {
        /// <summary>
        /// Retrieves a list of all the OrderPickingWorkItem objects that have not been completed
        /// </summary>
        /// <returns> List of OrderPickingWorkItem objects that fit the criteria in the database query</returns>
        List<OrderPickingWorkItem> GetAllCurrentAndUpcomingWorkItems();

        /// <summary>
        /// Retrieves a list of order picking containers associated with each order
        /// </summary>
        /// <returns>List of Order Picking Containers</returns>
        List<OrderPickingContainer> GetContainersForOrders();

        /// <summary>
        /// Gets the order picking order information.
        /// </summary>
        /// <returns>The order picking order information.</returns>
        /// <param name="workItem">Work item.</param>
        OrderPickingOrderInfo GetInformation(OrderPickingWorkItem workItem);
        
		/// <summary>
        /// Gets the list of (ordered) location descriptors for a Product ID.
        /// </summary>
        /// <returns>The location descriptors.</returns>
        /// <param name="productID">Product ID.</param>
        List<List<LocationDescriptor>> GetLocationDescriptorsListForProductId(long productID);

        /// <summary>
        /// Gets the next work item.
        /// </summary>
        /// <returns>The next work item.</returns>
        OrderPickingWorkItem GetNextWorkItem();

        /// <summary>
        /// Returns the number of completed work items specific to this workflow (OrderPickingWorkItems)
        /// </summary>
        /// <returns>The number completed work items.</returns>
        int GetNumCompletedWorkItems();

        /// <summary>
        /// Returns the number of work items specific to this workflow (OrderPickingWorkItem)
        /// </summary>
        /// <returns>The number work items.</returns>
        int GetNumWorkItems();

        /// <summary>
        /// Retrieves Picks for the current worker and stores them locally.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        Task GetPicksAsync();

        /// <summary>
        /// Finds the previous substitution for the passed in current substitution/work item
        /// </summary>
        /// <param name="currentSub">The current substitution</param>
        /// <param name="workItem">The current work item for that represents the order</param>
        /// <returns></returns>
        ProductSubstitutionMap GetPreviousSubstitution(ProductSubstitutionMap currentSub, OrderPickingWorkItem workItem);

        /// <summary>
        /// Gets the product for a given ID.
        /// </summary>
        /// <returns>The product.</returns>
        /// <param name="id">The Id of the database row.</param>
        Product GetProduct(long id);

        /// <summary>
        /// Gets the next available substitution for the work item.
        /// </summary>
        /// <param name="workItem">The order picking work item.</param>
        /// <returns>The highest-priority, unused substitution available.</returns>
        ProductSubstitutionMap GetSubstitution(OrderPickingWorkItem workItem);

        /// <summary>
        /// Get an indication if there are substitutions available for the work item.
        /// </summary>
        /// <param name="workItem">The order picking work item.</param>
        /// <returns>True if substitutions are available</returns>
        bool GetSubstitutionsAvailable(OrderPickingWorkItem workItem);

        /// <summary>
        /// Set the specific work item to completed.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        /// <param name="workItem">The specific work item.</param>
        Task SetWorkItemCompleteAsync(OrderPickingWorkItem workItem);

        /// <summary>
        /// Set the specific work item to in progress.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        /// <param name="workItem">The specific work item.</param>
        Task SetWorkItemInProgressAsync(OrderPickingWorkItem workItem);

        /// <summary>
        /// Set the specific work item to skipped.  This in
        /// effect modifies its sequence order to be last in the assignment.
        /// </summary>
        /// <param name="workItem">The specific work item.</param>
        void SetWorkItemSkipped(OrderPickingWorkItem workItem);

        /// <summary>
        /// Store the picked quantity in the data service and notify the proxy.
        /// </summary>
        /// <param name="workItemId">The id of the work item whose quantity you want to store</param>
        /// <param name="quantity">The quantity picked.</param>
        /// <returns></returns>
        Task StorePickedQuantityAsync(string workItemId, int quantity);

        /// <summary>
        /// Send the staging location for an order ID.
        /// </summary>
        /// <param name="orderId">The ID of the Order</param>
        /// <param name="stagingLocation">The staging location</param>
        /// <returns>A Task to indicate when the operation is complete</returns>
        Task StoreStagingLocationAsync(long orderId, string stagingLocation);

        /// <summary>
        /// Updates the database with the passed in substitution product
        /// </summary>
        /// <param name="sub"> The substitution product to update in the database</param>
        void UpdateSubInSubstitutionMap(ProductSubstitutionMap sub);
    }
}
