namespace MauiSpaceInvaders.SpaceInvaders
{
    internal class Bullet
    {
        public PointF Point { get; set; }
        public bool IsPlayer { get; set; }

        public Bullet(PointF point, bool isPlayer)
        {
            Point = point;
            IsPlayer = isPlayer;
        }
    }
}