using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui.Graphics.Text;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIInvaders
{
    internal class SpaceInvadersDrawable : IDrawable
    {
        public SpaceInvadersDrawable()
        {
            
           //TODO EnableTouchEvents = true;

            var ms = 1000.0 / _fps;
            var ts = TimeSpan.FromMilliseconds(ms);
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            _dpi = mainDisplayInfo.Density;

            Device.StartTimer(ts, TimerLoop);

            _primaryPaint = new SKPaint()
            {
                TextSize = 100,
                Color = new SKColor(50, 205, 50)
            };


            _secondaryPaint = new SKPaint()
            {
                TextSize = 36,
                Color = Color.FromHex("#000000").AsSKColor()
            };
        }

        public void Draw(ICanvas canvas, RectangleF dirtyRect)
        {
            //TODO aliens are not attacking

            _info = dirtyRect;

            const string Fire = "Fire";
            const string YouWin = "YOU WIN";
            const string GameOver = "GAME OVER";

            if (!_aliensLoaded)
                LoadAliens();

            if (_selectedCoordinate.Y == 0)
                _selectedCoordinate = new SKPoint(_info.Center.X, _info.Center.Y);

            if (_aliens.Count == 0)
            {
                //TODO code smell
                _isGameOver = true;
                PresentEndGame(canvas, YouWin);
                return;
            }

            if (_isGameOver)
            {
                PresentEndGame(canvas, GameOver);
                return;
            }
            //TODO did this work?
            canvas.ResetState();

            _jet = SKPath.ParseSvgPathData(Constants.JetSVG);

            // calculate the scaling need to fit to screen
            var scaleX = 100 / _jet.Bounds.Width;

            var jetMatrix = SKMatrix.CreateTranslation(
                _selectedCoordinate.X - (_jet.Bounds.Width * scaleX),
                _info.Height - _jet.Bounds.Height - _bulletDiameter);

            // draw the jet
            _jet.Transform(jetMatrix);
            //canvas.DrawPath(_jet, _primaryPaint);

            //Draw fire button
            _buttonPath = new SKPath();

            var buttonCentre = new SKPoint(_info.Width - 100, _info.Height - 100);
            _buttonPath.MoveTo(buttonCentre);
            _buttonPath.LineTo(new SKPoint(buttonCentre.X, buttonCentre.Y));
            _buttonPath.ArcTo(new SKRect(
                buttonCentre.X - (_buttonDiameter / 2),
                buttonCentre.Y - (_buttonDiameter / 2),
                buttonCentre.X + (_buttonDiameter / 2),
                buttonCentre.Y + (_buttonDiameter / 2)
                ), 0, 350, true);

            //canvas.DrawPath(_buttonPath, _primaryPaint);

            //Draw bullets
            for (int i = _bullets.Count - 1; i > -1; i--)
            {
                _bullets[i] = new SKPoint(_bullets[i].X, _bullets[i].Y - _bulletSpeed);
                //TODO canvas.DrawCircle(_bullets[i], _bulletDiameter, _primaryPaint);

                var alienTarged = _aliens.Any(alien => alien.Contains(_bullets[i].X, _bullets[i].Y));
                //Remove any aliens touched by the bullet
                _aliens.RemoveAll(alien => alien.Contains(_bullets[i].X, _bullets[i].Y));
                //Remove bullet that touched alien
                if (alienTarged)
                    _bullets.RemoveAt(i);
            }
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
            //TODO
            //InvalidateSurface();

            return true;
        }

        private void LoadAliens()
        {
            const int AlienCount = 35;
            const int AlienSpacing = 50;

            for (var i = 0; i < AlienCount; i++)
            {
                var alien = SKPath.ParseSvgPathData(Constants.AlienSVG);
                var alienLength = (float)_dpi * 33;
                var alienScaleX = alienLength / alien.Bounds.Width;
                var alienScaleY = alienLength / alien.Bounds.Height;

                alien.Transform(SKMatrix.CreateScale(alienScaleX, alienScaleY));

                //how many aliens fit into legnth
                //TODO can this be moved outside the loop?
                var a = (_info.Width - _buttonDiameter) / (alien.Bounds.Width + AlienSpacing);
                var columnCount = Convert.ToInt32(a - 2);

                var columnIndex = i % columnCount;
                var rowIndex = Math.Floor(i / (double)columnCount);

                var x = alien.Bounds.Width * (columnIndex + 1) + (AlienSpacing * (columnIndex + 1));
                var y = alien.Bounds.Height * (rowIndex + 1) + (AlienSpacing * (rowIndex + 1));

                var alienTranslateMatrix = SKMatrix.CreateTranslation((float)x, (float)y);

                alien.Transform(alienTranslateMatrix);
                _aliens.Add(alien);
            }

            _aliensLoaded = true;
        }

        private void PresentEndGame(ICanvas canvas, string title)
        {
            //TODO did that work canvas.Clear();
            canvas.ResetState();

            

            var textWidth = _primaryPaint.MeasureText(title);
            IAttributedText attributedText = MarkdownAttributedTextReader.Read(title);
           
            //TODO canvas.DrawText(attributedText, _info.Center.X - (textWidth / 2), _info.Center.Y,99,99, _primaryPaint);

            _buttonPath = new SKPath();

            var buttonCentre = new SKPoint(_info.Width - 100, _info.Height - 100);
            _buttonPath.MoveTo(buttonCentre);
            _buttonPath.LineTo(new SKPoint(buttonCentre.X, buttonCentre.Y));
            _buttonPath.ArcTo(new SKRect(
                buttonCentre.X - (_buttonDiameter / 2),
                buttonCentre.Y - (_buttonDiameter / 2),
                buttonCentre.X + (_buttonDiameter / 2),
                buttonCentre.Y + (_buttonDiameter / 2)
                ), 0, 350, true);

            //TODO canvas.DrawPath(_buttonPath, _primaryPaint);
            var width = _secondaryPaint.MeasureText("Play");
            //TODO canvas.DrawText("Play", new SKPoint(buttonCentre.X - (width / 2), buttonCentre.Y + (_secondaryPaint.TextSize / 3)), _secondaryPaint);
        }

        private SKPath _jet;
        private double _dpi;
        private bool _isGameOver;
        private RectangleF _info;
        private int _fpsCount = 0;
        private SKPath _buttonPath;
        private bool _aliensLoaded;
        private int _bulletSpeed = 10;
        private SKPaint _primaryPaint;
        private const double _fps = 30;
        private int _bulletDiameter = 4;
        private SKPaint _secondaryPaint;
        private int _buttonDiameter = 100;
        private SKPoint _selectedCoordinate;
        private List<SKPath> _aliens = new List<SKPath>();
        private List<SKPoint> _bullets = new List<SKPoint>();
        private readonly Stopwatch _stopWatch = new Stopwatch();
    }
}
