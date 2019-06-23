﻿using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace FormsRevealer.Sample
{
    [DesignTimeVisible(true)]
    public class ViewRevealer : ContentView
    {
        Grid _rootLayout;
        SKBitmap _childViewImage;
        SKCanvasView _canvasView;
        private float _revealProgress;

        public ViewRevealer()
        {
            // dependency service that creates a bitmap from a XF.View:
            // https://forums.xamarin.com/discussion/75408/is-it-possible-to-create-an-image-from-a-view-in-xamarin-forms
            // https://michaelridland.com/xamarin/creating-native-view-xamarin-forms-viewpage/

            // create an SKBitmap or better with that

            // reveal result in canvas

            // when done hide the canvas and show the childView

            this.BackgroundColor = Color.Transparent;
        }

        public static readonly BindableProperty ChildViewProperty = BindableProperty.Create(
            nameof(ChildView),
            typeof(View),
            typeof(ViewRevealer),
            new Label() { Text = "Set some content :D", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center }
            );

        public View ChildView
        {
            get => (View)GetValue(ChildViewProperty);
            set => SetValue(ChildViewProperty, value);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += Paint;

            _rootLayout = new Grid();

            ChildView.Opacity = 0.0;
            _rootLayout.Children.Add(ChildView);

            Content = _rootLayout;

            //StartRevealAnimation();
        }

        public void StartRevealAnimation()
        {
            _childViewImage = _childViewImage ?? GetChildViewImage();
            _rootLayout.Children.Add(_canvasView);

            var revealAnimation = new Animation(
                interpolatedValue =>
                {
                    _revealProgress = (float)interpolatedValue;
                    _canvasView.InvalidateSurface();
                },
                easing: Easing.CubicInOut
                );

            revealAnimation.Commit(this, "RevealAnimation", length: 900, finished: (d, b) =>
            {
                ChildView.Opacity = 1.0;
                _rootLayout.Children.Remove(_canvasView);
            });
        }

        private void Paint(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = args.Surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            var clippingPath = new SKPath();
            var hypotenuse = Math.Sqrt(Math.Pow(info.Width, 2) + Math.Pow(info.Height, 2));
            var radius = (float)(_revealProgress * hypotenuse);
            clippingPath.AddCircle(info.Width, info.Height, radius);

            canvas.ClipPath(clippingPath);

            canvas.DrawBitmap(_childViewImage, new SKPoint(0.0f, 0.0f));
        }

        private SKBitmap GetChildViewImage()
        {

            var r = DependencyService.Get<IViewImageProvider>(DependencyFetchTarget.GlobalInstance).GetViewImage(ChildView);
            SKCodec codec = SKCodec.Create(new MemoryStream(r));
            return SKBitmap.Decode(codec);

            //string resourceID = "FormsRevealer.Sample.ape.jpg";
            //Assembly assembly = GetType().GetTypeInfo().Assembly;

            //SKBitmap resourceBitmap;
            //using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            //{
            //    resourceBitmap = SKBitmap.Decode(stream);
            //}

            //return resourceBitmap;
        }

    }
}
