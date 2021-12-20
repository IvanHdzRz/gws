//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird;

    /// <summary>
    /// The DialogueView that binds the SingleResponseDialogue and the the SingleResponseViewModel.
    /// </summary>
    public class WarehousePickingSingleResponseDialogueView : SingleResponseDialogueView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResponseDialogueView"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="logger">A logger.</param>
        /// <param name = "uiDelegate">The user interface delegate.</param>
        public WarehousePickingSingleResponseDialogueView(SingleResponseViewModel viewModel, ILog logger, IUIDelegate uiDelegate) : 
        base(viewModel, logger, uiDelegate)
        {
        }

        /// <summary>
        /// Called when this view is started.
        /// </summary>
        public override Task OnStartAsync()
        {
            // Register so that we hear about invalid responses and can act
            ViewModel.ValidationModel.OnInvalidResponseAsync += OnInvalidResponseAsync;

            return base.OnStartAsync();
        }

        /// <summary>
        /// Called when this view is stopped.
        /// </summary>
        public override Task OnStopAsync()
        {
            ViewModel.ValidationModel.OnInvalidResponseAsync -= OnInvalidResponseAsync;
            return base.OnStopAsync();
        }

        /// <summary>
        /// In this method, we don't know which view gave the invalid response. Therefore, we'll
        /// be sure to reset the dialogue back to the start in case the user did some voice entry
        /// - we don't want that lingering on the retry.
        /// 
        /// Note that when the DialogueRunner returns a result, it is completely stopped.
        /// 
        /// TODO - it would be nice if this DialogueRunner restart stuff was done in the base class and hidden
        /// from custom DialogueViews needing to do this. 
        /// 
        /// </summary>
        protected virtual async Task OnInvalidResponseAsync(string invalidResponse)
        {
            // We must restart the dialogue as it's possible that 
            // the user entered a few digits vocally, then confirmed through the screen.
            await DialogueRunner.Stop();

            string errorMessage = invalidResponse;

            // If the default message is set then speak that message, else repeat the invalidResponse
            if (!string.IsNullOrEmpty(ViewModel.ValidationModel.DefaultInvalidResponseMessage))
            {
                errorMessage = ViewModel.ValidationModel.DefaultInvalidResponseMessage;
            }

            DialogueRunner.SpeakPrompt(errorMessage, true, true);

            // Restart the same dialogue. 
            await DialogueRunner.Start(Dialogue);
        }
    }
}
