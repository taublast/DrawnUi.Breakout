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
            Gestures = GesturesMode.Enabled;
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
        }

        protected override void Create(bool firsttime)
        {
            base.Create(firsttime);

            //will be called by code-behind hotreload
            Content = new BreakoutGame.Game.BreakoutGame();
        }
    }
}
