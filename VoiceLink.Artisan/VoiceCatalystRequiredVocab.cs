//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLinkArtisanModule
{
    using GuidedWork.VoiceCatalyst;
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// SAMPLE: This is a sample of overriding the base default definitions
    ///         for catalyst required vocabulary.
    ///         
    /// In the VoiceLink sample module, the Yes and no are mapped to custom keys,
    /// but still "yes" and "no". If that were all, then there would be 
    /// no need for this class and it's overrides. However in this sample 
    /// we are alternating the spoken and display values
    /// 
    /// Note: that for yes and no, we still define the vocab words using the same keys as 
    ///       in the VoiceLinkModuleVocab class, and still use the custom keys VoiceLink_ButtonText_Yes
    ///       and VoiceLink_ButtonText_No respectively, but in here we provide additional/different spoken
    ///       and display keys
    ///       
    /// So in GuidedWork, you would have Yes/No as the vocab, but in this manner you can change the catalyst
    /// vocabs to affirmative/negative if desired by changing the VocabWord_Yes and VocabWord_No keys.
    /// 
    /// This is mainly intended for changing the phonetic pronunciations for the TTS, but could be used for mapping different
    /// vocab than the defaults (i.e. "OK" for "ready" or "volume up" for "louder")
    /// </summary>
    public class VoiceCatalystRequiredVocab : DefaultVoiceCatalystRequiredVocab
    {
        /// <summary>
        /// The mapping between the Voice Catalyst "no" vocabulary word and
        /// the Guided Work <see cref="T:Honeywell.Firebird.CoreLibrary.VocabWordInfo"/>.
        /// </summary>
        public override NoVoiceCatalystVocabMapping NoVocabMapping => new NoVoiceCatalystVocabMapping(new VocabWordInfo("VoiceLink_ButtonText_No", "VocabWord_No", "VocabWord_No"));

        /// <summary>
        /// The mapping between the Voice Catalyst "yes" vocabulary word and
        /// the Guided Work <see cref="T:Honeywell.Firebird.CoreLibrary.VocabWordInfo"/>.
        /// </summary>
        public override YesVoiceCatalystVocabMapping YesVocabMapping => new YesVoiceCatalystVocabMapping(new VocabWordInfo("VoiceLink_ButtonText_Yes", "VocabWord_Yes", "VocabWord_Yes"));
    }
}