using SkiaSharp;

namespace MauiSpaceInvaders.SpaceInvaders
{
    internal class Bullet
    {
        public SKPoint Point { get; set; }
        public bool IsPlayer { get; set; }

        public Bullet(SKPoint point, bool isPlayer)
        {
            Point = point;
            IsPlayer = isPlayer;
        }
    }
}