//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using GuidedWork;
    using GuidedWork.VoiceCatalyst;

    /// <summary>
    /// The information needed to generate a VAD for the LApp workflow. 
    /// </summary>
    public class LAppVoiceCatalystWorkflowDTO : IVoiceCatalystWorkflowDTO
    {
        /// <summary>
        /// The constructor for <see cref="LAppVoiceCatalystWorkflowDTO"/>.
        /// </summary>
        /// <param name="moduleVocab">The vocabulary for the LApp module.
        /// </param>
        /// <param name="voiceCatalystRequiredVocab">The required voice catalyst
        /// vocabulary.</param>
        public LAppVoiceCatalystWorkflowDTO(IModuleVocab moduleVocab,
            IVoiceCatalystRequiredVocab voiceCatalystRequiredVocab)
        {
             Vocabulary = VocabularyUtils.GetVocabularyForModuleVocab(
                 voiceCatalystRequiredVocab, moduleVocab);
        }

        /// <summary>
        /// The name of the workflow.
        /// </summary>
        public string Workflow => LAppModule.LAppWorkflowName;

        /// <summary>
        /// A list of repository entries to populate a VAD's configurable
        /// property list.
        /// </summary>
        public List<RepositoryEntry> Repository { get; } = new List<RepositoryEntry>
        {
            new RepositoryEntry { PropertyName = "WorkflowFilterChoice", DisplayName = "Server or EmbeddedDemo", ConsoleType = "string", Required = "true", DefaultValue = "EmbeddedDemo" },
            new RepositoryEntry { PropertyName = "Host", DisplayName = "Host", ConsoleType = "string", Required = "false", DefaultValue = "" },
            new RepositoryEntry { PropertyName = "Port", DisplayName = "Port", ConsoleType = "string", Required = "false", DefaultValue = "" },
            new RepositoryEntry { PropertyName = "Proxy", DisplayName = "Proxy", ConsoleType = "boolean", Required = "true", DefaultValue = "true" },
            new RepositoryEntry { PropertyName = "Secure", DisplayName = "Secure", ConsoleType = "boolean", Required = "true", DefaultValue = "true" },
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
                "LAppResources", "DevKitResources", "GuidedWorkCoreResources"
            }
        };
    }
}
