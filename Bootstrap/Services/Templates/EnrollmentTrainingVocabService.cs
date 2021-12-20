//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Bootstrap
{
    using System.Collections.Generic;
    using System.Linq;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;
    using TinyIoC;
    using Xamarin.Forms;

    /// <summary>
    /// A service that provides the words to train during enrollement training.
    /// </summary>
    public class EnrollmentTrainingVocabService : IEnrollmentTrainingVocabService
    {
        private IServerConfigRepository _ServerConfigRepo;

        /// <summary>
        /// Gets the vocab to train.
        /// </summary>
        /// <value>The vocab to train.</value>
        public virtual IEnumerable<VocabWordInfo> VocabToTrain => GenerateVocabList();

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:GuidedWork.EnrollmentTrainingVocabService"/> class.
        /// It builds <see cref="VocabToTrain"/> using
        /// <see cref="PlatformIndependentVocab"/> and the OS-specific vocab.
        ///
        /// You can either override the parts, or override
        /// <see cref="VocabToTrain"/> directly if you want to change the
        /// vocabulary.
        /// </summary>
        public EnrollmentTrainingVocabService(IServerConfigRepository serverConfigRepository)
        {
            _ServerConfigRepo = serverConfigRepository;
        }

        private IEnumerable<VocabWordInfo> GenerateVocabList()
        {
            var moduleVocab = TinyIoCContainer.Current.Resolve<IModuleVocab>(nameof(DefaultModuleVocab));
            if (TinyIoCContainer.Current.CanResolve<IModuleVocab>(_ServerConfigRepo.GetConfig("CurrentWorkflowName").Value))
            {
                moduleVocab = TinyIoCContainer.Current.Resolve<IModuleVocab>(_ServerConfigRepo.GetConfig("CurrentWorkflowName").Value);
            }

            var vocabToTrain = moduleVocab.PlatformIndependentVocab.ToList();

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    vocabToTrain.AddRange(moduleVocab.AndroidVocab);
                    break;

                case Device.iOS:
                    vocabToTrain.AddRange(moduleVocab.IosVocab);
                    break;
            }

            return vocabToTrain;
        }
    }
}