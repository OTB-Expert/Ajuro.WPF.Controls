using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ajuro.WPF.Controls
{
	/// <summary>
	/// MandatoryChanged evant handler
	/// </summary>
	/// <param name="source"></param>
	/// <param name="e"></param>
	public delegate void MandatoryChangedEventHandler(object source, EventArgs e);
	/// <summary>
	/// MandatoryChanged evant handler
	/// </summary>
	/// <param name="source"></param>
	/// <param name="e"></param>
	public delegate void DependencyPropertyChangedEventHandler(DependencyObject source, DependencyPropertyChangedEventArgs e);

	[ContentProperty("MainData")]
	public class AjuroSelector : Control
	{
		/// <summary>
		/// on MandatoryChanged event
		/// </summary>
		public static event DependencyPropertyChangedEventHandler OnMainDataChanged;
		/// <summary>
		/// Error message in red
		/// </summary>
		public static readonly DependencyProperty MainDataProperty;

		static AjuroSelector()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AjuroList), new FrameworkPropertyMetadata(typeof(AjuroList)));
			MainDataProperty = DependencyProperty.Register("MainData", typeof(object), typeof(AjuroList), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, MainDataChanged));
		}

		#region DepenencyProperties

		/// <summary>
		/// Text of the label
		/// </summary>
		[Description("Data for the area presenter"), Category("Common")]
		public virtual object MainData
		{
			get { return GetValue(MainDataProperty); }
			set { SetValue(MainDataProperty, value); }
		}

		private static void MainDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (OnMainDataChanged != null)
			{
				OnMainDataChanged(d, e);
			}
		}
		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}
	}
}
