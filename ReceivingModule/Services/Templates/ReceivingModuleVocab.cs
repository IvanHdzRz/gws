//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;
    using System.Linq;

    /// <summary>
    /// A class that provides the vocabulary for the VoiceLink Module.
    /// </summary>
    public class ReceivingModuleVocab : DefaultModuleVocab
    {
        /// <summary>
        /// Gets the default platform independent vocabulary localization keys.
        /// It is constructed from the concatenation of
        /// <see cref="P:GuidedWork.DefaultModuleVocab.Numerics"/>, and
        /// <see cref="P:GuidedWork.DefaultModuleVocab.Alphas"/>. and workflow-
        /// specific keys.
        /// </summary>
        /// <value>The platform independent vocab.</value>
        public override VocabWordInfo[] PlatformIndependentVocab { get; } =
            BaseRequiredVocab
                .Concat(Numerics)
                .Concat(Common)
                .Concat(new []
            {
                new VocabWordInfo("VocabWord_SignOff")
            }).ToArray();

        /// <summary>
        /// Gets the iOS vocabulary localization keys.
        /// </summary>
        /// <value>The ios vocab.</value>
        public override VocabWordInfo[] IosVocab { get; } =
            new VocabWordInfo[]
            {
                new VocabWordInfo("VocabWord_Crushed"),
                new VocabWordInfo("VocabWord_Leaking"),
                new VocabWordInfo("VocabWord_Missing"),
                new VocabWordInfo("VocabWord_Dry"),
                new VocabWordInfo("VocabWord_Freezer"),
                new VocabWordInfo("VocabWord_Produce"),
                new VocabWordInfo("VocabWord_Good"),
                new VocabWordInfo("VocabWord_Damaged")
            };
    }
}
