//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLinkArtisanModule
{
    using System.Collections.Generic;
    using GuidedWork;
    using GuidedWork.VoiceCatalyst;
    using VoiceLink;

    /// <summary>
    /// The information needed to generate a VAD for the VoiceLink workflow. 
    /// </summary>
    public class VoiceLinkVoiceCatalystWorkflowDTO : IVoiceCatalystWorkflowDTO
    {
        /// <summary>
        /// The constructor for <see cref="VoiceLinkVoiceCatalystWorkflowDTO"/>.
        /// </summary>
        /// <param name="moduleVocab">The vocabulary for the VoiceLink module.
        /// </param>
        /// <param name="voiceCatalystRequiredVocab">The required voice catalyst
        /// vocabulary.</param>
        public VoiceLinkVoiceCatalystWorkflowDTO(IModuleVocab moduleVocab,
            IVoiceCatalystRequiredVocab voiceCatalystRequiredVocab)
        {
            Vocabulary = VocabularyUtils.GetVocabularyForModuleVocab(
                voiceCatalystRequiredVocab, moduleVocab);
        }

        /// <summary>
        /// The name of the workflow.
        /// </summary>
        public string Workflow => VoiceLinkModule.VoiceLinkWorkflowName;

        /// <summary>
        /// A list of repository entries to populate a VAD's configurable
        /// property list.
        /// </summary>
        public List<RepositoryEntry> Repository { get; } = new List<RepositoryEntry>
        {
            new RepositoryEntry { PropertyName = "WorkflowFilterChoice", DisplayName = "Server or EmbeddedDemo", ConsoleType = "string", Required = "true", DefaultValue = "EmbeddedDemo" },
            new RepositoryEntry { PropertyName = "Host", DisplayName = "VoiceLink Host", ConsoleType = "string", Required = "true", DefaultValue = "<IP or host name>" },
            new RepositoryEntry { PropertyName = "ODRPort", DisplayName = "ODR Port", ConsoleType = "int", Required = "true", DefaultValue = "15009" },
            new RepositoryEntry { PropertyName = "Port", DisplayName = "LUT or Host port", ConsoleType = "int", Required = "true", DefaultValue = "15008" },
            new RepositoryEntry { PropertyName = "SecureConnections", DisplayName = "Use Secure Connections", ConsoleType = "boolean", Required = "false", DefaultValue = "false" },
            new RepositoryEntry { PropertyName = "SiteName", DisplayName = "Site Name", ConsoleType = "string", Required = "true", DefaultValue = "Default" }
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
                "VoiceLinkResources", "DevKitResources", "GuidedWorkCoreResources"
            }
        };
    }
}
