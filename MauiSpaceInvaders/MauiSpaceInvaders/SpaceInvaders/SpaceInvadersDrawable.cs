﻿using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;

namespace MauiSpaceInvaders.SpaceInvaders
{
    internal class SpaceInvadersDrawable : IDrawable
    {
        public double XAxis { get; set; }

        public SpaceInvadersDrawable()
        {
            //TODO EnableTouchEvents = true;


            try
            {
                
                _dpi = DeviceDisplay.MainDisplayInfo.Density;
            }
            catch (Exception e)
            {
                _dpi = 3;
            }
            XAxis = 0.5;
           

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

            if (_aliens.Count == -1)
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

            var jet = SKPath.ParseSvgPathData(Constants.JetSVG);

            _jet = new PathF();

            //TODO raise issuee that there is no PraseSVGPathData on PathF
            _jet = ParseSVGPathData(jet.Points);
            
            //TODO make _primaryPaint
            canvas.StrokeColor = Colors.Green;
            //canvas.FillColor = Colors.Green;
            // calculate the scaling need to fit to screen
            var scaleX = 100 / _jet.Bounds.Width;

            var jetScaleMatrix = System.Numerics.Matrix3x2.CreateScale(Scale);
            _jet.Transform(jetScaleMatrix);

            var jetTranslationMatrix = System.Numerics.Matrix3x2.CreateTranslation((float)(XAxis * (_info.Width - _jet.Bounds.Width)),
                 _info.Height - _jet.Bounds.Height - _bulletDiameter);
            
            _jet.Transform(jetTranslationMatrix);

            _jetMidX = _jet.Bounds.Center.X;

            // draw the jet
            canvas.DrawPath(_jet);

            //Draw bullets
            for (int i = _bullets.Count - 1; i > -1; i--)
            {
                _bullets[i] = new SKPoint(_bullets[i].X, _bullets[i].Y - _bulletSpeed);
                canvas.DrawCircle(_bullets[i].AsPointF(), _bulletDiameter);

                var alienTarged = _aliens.Any(alien => alien.Contains(_bullets[i].X, _bullets[i].Y));
                //Remove any aliens touched by the bullet
                _aliens.RemoveAll(alien => alien.Contains(_bullets[i].X, _bullets[i].Y));
                //Remove bullet that touched alien
                if (alienTarged)
                    _bullets.RemoveAt(i);
            }

            //Has an alien reached a horizontal edge of game?
            var switched = _aliens.Select(x => x.Bounds)
                .Any(x => x.Left < 0
                || x.Right > _info.Right);

            _aliensSwarmingRight = switched ? !_aliensSwarmingRight : _aliensSwarmingRight;

            //Has an alien hit the ships y axis?
            _isGameOver = _aliens
                .Select(x => x.Bounds.Bottom)
                .Any(x => x > _jet.Bounds.Top);
            _isGameOver = false;
            if (_isGameOver)
            {
                PresentEndGame(canvas, GameOver);
                return;
            }

            //Draw aliens
            for (var i = 0; i < _aliens.Count; i++)
            {
                //Move Aliens
                var alienMatrix = SKMatrix.CreateTranslation(
                _aliensSwarmingRight ? _alienSpeed : _alienSpeed * -1,
                switched ? 50 : 0);

                _aliens[i].Transform(alienMatrix);

                var alienPath = ParseSVGPathData(_aliens[i].Points);
                canvas.DrawPath(alienPath);
            }


            //Remove bullets that leave screen
            _bullets.RemoveAll(x => x.Y < 0);
        }
        
        /// <summary>
        /// TODO create PR to add ParseSVGPathData to Maui.Graphics
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private PathF ParseSVGPathData(SKPoint[] points)
        {
            var path = new PathF();
            for (int i = 0; i < points.Count(); i++)
            {
                var point = new PointF(points[i].X, points[i].Y);

                if (i == 0)
                {
                    path.MoveTo(point);
                }
                else if (i == points.Count() - 1)
                {
                    path.LineTo(point);
                    path.Close();
                }
                else
                {
                    path.LineTo(point);
                }
            }
            return path;
        }

        private void LoadAliens()
        {
            const int AlienCount = 35;
            const int AlienSpacing = 50;

            for (var i = 0; i < AlienCount; i++)
            {
                var alien = SKPath.ParseSvgPathData(Constants.AlienSVG);
                var alienLength =  30;
                var alienScaleX = alienLength / alien.Bounds.Width;
                var alienScaleY = alienLength / alien.Bounds.Height;

                alien.Transform(SKMatrix.CreateScale(alienScaleX, alienScaleY));

                //how many aliens fit into legnth
                //TODO can this be moved outside the loop?
                var scaledAlienLength = (_info.Width - _buttonDiameter) / (alien.Bounds.Width + AlienSpacing);
                var columnCount = Convert.ToInt32(scaledAlienLength - 2);

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
            //IAttributedText attributedText =  MarkdownAttributedTextReader.Read(title);

            //TODO canvas.DrawText(attributedText, _info.Center.X - (textWidth / 2), _info.Center.Y,99,99, _primaryPaint);


            //TODO canvas.DrawPath(_buttonPath, _primaryPaint);
            var width = _secondaryPaint.MeasureText("Play");
            //TODO canvas.DrawText("Play", new SKPoint(buttonCentre.X - (width / 2), buttonCentre.Y + (_secondaryPaint.TextSize / 3)), _secondaryPaint);
        }



        public void Fire(bool isPlayer, SKPoint? startingPosition = null)
        {
            if (isPlayer)
            {
                _bullets.Add(new SKPoint(_jetMidX, _info.Height - _jet.Bounds.Height - _bulletDiameter - 20));
            }
        }

        //There is an issue  with Maui Essentials DisplayInfo so we must manually assign for now
        private int _height = 650;
        private int _width = 800;

        private const float Scale = 0.4f;
        private float _jetMidX;
        private PathF _jet;
        private double _dpi;
        private bool _isGameOver;
        private RectangleF _info;
        private bool _aliensLoaded;
        private int _alienSpeed = 10;
        private int _bulletSpeed = 10;
        private SKPaint _primaryPaint;
        private int _bulletDiameter = 4;
        private SKPaint _secondaryPaint;
        private int _buttonDiameter = 100;
        private bool _aliensSwarmingRight;
        private List<SKPath> _aliens = new List<SKPath>();
        private List<SKPoint> _bullets = new List<SKPoint>();
    }
}
