//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWork;

    public class ReceivingUser
    {
        public string Id { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// The inteface for view models for the Receiving
    /// workflow.  The interface abstracts the retrieve of data from the 
    /// view model.  The model serves as temporary storage of information
    /// between view models associate with simple WFAs.
    /// </summary>
    public interface IReceivingModel : IWorkflowModel
    {
        /// <summary>
        /// Maintains the application's state in the workflow.
        /// </summary>
        IReceivingStateMachine StateMachine { get; }

        /// <summary>
        /// Consolidated grouping of data items required for presentation layer.
        /// </summary>
        ReceivingDataStore DataStore { get; }

        string CurrentUserMessage { get; set; }

        string SelectedOrder { get; set; }
        string EnteredHiQuantity { get; set; }
        string EnteredTiQuantity { get; set; }
        bool QuantityConfirmation { get; set; }
        string AcknowledgePrintingLabel { get; set; }
        string AcknowledgeApplyLabel { get; set; }
        bool PalletCondition { get; set; }
        string DamageReason { get; set; }
        string AcknowledgeInvoiceSummary { get; set; }
        bool NoMoreConfirmation { get; set; }

        ReceivingUser User { get; }
        void UpdateUser(string userId, string password);

        Task ProcessUserInputAsync();
        Task<bool> ValidateUserCredentialsAsync();

        Task GetReceivingWorkItemsAsync();

        /// <summary>
        /// Gets and sets the current barcode of the product being selected.
        /// </summary>
        string CurrentBarcode { get; set; }

        /// <summary>
        /// Gets and sets the last invalid barcode, so that it can be displayed
        /// in another view.
        /// </summary>
        string LastInvalidBarcode { get; set; }

        /// <summary>
        /// Determines if there are any receiving items to pick.
        /// </summary>
        bool MoreWorkItems { get; }

        /// <summary>
        /// Gets the index of the current receiving work item.  This can be displayed to give
        /// an indication of progress, e.g. item 1 of 7.
        /// </summary>
        int CurrentWorkItemIndex { get; }

        /// <summary>
        /// Gets the total number of receiving work items assigned to the user.  This can be
        /// displayed to give an indication of progress, e.g. item 1 of 7.
        /// </summary>
        int TotalNumberOfWorkItems { get; }

        /// <summary>
        /// Gets the current receiving work item.
        /// </summary>
        ReceivingWorkItem ReceivingWorkItem { get; }

        /// <summary>
        /// Gets the current last valid receiving work item.
        /// </summary>
        ReceivingWorkItem LastValidWorkItem { get; set; }

        /// <summary>
        /// Gets the image path for the product associated with the work item.
        /// </summary>
        string CurrentImagePath { get; }

        /// <summary>
        /// Gets the current product's information.
        /// </summary>
        Product CurrentProduct { get; }

        /// <summary>
        /// Updates the remaining quantity to be picked in the work item.
        /// </summary>
        void UpdateRemainingWorkItemQuantity(int quantityPicked);

        /// <summary>
        /// Indicate if the product is damaged.
        /// </summary>
        /// <param name="damaged">Indicates if the product is damaged</param>
        void UpdateDamaged(bool damaged);

        /// <summary>
        /// Gets and sets the quantity of product last received
        /// </summary>
        /// <value>The quantity last picked.</value>
        int QuantityLastReceived { get; set; }

        /// <summary>
        /// Gets and sets the hi quantity of product last received
        /// </summary>
        /// <value>The quantity last picked.</value>
        int HiQuantityLastReceived { get; set; }

        /// <summary>
        /// Gets and sets the ti quantity of product last received
        /// </summary>
        /// <value>The quantity last picked.</value>
        int TiQuantityLastReceived { get; set; }

        /// <summary>
        /// Gets the product location text.
        /// </summary>
        string ProductLocationText { get; }

        /// <summary>
        /// Gets the product numbers.
        /// </summary>
        /// <value>The product numbers.</value>
        IReadOnlyList<string> ProductNumbers { get; }

        /// <summary>
        /// Gets the response expressions based for the product being loaded to cart.
        /// </summary>
        /// <param name="hintLength"> The lenght of the hint</param>
        /// <returns>response expressions.</returns>
        HashSet<string> GetResponseExpressions(int hintLength);

        /// <summary>
        /// Retrieves items that match the full or partial productIdentifier.
        /// </summary>
        /// <param name="productIdentifier">The full or partial identifier</param>
        /// <returns>A list of matching work items.</returns>
        List<ReceivingWorkItem> RetrieveMatchingWorkItems(string productIdentifier);

        /// <summary>
        /// Modifies the sequence number of the current work item to put it at
        /// the end of the assignment.
        /// </summary>
        void SetWorkItemSkipped();

        /// <summary>
        /// Set the current work item's completed flag.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task SetWorkItemCompleteAsync();

        /// <summary>
        /// Gets all of the non-completed ReceivingWorkItems for the current order
        /// </summary>
        /// <returns>List of non-completed ReceivingWorkItem objects</returns>
        List<ReceivingWorkItem> CurrentAndUpcomingReceivingWorkItems { get; }

        /// <summary>
        /// Gets all of the ReceivingWorkItems for the current order
        /// </summary>
        List<ReceivingWorkItem> AllReceivingWorkItems { get; }

        /// <summary>
        /// Retrieves a product based on passed in product id and extracts image path and product name
        /// </summary>
        /// <param name="productID">the product id of the target product to get information for</param>
        /// <returns>a list of strings where index 0 = product image path, index 1 = product name</returns>
        List<string> GetProductInfoForProductID(string productID);
    }
}
