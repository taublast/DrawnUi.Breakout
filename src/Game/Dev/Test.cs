using AppoMobi.Maui.Gestures;
using SkiaSharp;

namespace Breakout.Game.Dev
{
    //public class Test : Canvas
    //{
    //    public Test()
    //    {
    //        RetainedMode = true;

    //        Gestures = GesturesMode.Enabled;
    //        HardwareAcceleration = HardwareAccelerationMode.Enabled;
    //        HorizontalOptions = LayoutOptions.Fill;
    //        VerticalOptions = LayoutOptions.Fill;
    //        BackgroundColor = Colors.DarkGrey;
    //        Content = new TouchLayout()
    //        {
    //        };
    //    }
    //}

    /// <summary>
    /// Just testing retained here. Could rewrite game to use retained rendering at some point.
    /// </summary>
    public class TouchLayout : SkiaLayout
    {
        private PointF? _tapped;
        private List<PointF> _brushStrokes = new List<PointF>();
        private float _brushSize = 30;
        private SKColor _brushColor = SKColors.Blue;

        public TouchLayout()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
        }

        protected override void Paint(DrawingContext ctx)
        {
            base.Paint(ctx);

            if (_tapped != null)
            {
                var canvas = ctx.Context.Canvas;


                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = _brushColor,
                    Style = SKPaintStyle.Fill
                };

                canvas.DrawCircle(_tapped.Value.X, _tapped.Value.Y,
                    _brushSize, paint);

                // Add to our persistent collection
                _brushStrokes.Add(_tapped.Value);
                _tapped = null;

                // Alternate brush colors to make it easy to see each new tap
                if (_brushColor == SKColors.Blue)
                    _brushColor = SKColors.Red;
                else if (_brushColor == SKColors.Red)
                    _brushColor = SKColors.Green;
                else
                    _brushColor = SKColors.Blue;

                _tapped = null;
            }
        }

        public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args,
            GestureEventProcessingInfo apply)
        {
            if (args.Type == TouchActionResult.Tapped)
            {
                _tapped = args.Event.Location;


                return this;
            }

            return base.ProcessGestures(args, apply);
        }
    }
}