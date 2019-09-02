using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ajuro.WPF.Controls
{

	/// <summary>
	/// base control for all display controls
	/// </summary>
	[TemplatePart(Name = "PART_ProjectSelector", Type = typeof(StackPanel))]
	[TemplatePart(Name = "PART_ItemsList", Type = typeof(ListView))]
	[ContentProperty("MainData")]
	public class AjuroJsonEditor : Control
	{
		/// <summary>
		/// on MandatoryChanged event
		/// </summary>
		public static event DependencyPropertyChangedEventHandler OnMainDataChanged;
		/// <summary>
		/// Error message in red
		/// </summary>
		public static readonly DependencyProperty MainDataProperty;

		internal const string PartProjectSelectorName = "PART_ProjectSelector";
		internal const string PartItemsListName = "PART_ItemsList";

		private StackPanel _projectSelector;
		private ListView _itemsList;

		public static readonly DependencyProperty ProjectSelectorProperty;
		public static readonly DependencyProperty ItemsListProperty;

		static AjuroJsonEditor()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AjuroJsonEditor), new FrameworkPropertyMetadata(typeof(AjuroJsonEditor)));

			MainDataProperty = DependencyProperty.Register("MainData", typeof(object), typeof(AjuroJsonEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, MainDataChanged));
			ProjectSelectorProperty = DependencyProperty.Register("ProjectSelector", typeof(StackPanel), typeof(AjuroJsonEditor), new PropertyMetadata(null));
			ItemsListProperty = DependencyProperty.Register("ItemsList", typeof(ListView), typeof(AjuroJsonEditor), new PropertyMetadata(null));
		}

		#region DepenencyProperties

		/// <summary>
		/// Start date value
		/// </summary>
		[Description("Project selector"), Category("Common")]
		public DateTime ProjectSelector
		{
			get { return (DateTime)GetValue(ProjectSelectorProperty); }
			set { SetValue(ProjectSelectorProperty, value); }
		}

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

		/// <summary>
		/// End date value
		/// </summary>
		[Description("Items list"), Category("Common")]
		public DateTime ItemsList
		{
			get { return (DateTime)GetValue(ItemsListProperty); }
			set { SetValue(ItemsListProperty, value); }
		}
		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_projectSelector = base.GetTemplateChild(PartProjectSelectorName) as StackPanel;
			_itemsList = base.GetTemplateChild(PartItemsListName) as ListView;

			if (_projectSelector == null || _itemsList == null)
			{
				// throw new NullReferenceException(string.Format("Templates {0} and {1} parts are mandatory for CustomDateRangePicker control.", PartStartDateName, PartEndDateName));
			}
		}
	}
}
