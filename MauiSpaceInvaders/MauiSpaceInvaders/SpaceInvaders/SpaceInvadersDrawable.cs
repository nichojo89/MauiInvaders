using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;

namespace MauiSpaceInvaders.SpaceInvaders
{
    internal class SpaceInvadersDrawable : View, IDrawable
    {
        public double XAxis { get; set; }
        public bool IsGameOver { get; set; }
        public int AlienFireRate { get; set; }

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
            XAxis = 0.5;
            AlienFireRate = 1;
            ButtonText = Constants.Fire;
        }

        /// <summary>
        /// Fires alien shot
        /// </summary>
        public void AlienFire()
        {
            if (_aliens.Count() == 0)
            {
                return;
            }

            var rdm = new Random();
            var activeShooters = _aliens.TakeLast(_columnCount);
            var shooterIndex = rdm.Next(activeShooters.Count());
            var shooter = activeShooters.ElementAt(shooterIndex);
            var bullet = new Bullet(new SKPoint(shooter.Bounds.MidX, shooter.Bounds.MidY), false);
            Fire(false, bullet);
        }

        public void Draw(ICanvas canvas, RectangleF dirtyRect)
        {
            _info = dirtyRect;

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
                        ? Constants.YouWin
                        : Constants.GameOver);
                    return;
                }
            }

            canvas.ResetState();

            var jet = SKPath.ParseSvgPathData(Constants.JetSVG);

            _jet = ParseSVGPathData(jet.Points);
            
            canvas.StrokeColor = Colors.Green;
            canvas.FillColor = Colors.Green;

            //Calculate the scaling need to fit to screen
            var scaleX = 100 / _jet.Bounds.Width;

            var jetScaleMatrix = System.Numerics.Matrix3x2.CreateScale(Scale);
            _jet.Transform(jetScaleMatrix);

            var jetTranslationMatrix = System.Numerics.Matrix3x2.CreateTranslation((float)(XAxis * (_info.Width - _jet.Bounds.Width)),
                 _info.Height - _jet.Bounds.Height - BulletDiameter);
            
            _jet.Transform(jetTranslationMatrix);

            _jetMidX = _jet.Bounds.Center.X;

            var jetDown = _bullets.Any(b => _jet.Bounds.Contains(b.Point.X, b.Point.Y));
            if (jetDown)
            {
                PresentEndGame(canvas, Constants.GameOver);
                return;
            }

            //Draw the jet
            canvas.FillPath(_jet);

            //Draw bullets
            for (int i = _bullets.Count - 1; i > -1; i--)
            {
                _bullets[i].Point = new SKPoint(_bullets[i].Point.X, _bullets[i].Point.Y + (_bullets[i].IsPlayer ? BulletSpeed * -1 : BulletSpeed));
                canvas.FillCircle(_bullets[i].Point.AsPointF(), BulletDiameter);

                var alienTarged = _aliens.Any(alien => alien.Contains(_bullets[i].Point.X, _bullets[i].Point.Y));
                //Remove any aliens touched by the bullet
                _aliens.RemoveAll(alien => alien.Contains(_bullets[i].Point.X, _bullets[i].Point.Y));
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
                canvas.FillPath(alienPath);
            }

            //Remove bullets that leave screen
            _bullets.RemoveAll(x => x.Point.Y < 0);
        }
        
        /// <summary>
        /// TODO Raise issue to add ParseSVGPathData to PathF in Maui.Graphics
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

        /// <summary>
        /// Loads alien landing coordinates
        /// </summary>
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
                _columnCount = Convert.ToInt32(scaledAlienLength - 2);

                var columnIndex = i % _columnCount;
                var rowIndex = Math.Floor(i / (double)_columnCount);

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
        /// Resets game
        /// </summary>
        private void Reset()
        {
            IsGameOver = false;

            _aliens.Clear();
            _bullets.Clear();

            LoadAliens();
            ButtonText = Constants.Fire;
        }

        /// <summary>
        /// Fires bullet for 
        /// </summary>
        /// <param name="isPlayer"></param>
        /// <param name="startingPosition"></param>
        public void Fire(bool isPlayer, Bullet startingPosition = null)
        {
            if (IsGameOver && isPlayer)
                Reset();
            else
            {
                if (isPlayer)
                {
                    _bullets.Add(new Bullet(new SKPoint(_jetMidX, _info.Height - _jet.Bounds.Height - BulletDiameter - 20), true));
                }
                else
                {
                    _bullets.Add(startingPosition);
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
        private float _jetMidX;
        private int _columnCount;
        private RectangleF _info;
        private string _buttonText;
        private bool _aliensLoaded;
        private bool _aliensSwarmingRight;
        private List<SKPath> _aliens = new List<SKPath>();
        private List<Bullet> _bullets = new List<Bullet>();
    }
}