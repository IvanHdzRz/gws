//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWork;

    /// <summary>
    /// Objects conforming to this interface operate on the data store of work
    /// items, whether remote or local.
    /// </summary>
    public interface IWarehousePickingDataService
    {
        /// <summary>
        /// Retrieves a list of all the WarehousePickingWorkItem objects in this trip
        /// </summary>
        /// <returns> List of WarehousePickingWorkItem objects that fit the criteria in the database query</returns>
        List<WarehousePickingWorkItem> GetAllWorkItems();

        /// <summary>
        /// Retrieves a list of all the WarehousePickingWorkItem objects that have not been completed
        /// </summary>
        /// <returns> List of WarehousePickingWorkItem objects that fit the criteria in the database query</returns>
        List<WarehousePickingWorkItem> GetAllCurrentAndUpcomingWorkItems();
        
		/// <summary>
        /// Gets the list of (ordered) location descriptors for a Product ID.
        /// </summary>
        /// <returns>The location descriptors.</returns>
        /// <param name="productID">Product ID.</param>
        List<List<LocationDescriptor>> GetLocationDescriptorsListForProductId(long productID);

        /// <summary>
        /// Gets the total quantity remaining to be picked.
        /// </summary>
        /// <returns>Total quantity remaining</returns>
        int GetTotalQuantityRemaining();

        /// <summary>
        /// Gets the total quantity that has been picked.
        /// </summary>
        /// <returns></returns>
        int GetTotalQuantityPicked();

        /// <summary>
        /// Gets the next work item.
        /// </summary>
        /// <returns>The next work item.</returns>
        WarehousePickingWorkItem GetNextWorkItem();

        /// <summary>
        /// Returns the number of completed work items specific to this workflow (WarehousePickingWorkItems)
        /// </summary>
        /// <returns>The number completed work items.</returns>
        int GetNumCompletedWorkItems();

        /// <summary>
        /// Returns the number of work items specific to this workflow (WarehousePickingWorkItem)
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
        /// Reset the embedded data so that the assignment can be executed again.
        /// </summary>
        void ResetPicks();

        /// <summary>
        /// Gets the product for a given ID.
        /// </summary>
        /// <returns>The product.</returns>
        /// <param name="id">The Id of the database row.</param>
        Product GetProduct(long id);

        /// <summary>
        /// Set the specific work item to completed.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        /// <param name="workItem">The specific work item.</param>
        Task SetWorkItemCompleteAsync(WarehousePickingWorkItem workItem);

        /// <summary>
        /// Set the specific work item to in progress.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        /// <param name="workItem">The specific work item.</param>
        Task SetWorkItemInProgressAsync(WarehousePickingWorkItem workItem);

        /// <summary>
        /// Set the specific work item to skipped.  This in
        /// effect modifies its sequence order to be last in the assignment.
        /// </summary>
        /// <param name="workItem">The specific work item.</param>
        void SetWorkItemSkipped(WarehousePickingWorkItem workItem);

        /// <summary>
        /// Store the picked quantity in the data service and notify the proxy.
        /// </summary>
        /// <param name="workItemId">The id of the work item whose quantity you want to store</param>
        /// <param name="quantity">The quantity picked.</param>
        /// <returns></returns>
        Task StorePickedQuantityAsync(string workItemId, int quantity);
    }
}
