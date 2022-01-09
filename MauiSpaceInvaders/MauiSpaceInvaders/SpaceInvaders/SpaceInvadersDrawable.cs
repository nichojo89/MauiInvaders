using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;

namespace MauiSpaceInvaders.SpaceInvaders
{
    internal class SpaceInvadersDrawable : View, IDrawable
    {
        public double XAxis { get; set; }
        public bool IsGameOver { get; set; }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

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
            ButtonText = "Test";
        }

        

        
        public void Draw(ICanvas canvas, RectangleF dirtyRect)
        {
            _info = dirtyRect;

            const string YouWin = "YOU WIN";
            const string GameOver = "GAME OVER";

            if (!_aliensLoaded)
                LoadAliens();

            if (_jet != null)
            {
                //Has an alien hit the ships y axis?
                IsGameOver = _aliens
                    .Select(x => x.Bounds.Bottom)
                    .Any(x => x > _jet.Bounds.Top);

                if (IsGameOver || _aliens.Count == 0)
                {
                    PresentEndGame(canvas, _aliens.Count == 0
                        ? YouWin
                        : GameOver);
                    return;
                }
            }

            //TODO did this work?
            canvas.ResetState();

            var jet = SKPath.ParseSvgPathData(Constants.JetSVG);

            _jet = new PathF();

            _jet = ParseSVGPathData(jet.Points);
            
            //TODO make _primaryPaint
            canvas.StrokeColor = Colors.Green;
            //canvas.FillColor = Colors.Green;

            // calculate the scaling need to fit to screen
            var scaleX = 100 / _jet.Bounds.Width;

            var jetScaleMatrix = System.Numerics.Matrix3x2.CreateScale(Scale);
            _jet.Transform(jetScaleMatrix);

            var jetTranslationMatrix = System.Numerics.Matrix3x2.CreateTranslation((float)(XAxis * (_info.Width - _jet.Bounds.Width)),
                 _info.Height - _jet.Bounds.Height - BulletDiameter);
            
            _jet.Transform(jetTranslationMatrix);

            _jetMidX = _jet.Bounds.Center.X;

            // draw the jet
            canvas.DrawPath(_jet);

            //Draw bullets
            for (int i = _bullets.Count - 1; i > -1; i--)
            {
                _bullets[i] = new SKPoint(_bullets[i].X, _bullets[i].Y - BulletSpeed);
                canvas.DrawCircle(_bullets[i].AsPointF(), BulletDiameter);

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

            //Draw aliens
            for (var i = 0; i < _aliens.Count; i++)
            {
                //Move Aliens
                var alienMatrix = SKMatrix.CreateTranslation(
                _aliensSwarmingRight ? AlienSpeed : AlienSpeed * -1,
                switched ? 50 : 0);

                _aliens[i].Transform(alienMatrix);

                var alienPath = ParseSVGPathData(_aliens[i].Points);
                canvas.DrawPath(alienPath);
            }

            //Remove bullets that leave screen
            _bullets.RemoveAll(x => x.Y < 0);
        }
        
        /// <summary>
        /// TODO Create PR to add ParseSVGPathData to Maui.Graphics
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
                var scaledAlienLength = (_info.Width - ButtonDiameter) / (alien.Bounds.Width + AlienSpacing);
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

        /// <summary>
        /// Presents end game UI
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="title"></param>
        private void PresentEndGame(ICanvas canvas, string title)
        {
            IsGameOver = true;
            canvas.ResetState();

            canvas.FontColor = Colors.White;
            canvas.FontSize = 40;
            canvas.DrawString(title, _info.Center.X, _info.Center.Y, HorizontalAlignment.Center);

            ButtonText = Constants.Play;

        }

        /// <summary>
        /// Fires bullet for 
        /// </summary>
        /// <param name="isPlayer"></param>
        /// <param name="startingPosition"></param>
        public void Fire(bool isPlayer, SKPoint? startingPosition = null)
        {
            if (IsGameOver)
            {
                IsGameOver = false;

                _aliens.Clear();
                _bullets.Clear();
                LoadAliens();
            }
            else {
                if (isPlayer)
                {
                    _bullets.Add(new SKPoint(_jetMidX, _info.Height - _jet.Bounds.Height - BulletDiameter - 20));
                }
            }
        }

        //TODO There is an issue  with Maui Essentials DisplayInfo so we must static assign fake dimensions for now
        private const int Width = 800;
        private const int Height = 1000;
       
        private const float Scale = 0.4f;
        private const int AlienSpeed = 5;
        private const int BulletSpeed = 10;
        private const int BulletDiameter = 4;
        private const int ButtonDiameter = 100;

        private PathF _jet;
        private double _dpi;
        private float _jetMidX;
        private RectangleF _info;
        private string _buttonText;
        private bool _aliensLoaded;
        private SKPaint _primaryPaint;
        private SKPaint _secondaryPaint;
        private bool _aliensSwarmingRight;
        private List<SKPath> _aliens = new List<SKPath>();
        private List<SKPoint> _bullets = new List<SKPoint>();
    }
}