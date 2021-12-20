//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWork;

    public interface IReceivingDataService
    {
        /// <summary>
        /// Retrieves a list of all the ReceivingWorkItem objects in this trip
        /// </summary>
        /// <returns> List of ReceivingWorkItem objects that fit the criteria in the database query</returns>
        List<ReceivingWorkItem> GetAllWorkItems();

        /// <summary>
        /// Retrieves a list of all the ReceivingWorkItem objects that have not been completed
        /// </summary>
        /// <returns> List of ReceivingWorkItem objects that fit the criteria in the database query</returns>
        List<ReceivingWorkItem> GetAllCurrentAndUpcomingWorkItems();

        /// <summary>
        /// Gets the product for a given ID.
        /// </summary>
        /// <returns>The product.</returns>
        /// <param name="id">The Id of the database row.</param>
        Product GetProduct(string id);

        /// <summary>
        /// Gets the work items specific to this workflow (ReceivingWorkItems) from the server
        /// </summary>
        /// <returns>A task representing the work to retrieve the work items from the server
        /// and put them in the database</returns>
        Task GetReceivingWorkItemsAsync();

        /// <summary>
        /// Returns a list of ReceivingWorkItems whose product matches the passed in 
        /// product id or number
        /// </summary>
        /// <returns>List of receiving work items whose product maches the product id or identifier</returns>
        /// <param name="productIdOrNumber">Product identifier or number.</param>
        List<ReceivingWorkItem> GetMatchingReceivingWorkItems(string productIdOrNumber);

        /// <summary>
        /// Gets the next ReceivingWorkItem from the app side database
        /// </summary>
        /// <returns>TThe next ReceivingWorkItem</returns>
        ReceivingWorkItem GetNextWorkItem();

        /// <summary>
        /// Returns the number of completed work items specific to this workflow (ReceivingWorkItem)
        /// </summary>
        /// <returns>The number completed work items.</returns>
        int GetNumCompletedWorkItems();

        /// <summary>
        /// Returns the number of work items specific to this workflow (ReceivingWorkItem)
        /// </summary>
        /// <returns>The number work items.</returns>
        int GetNumWorkItems();

        /// <summary>
        /// Store the changes to the workitem in the data service.
        /// </summary>
        /// <param name="workItem">The work item to be updated.</param>
        void UpdateWorkItem(ReceivingWorkItem workItem);

        /// <summary>
        /// Set the specific work item to skipped.  This in
        /// effect modifies its sequence order to be last in the assignment.
        /// </summary>
        /// <param name="workItem">The specific work item.</param>
        void SetWorkItemSkipped(ReceivingWorkItem workItem);

        /// <summary>
        /// Set the specific work item to completed.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        /// <param name="workItem">The specific work item.</param>
        Task SetWorkItemCompleteAsync(ReceivingWorkItem workItem);
    }
}
