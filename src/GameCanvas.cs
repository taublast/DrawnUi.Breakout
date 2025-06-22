using DrawnUi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreakoutGame
{
    public class GameCanvas : Canvas
    {
        public GameCanvas()
        {
            RenderingMode = RenderingModeType.Accelerated;
            BackgroundColor = Colors.Black;
            Gestures = GesturesMode.Enabled;
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
        }

        protected override ISkiaControl CreateContent(bool firsttime)
        {
            return new BreakoutGame.Game.BreakoutGame();
        }
    }
}
