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

		/// <summary>
		/// Gets or sets the time displayed by the clock.
		/// Changing this value immediately repositions the hour and minute hands and starts
		/// the animated sweep of the second hand.
		/// </summary>
		public DateTime Value
		{
			get { return (DateTime)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		/// <summary>
		/// Dependency property backing <see cref="Value"/>.
		/// The property-changed callback delegates to <see cref="OnValueChanged"/>.
		/// </summary>
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(DateTime), typeof(Clock), new PropertyMetadata((d,e) => ((Clock)d).OnValueChanged(d, e)));
		
		/// <summary>
		/// Updates the three clock hands whenever <see cref="Value"/> changes.
		/// Hour and minute hands snap to their target angle; the second hand animates
		/// smoothly over one second at the rate set by <see cref="Speed"/>.
		/// </summary>
		private void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var n = Value.TimeOfDay;
			// Each hour occupies 30 degrees (360 / 12); modulo ensures 12-hour display.
			hour.Angle   = n.TotalHours % 12 * 30;
			// Each minute occupies 6 degrees (360 / 60).
			minute.Angle = n.Minutes * 6;
			// Animate the second hand from its current position to the next tick.
			// Speed > 1 makes the sweep faster (e.g. fast-forward playback).
			var sec = n.TotalSeconds % 60 * 6;
			second.BeginAnimation(RotateTransform.AngleProperty, 
				new DoubleAnimation(sec, sec + Speed * 6, TimeSpan.FromSeconds(1)));
		}

		/// <summary>
		/// Gets or sets the playback speed used to scale the second-hand animation duration.
		/// <c>1</c> (default) = real-time; <c>2</c> = double-speed; <c>-1</c> = real-time reverse.
		/// </summary>
		public double Speed { get; set; } = 1;

		/// <summary>
		/// Gets or sets how the clock face is scaled within its container.
		/// Defaults to <see cref="Stretch.Uniform"/> (preserve aspect ratio).
		/// </summary>
		public Stretch Stretch
		{
			get { return (Stretch)GetValue(StretchProperty); }
			set { SetValue(StretchProperty, value); }
		}
		/// <summary>Dependency property backing <see cref="Stretch"/>.</summary>
		public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(Clock), new PropertyMetadata(Stretch.Uniform));


		/// <summary>
		/// Populates the clock face with tick marks and hour labels once the control is loaded,
		/// then initialises the hands to the current wall-clock time.
		/// </summary>
		private void Grid_Loaded(object sender, EventArgs e)
		{
			// Add 12 large ticks at every 5-minute position (every 30 degrees).
			for (int m = 0; m < 60; m += 5)
			{
				var bigTick = (UIElement)ticks.FindResource("bigTick");
				bigTick.RenderTransform = new RotateTransform(m * 6);
				ticks.Children.Add(bigTick);
			}
			// Add 60 small ticks at every minute position (every 6 degrees).
			for (int m = 0; m < 60; m++)
			{
				var smallTick = (UIElement)ticks.FindResource("smallTick");
				smallTick.RenderTransform = new RotateTransform(m * 6);
				ticks.Children.Add(smallTick);
			}
			// Place hour number labels around the dial using polar-to-Cartesian conversion.
			// Radius 78 keeps labels just inside the outer tick ring.
			for (int h = 1; h <= 12; h++)
			{
				var t = new TextBlock { Text = h.ToString(), Width = 25, TextAlignment = TextAlignment.Center };
				Canvas.SetTop(t,  85 - 78 * Math.Cos(h * Math.PI / 6));
				Canvas.SetLeft(t, 88 + 78 * Math.Sin(h * Math.PI / 6));
				labels.Children.Add(t);
			}
			// Initialise the hands to the current time without triggering the second-hand animation.
			SetCurrentValue(ValueProperty, DateTime.Now);
		}
	}
}
