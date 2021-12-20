//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp.Artisan
{
    using System.Collections.Generic;
    using GuidedWork;
    using GuidedWork.VoiceCatalyst;
    using SimpleApp;

    /// <summary>
    /// The information needed to generate a VAD for the SimpleApp workflow. 
    /// </summary>
    public class SimpleAppVoiceCatalystWorkflowDTO : IVoiceCatalystWorkflowDTO
    {
        /// <summary>
        /// The constructor for <see cref="SimpleAppVoiceCatalystWorkflowDTO"/>.
        /// </summary>
        /// <param name="moduleVocab">The vocabulary for the SimpleApp module.
        /// </param>
        /// <param name="voiceCatalystRequiredVocab">The required voice catalyst
        /// vocabulary.</param>
        public SimpleAppVoiceCatalystWorkflowDTO(IModuleVocab moduleVocab,
            IVoiceCatalystRequiredVocab voiceCatalystRequiredVocab)
        {
            Vocabulary = VocabularyUtils.GetVocabularyForModuleVocab(
                voiceCatalystRequiredVocab, moduleVocab);
        }

        /// <summary>
        /// The name of the workflow.
        /// </summary>
        public string Workflow => SimpleAppModule.SimpleAppWorkflowName;

        /// <summary>
        /// A list of repository entries to populate a VAD's configurable
        /// property list.
        /// </summary>
        public List<RepositoryEntry> Repository { get; } = new List<RepositoryEntry>
        {
            new RepositoryEntry { PropertyName = "Host", DisplayName = "SimpleApp Host", ConsoleType = "string", Required = "true", DefaultValue = "<IP or host name>" },
            new RepositoryEntry { PropertyName = "Port", DisplayName = "Host port", ConsoleType = "int", Required = "true", DefaultValue = "9090" },
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
                "SimpleAppResources", "DevKitResources", "GuidedWorkCoreResources"
            }
        };
    }
}
