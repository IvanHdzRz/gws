//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePickingArtisanModule
{
    using System.Collections.Generic;
    using GuidedWork;
    using GuidedWork.VoiceCatalyst;
    using BasePicking;

    /// <summary>
    /// The information needed to generate a VAD for the LApp workflow.
    /// </summary>
    public class BasePickingVoiceCatalystWorkflowDTO : IVoiceCatalystWorkflowDTO
    {
        /// <summary>
        /// The constructor for <see cref="BasePickingVoiceCatalystWorkflowDTO"/>.
        /// </summary>
        /// <param name="moduleVocab">The vocabulary for the BasePicking module.
        /// </param>
        /// <param name="voiceCatalystRequiredVocab">The required voice catalyst
        /// vocabulary.</param>
        public BasePickingVoiceCatalystWorkflowDTO(IModuleVocab moduleVocab,
            IVoiceCatalystRequiredVocab voiceCatalystRequiredVocab)
        {
            Vocabulary = VocabularyUtils.GetVocabularyForModuleVocab(
                voiceCatalystRequiredVocab, moduleVocab);
        }

        /// <summary>
        /// The name of the workflow.
        /// </summary>
        public string Workflow => BasePickingModule.BasePickingWorkflowName;

        /// <summary>
        /// A list of repository entries to populate a VAD's configurable
        /// property list.
        /// </summary>
        public List<RepositoryEntry> Repository { get; } = new List<RepositoryEntry>
        {
            new RepositoryEntry { PropertyName = "WorkflowFilterChoice", DisplayName = "Server or EmbeddedDemo", ConsoleType = "string", Required = "true", DefaultValue = "EmbeddedDemo" },
            new RepositoryEntry { PropertyName = "Host", DisplayName = "BasePicking Host", ConsoleType = "string", Required = "false", DefaultValue = "<IP or host name>" },
            new RepositoryEntry { PropertyName = "Port", DisplayName = "LUT or Host port", ConsoleType = "int", Required = "true", DefaultValue = "8080" },
            new RepositoryEntry { PropertyName = "ODRPort", DisplayName = "ODR Port", ConsoleType = "int", Required = "true", DefaultValue = "15001" },
            new RepositoryEntry { PropertyName = "SecureConnections", DisplayName = "Use Secure Connections", ConsoleType = "boolean", Required = "true", DefaultValue = "false" },
            new RepositoryEntry { PropertyName = "PickQuantityCountdown", DisplayName = "Request remaining quantity if not fully picked", ConsoleType = "boolean", Required = "false", DefaultValue = "true" },
            new RepositoryEntry { PropertyName = "PickMethod", DisplayName = "Discrete or Cluster picking", ConsoleType = "string", Required = "false", DefaultValue = "Discrete" },
            new RepositoryEntry { PropertyName = "ConfirmLocation", DisplayName = "Confirm location with check digit", ConsoleType = "boolean", Required = "false", DefaultValue = "true" },
            new RepositoryEntry { PropertyName = "ConfirmProduct", DisplayName = "Confirm product with product id", ConsoleType = "boolean", Required = "false", DefaultValue = "true" },
            new RepositoryEntry { PropertyName = "ConfirmQuantityVoiceInput", DisplayName = "Confirm the quantity input", ConsoleType = "boolean", Required = "false", DefaultValue = "true" }
        };

        /// <summary>
        /// The full set of vocabulary for this workflow.
        /// </summary>
        public Vocabulary Vocabulary { get; }

        /// <summary>
        /// Information about the RESX resources used by this workflow.
        /// </summary>
        public Resources Resources { get; } = new Resources
        {
            BaseLocale = "en-US",
            ResourcePriority = new List<string> {
                "BasePickingResources", "DevKitResources", "GuidedWorkCoreResources"
            }
        };
    }
}
