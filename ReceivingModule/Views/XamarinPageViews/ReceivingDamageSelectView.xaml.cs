//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using SkiaSharp;
    using SkiaSharp.Views.Forms;

    public partial class ReceivingDamageSelectView : FlowListSelectionView
    {
        public ReceivingDamageSelectView(SelectionViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();

            BindingContext = viewModel;
            ListView = SelectListView;
        }

        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPaint paintBlueCircle = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse("1792E5"),
                IsAntialias = true,
            };

            canvas.DrawCircle(info.Width / 2f, info.Height / 2f, info.Height / 2f, paintBlueCircle);
        }
    }
}
