using System.Diagnostics;
using System.Windows.Input;

namespace MauiSpaceInvaders.SpaceInvaders
{
    internal class SpaceInvadersGraphicsView : GraphicsView
    {
        public static readonly BindableProperty XAxisScaleProperty = BindableProperty.Create(nameof(XAxisScale),
            typeof(double),
            typeof(SpaceInvadersGraphicsView),
            0.5,
            propertyChanged: (b,o,n) => {
                Drawable.XAxis = (double)n;
            });

        public double XAxisScale
        {
            get => (double)GetValue(XAxisScaleProperty);
            set => SetValue(XAxisScaleProperty, value);
        }

        public static ICommand Fire = new Command(() => Drawable.Fire(true));


        public static SpaceInvadersDrawable Drawable;
        public SpaceInvadersGraphicsView()
        {
            base.Drawable = Drawable = new SpaceInvadersDrawable();

            var ms = 1000.0 / _fps;
            var ts = TimeSpan.FromMilliseconds(ms);
            Device.StartTimer(ts, TimerLoop);
        }
        private bool TimerLoop()
        {
            if (_stopWatch.Elapsed.Seconds == 1 || _stopWatch.Elapsed.Seconds > _elapsedSeconds)
            {
                
                _elapsedSeconds = _stopWatch.Elapsed.Seconds;
            }

            // get the elapsed time from the stopwatch because the 1/30 timer interval is not accurate and can be off by 2 ms
            var dt = _stopWatch.Elapsed.TotalSeconds;
            _stopWatch.Restart();

            // calculate current fps
            var fps = dt > 0 ? 1.0 / dt : 0;

            // when the fps is too low reduce the load by skipping the frame
            if (fps < _fps / 2)
                return true;

            _fpsCount++;
            _fpsElapsed++;

            if (_fpsCount == 20)
                _fpsCount = 0;

            //Its been a second
            if (_fpsElapsed == _fps)
            {
                _fpsElapsed = 0;
                Drawable.AlienFire();
            }

            Invalidate();

            return true;
        }

        private int _fpsElapsed;
        private int _fpsCount = 0;
        private long _elapsedSeconds;
        private const double _fps = 30;
        private readonly Stopwatch _stopWatch = new Stopwatch();
    }
}