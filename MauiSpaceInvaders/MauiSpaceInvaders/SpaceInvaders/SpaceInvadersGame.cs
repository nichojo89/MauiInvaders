using System.Diagnostics;

namespace MauiSpaceInvaders.SpaceInvaders
{
    internal class SpaceInvadersGame : GraphicsView
    {
        public SpaceInvadersGame()
        {
            Drawable = new SpaceInvadersDrawable();
            
            var ms = 1000.0 / _fps;
            var ts = TimeSpan.FromMilliseconds(ms);
            Device.StartTimer(ts, TimerLoop);

        }
        private bool TimerLoop()
        {
            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalSeconds;

            _stopWatch.Restart();

            // calculate current fps
            var fps = dt > 0 ? 1.0 / dt : 0;

            // when the fps is too low reduce the load by skipping the frame
            if (fps < _fps / 2)
                return true;

            _fpsCount++;

            if (_fpsCount == 20)
            {
                _fpsCount = 0;
            }

            Invalidate();

            return true;
        }

        private int _fpsCount = 0;
        private const double _fps = 30;
        private readonly Stopwatch _stopWatch = new Stopwatch();
    }
}