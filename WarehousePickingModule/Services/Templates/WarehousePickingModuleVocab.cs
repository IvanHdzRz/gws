//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;
    using System.Linq;

    /// <summary>
    /// A class that provides the vocabulary for the Warehouse Picking Module.
    /// </summary>
    public class WarehousePickingModuleVocab : DefaultModuleVocab
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
                new VocabWordInfo("VocabWord_EndOrder"),
                new VocabWordInfo("VocabWord_LastPick"),
                new VocabWordInfo("VocabWord_OrderStatus"),
                new VocabWordInfo("VocabWord_SignOff"),
                new VocabWordInfo("VocabWord_SkipProduct")
            }).ToArray();
    }
}
