using Ajuro.WPF.Base.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Ajuro.WPF.Controls
{
	[ContentProperty("EditorDataContext")]
	public partial class AjuroJsonEditorBusiness : System.Windows.ResourceDictionary, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector
	{
		public WizardModel MainViewModel { get; set; }
		public static int IdSequence = 0;
		JObject SampleJson { get; set; }
		public static readonly DependencyProperty EditorDataContextProperty;
		public static event DependencyPropertyChangedEventHandler OnEditorDataContextChanged;
		static AjuroJsonEditorBusiness()
		{
			EditorDataContextProperty = DependencyProperty.Register("EditorDataContext", typeof(object), typeof(AjuroJsonEditor), new FrameworkPropertyMetadata(null, null, null));
			// EditorDataContextProperty = DependencyProperty.Register("EditorDataContext", typeof(object), typeof(AjuroJsonEditorBusiness), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEditorDataContextChanged));
			// EditorDataContextProperty = DependencyProperty.Register("EditorDataContext", typeof(object), typeof(AjuroJsonEditorBusiness), new PropertyMetadata(null));
		}
		private static void EditorDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (OnEditorDataContextChanged != null)
			{
				OnEditorDataContextChanged(d, e);
			}
		}

		private object _editorDataContext;

		#region DepenencyProperties
		[Description("Data for the area presenter"), Category("Common")]
		public virtual object EditorDataContext
		{get;set;
		}
		/*public object EditorDataContext
			{
				get { return (object)GetValue(EditorDataContextProperty); }
				set { SetValue(EditorDataContextProperty, value); }
			}*/
		#endregion

		public AjuroJsonEditorBusiness()
		{

			// InitializeComponent();
			MainViewModel = WizardModel.Instance;
			WizardModel.DataJsonPath = "C:\\OTB\\templates\\profile.json";
			if (!File.Exists(WizardModel.DataJsonPath))
			{
				WizardModel.DataJsonPath = "Resources\\data.json";
			}


			if (File.Exists(WizardModel.DataJsonPath))
			{
				MainViewModel.SampleJson = File.ReadAllText(WizardModel.DataJsonPath);
				SampleJson = JObject.Parse(MainViewModel.SampleJson);
				MainViewModel.SampleTree = UniversalTreeNode.CreateTree(SampleJson);
				MainViewModel.SampleJson = JsonConvert.SerializeObject(SampleJson, Formatting.Indented);
			}

			if (!File.Exists("Resources\\meta2.json"))
			{
				var stream = File.Create("Resources\\meta2.json");
				stream.Close();
				File.WriteAllText("Resources\\meta2.json", "{}");
			}

			if (File.Exists("Resources\\meta2.json"))
			{
				MainViewModel.MetaJson = File.ReadAllText("Resources\\meta2.json");
				var metaJson = JObject.Parse(MainViewModel.MetaJson);
				MainViewModel.MetaJson = JsonConvert.SerializeObject(metaJson, Formatting.Indented);
			}
			var steps = Newtonsoft.Json.JsonConvert.DeserializeObject<WizardStep>(MainViewModel.MetaJson).Children;
			if (steps == null)
			{
				steps = new ObservableCollection<WizardStep>();
			}
			foreach (var step in steps)
			{
				WizardModel.Instance.WizardSteps.Add(step);
			}
			this.EditorDataContext = MainViewModel;
		}

		public void ExpandAll(ItemsControl items, bool expand)
		{
			foreach (object obj in items.Items)
			{
				ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
				if (childControl != null)
				{
					ExpandAll(childControl, expand);
				}
				TreeViewItem item = childControl as TreeViewItem;
				if (item != null)
					item.IsExpanded = true;
			}
		}

		public void StepsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			string editorPath = string.Empty;
			var StepsTree = (TreeView)sender;
			
			if (StepsTree.SelectedItem != null)
			{
				WizardModel.Instance.SelectedStep = (WizardStep)StepsTree.SelectedItem;
				if (WizardModel.Instance.SelectedStep.WizardSections.Count == 0)
				{
					/*
					WizardModel.Instance.SelectedStep.WizardSections.Add(new WizardSection()
					{
						Id = (MainWindow.IdSequence++),
						Name = "Section 1",
						Info = "FB title",
						Type = WizardSection.DataType.Text32,
						XPath= "$.Collections[*].Placeholder"
					});
					*/
				}
				foreach (var step in WizardModel.Instance.SelectedStep.WizardSections)
				{
					if (step.XPath == null)
					{
						// nothing to edit
						continue;
					}
					try
					{
						if (step.CurrentValues == null)
						{
							step.CurrentValues = new ObservableCollection<EditorMeta>();
						}
						step.CurrentValues.Clear();
						List<object> labels = new List<object>();
						var xPaths = SampleJson.SelectTokens(step.XPath);
						editorPath = step.XPath;
						if (step.XLabel == null)
						{
							step.XLabel = step.XPath.IndexOf('.') < 0 ? null : step.XPath.Substring(step.XPath.LastIndexOf('.') + 1);
						}

						if (step.XLabel != null)
						{
							if (step.XLabel.IndexOf('/') > -1)
							{
								step.XLabel = step.XLabel.Substring(step.XLabel.LastIndexOf('/') + 1);
							}
							var xLabels = SampleJson.SelectTokens(step.XLabel);

							foreach (var label in xLabels)
							{
								labels.Add(label);
							}
						}
						else
						{
							labels.Add("Root");
						}

						var stResult = JsonConvert.SerializeObject(xPaths);

						List<object> paths = new List<object>();
						foreach (var result in xPaths)
						{
							paths.Add(result);
						}

						for (int i = 0; i < paths.Count; i++)
						{
							var item = paths[i] as JToken;
							if (item.Type is JTokenType.Object)
							{
								foreach (var property in item)
								{
									// if (property.Children() > 0 && property.Children[0].Type == JTokenType.Array)
									if (true)
									{
										var editor = new EditorMeta()
										{
											IsObject = false,
											ShowLabel = true,
											CurrentLabel = ((JProperty)property).Name.ToString(),
											CurrentPath = editorPath + "." + ((JProperty)property).Name.ToString(),
											CurrentValue = ((JProperty)property).Value.ToString(),
											Index = i
										};
										step.CurrentValues.Add(editor);
										if (editor.CurrentValue.Contains("[") && editor.CurrentValue.Contains("{") && editor.CurrentValue.Length > 200)
										{
											editor.IsObject = true;
										}
									}
									else
									{
										step.CurrentValues.Add(new EditorMeta()
										{
											IsObject = false,
											ShowLabel = true,
											CurrentLabel = ((JProperty)property).Name.ToString(),
											CurrentPath = editorPath,
											CurrentValue = ((JProperty)property).Value.ToString(),
											Index = i
										});
									}
								}
							}
							else
							{
								string label = (step.XLabel != null && labels.Count > 0 ? "" + labels[i % (labels.Count == 0 ? 1 : labels.Count)].ToString() + "" : step.XPath.IndexOf('.') < 0 ? step.XPath : step.XPath.Substring(step.XPath.LastIndexOf('.') + 1));
								step.CurrentValues.Add(new EditorMeta()
								{
									IsObject = false,
									Id = i,
									ShowLabel = true,
									CurrentLabel = label,
									CurrentPath = editorPath.Replace("*", "" + i),// + (label != null ? label : ""),
									CurrentValue = item.ToString(),
									Index = i
								});
							}
						}
					}
					catch (Exception ee)
					{
					}
					try
					{
						JToken acme = SampleJson.SelectToken(step.XPath);
					}
					catch (Exception ee)
					{
					}
				}
				/*


				CurrentValue = val;
				*/
			}
			
		}

		public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			File.WriteAllText("Resources\\meta2.json", Newtonsoft.Json.JsonConvert.SerializeObject(new WizardStep() { Children = WizardModel.Instance.WizardSteps }));
		}

		public void SearchNode_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				string searchText = "";
				//    Todo: string searchText = SearchNode.Text.Trim();

				WizardStep node = null;
				ObservableCollection<WizardStep> items = new ObservableCollection<WizardStep>();

				if (WizardModel.Instance.SelectedStep == null)
				{
					items = WizardModel.Instance.WizardSteps;
				}
				else
				{
					if (WizardModel.Instance.SelectedStep.Children == null)
					{
						WizardModel.Instance.SelectedStep.Children = new ObservableCollection<WizardStep>();
					}
					items = WizardModel.Instance.SelectedStep.Children;
				}

				node = SearchingFor(searchText.ToLower(), items);

				if (node != null)
				{

				}
				else
				{
					var newItem = new WizardStep()
					{
						Name = searchText
					};
					items.Add(newItem);
				}
			}
		}

		public WizardStep SearchingFor(string searchText, ObservableCollection<WizardStep> items)
		{
			foreach (var step in items)
			{
				if (step.Name.ToLower().Contains(searchText))
				{
					return step;
				}
			}
			return null;
		}

		public void Button_Click(object sender, RoutedEventArgs e)
		{

		}

		public void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
		{

		}

		public void EntryTextBox_MouseDown(object sender, MouseButtonEventArgs e)
		{

		}

		public void TextBox_MouseEnter(object sender, MouseEventArgs e)
		{
			var control = (TextBox)sender;
			var name = control.Text;
			var parent = (StackPanel)control.Parent;
			parent = (StackPanel)parent.Parent;
			var labelTextBox = (TextBlock)parent.Children[2];
			var label = labelTextBox.Text;
			var pathTextBox = (TextBox)parent.Children[0];
			var id = -1;
			var idTextBox = (TextBox)parent.Children[1];
			int.TryParse(idTextBox.Text, out id);
			var path = pathTextBox.Text;
			for (int i = 0; i < MainViewModel.SelectedStep.WizardSections.Count; i++)
			{
				if (MainViewModel.SelectedStep.WizardSections[i].XPath == path)
				{
					MainViewModel.SelectedSectionIndex = i;
					MainViewModel.SelectedSection = MainViewModel.SelectedStep.WizardSections[i];
					break;
				}
			}
			for (int i = 0; i < MainViewModel.SelectedSection.CurrentValues.Count; i++)
			{
				if (MainViewModel.SelectedSection.CurrentValues[i].Id == id)
				{
					MainViewModel.SelectedEntryIndex = i;
					MainViewModel.SelectedEntry = MainViewModel.SelectedSection.CurrentValues[i];
					break;
				}
			}
			var node = FindTreeViewNode(MainViewModel.SampleTree, path.Split('.'), MainViewModel.SelectedEntryIndex, label, name);
		}

		public List<UniversalTreeNode> FindTreeViewNode(UniversalTreeNode node, string[] path, int index, string label, string name, int i = 1)
		{
			List<UniversalTreeNode> selectedItem = new List<UniversalTreeNode>();
			foreach (var item in node.Children)
			{
				var pathFragment = path[i].Split('[');
				if (pathFragment.Length > 1)
				{
					pathFragment[1] = pathFragment[1].Substring(0, pathFragment[1].Length - 1);
				}
				var uitem = (UniversalTreeNode)item;
				if (uitem.Name == pathFragment[0])
				{
					selectedItem.Add(uitem);
					uitem.IsExpanded = true;
					if (pathFragment.Length > 1 && pathFragment[1] != "*")
					{
						int idx = -1;

						if (int.TryParse(pathFragment[1], out idx))
						{
							for (var t = 0; t < uitem.Children.Count; t++)
							{
								if (t == idx)
								{
									uitem.Children[t].IsExpanded = true;
									uitem.Children[t].IsSelected = true;
									MainViewModel.SelectedNode = uitem.Children[t];

									if (label != null)
									{
										for (int ii = 0; ii < uitem.Children[t].Children.Count; ii++)
										{
											if (uitem.Children[t].Children[ii].Name == label)
											{
												uitem.Children[t].Children[ii].IsSelected = true;
												MainViewModel.SelectedNode = uitem.Children[t].Children[ii];
											}
										}
									}
								}
								else
								{
									uitem.Children[t].IsExpanded = false;
								}
							}

							if (i < path.Length - 1)
							{
								return FindTreeViewNode(uitem.Children[idx], path, index, label, name, i + 1);
							}
						}
					}

					if (i == path.Length - 2)
					{
						for (var t = 0; t < uitem.Children.Count; t++)
						{
							if (t == index)
							{
								uitem.Children[t].IsExpanded = true;

								for (var p = 0; p < uitem.Children[t].Children.Count; p++)
								{
									if (uitem.Children[t].Children[p].Name == path[path.Length - 1])
									{
										uitem.Children[t].Children[p].IsSelected = true;
										MainViewModel.SelectedNode = uitem.Children[t].Children[p];
									}
								}
							}
							else
							{
								uitem.Children[t].IsExpanded = false;
							}
						}
					}
					else if (path.Length == 2 && label.StartsWith("."))
					{
						uitem.IsSelected = true;
						MainViewModel.SelectedNode = uitem;
					}
				}
				else
				{
					uitem.IsExpanded = false;
				}
			}
			return selectedItem;
		}

		public void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			var control = (TextBox)sender;
			var newValue = control.Text;
			var meta = (EditorMeta)control.DataContext;
			if (newValue != MainViewModel.SelectedNode.Value)
			{
				MainViewModel.SelectedNode.Value = newValue;

				string data = File.ReadAllText(WizardModel.DataJsonPath);
				var o = JObject.Parse(data);
				JToken acme = o.SelectToken(meta.CurrentPath);
				if (acme != null)
				{
					acme.Replace(newValue);
				}
				var str = JsonConvert.SerializeObject(o);
				File.WriteAllText(WizardModel.DataJsonPath, str);
			}
		}

		public void Connect(int connectionId, object target)
		{
			// throw new System.NotImplementedException();
		}
	}
}