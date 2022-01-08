namespace MauiSpaceInvaders;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        _drawable = new TestDrawable();
        Game = new Test(_drawable);
        Game.Invalidate();
	}

    public class Test : GraphicsView
    {
        public Test(IDrawable drawable)
        {
            Drawable = drawable;
        }
    }

    public class TestDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectangleF dirtyRect)
        {
            //throw new NotImplementedException();
        }
    }

    private IDrawable _drawable;
}

