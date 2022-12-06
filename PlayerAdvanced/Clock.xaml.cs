using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PlayerClock
{
    /// <summary>
    /// Analog wall clock control
    /// </summary>
    public partial class Clock : UserControl
	{
		public Clock()
		{
			InitializeComponent();
			viewbox.DataContext = this;
		}

		// clock position
		public DateTime Value
		{
			get { return (DateTime)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(DateTime), typeof(Clock), new PropertyMetadata((d,e) => ((Clock)d).OnValueChanged(d, e)));
		
		// move needles
		private void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var n = Value.TimeOfDay;
			hour.Angle = n.TotalHours % 12 * 30;
			minute.Angle = n.Minutes * 6;
			var sec = n.TotalSeconds % 60 * 6;
			second.BeginAnimation(RotateTransform.AngleProperty, 
				new DoubleAnimation(sec, sec + Speed * 6, TimeSpan.FromSeconds(1)));
		}

		public double Speed { get; set; } = 1;

		public Stretch Stretch
		{
			get { return (Stretch)GetValue(StretchProperty); }
			set { SetValue(StretchProperty, value); }
		}
		public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(Clock), new PropertyMetadata(Stretch.Uniform));


		// draw background ticks and labels
		private void Grid_Loaded(object sender, EventArgs e)
		{
			for (int m = 0; m < 60; m += 5)
			{
				var bigTick = (UIElement)ticks.FindResource("bigTick");
				bigTick.RenderTransform = new RotateTransform(m * 6);
				ticks.Children.Add(bigTick);
			}
			for (int m = 0; m < 60; m++)
			{
				var smallTick = (UIElement)ticks.FindResource("smallTick");
				smallTick.RenderTransform = new RotateTransform(m * 6);
				ticks.Children.Add(smallTick);
			}
			for (int h = 1; h <= 12; h++)
			{
				var t = new TextBlock { Text = h.ToString(), Width = 25, TextAlignment = TextAlignment.Center };
				Canvas.SetTop(t, 85 - 78 * Math.Cos(h * Math.PI / 6));
				Canvas.SetLeft(t, 88 + 78 * Math.Sin(h * Math.PI / 6));
				labels.Children.Add(t);
			}
			SetCurrentValue(ValueProperty, DateTime.Now);
		}
	}
}
