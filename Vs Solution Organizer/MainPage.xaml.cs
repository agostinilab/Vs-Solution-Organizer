using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vs_Solution_Organizer.Helpers;
using Vs_Solution_Organizer.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410

namespace Vs_Solution_Organizer
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //private ObservableCollection<string> VisualStudioSolutions = new ObservableCollection<string>();
        private ObservableCollection<Solution> solutions = new ObservableCollection<Solution>();
        private ObservableCollection<Solution> filteredSolutions = new ObservableCollection<Solution>();

        public MainPage()
        {
            this.InitializeComponent();
            SearchForSolutions();
            filteredSolutions = solutions;
            solutionsGrid.ItemsSource = filteredSolutions;            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        public async void SearchForSolutions()
        {
            var configurationStored = await Serializer.LoadConfiguration<ObservableCollection<Solution>>();
            if(configurationStored != null)
            {
                solutions = configurationStored;
                solutionsGrid.ItemsSource = solutions;
            }
            else
            {
                StorageFolder searchFolder;
                if(StorageApplicationPermissions.FutureAccessList.ContainsItem("VsSolutionOrganizerBaseSearchPath"))
                {
                    searchFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("VsSolutionOrganizerBaseSearchPath");
                }
                else                
                {
                    FolderPicker picker = new FolderPicker();
                    picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                    picker.FileTypeFilter.Add(".sln");
                    searchFolder = await picker.PickSingleFolderAsync();
                    if (searchFolder != null)
                    {
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace("VsSolutionOrganizerBaseSearchPath", searchFolder);
                    }
                }
                if (searchFolder != null)
                {
                    foreach (var solutionFolder in await searchFolder.GetFoldersAsync())
                    {                        
                        solutions.Add(new Solution { id = Guid.NewGuid().ToString(), name = solutionFolder.Name, stauts = StatusOfSolution.InDevelop, tags = new List<string>()});
                    }
                }
            }
                        
        }

        private void solutionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void ShowToastNotification(string title, string stringContent, int seconds = 4)
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(stringContent));
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(seconds);
            ToastNotifier.Show(toast);
        }

        private async void solutionsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Solution choice = e.ClickedItem as Solution;
            ShowToastNotification("Item Clicked!", choice.name, 2);
            StorageFolder searchFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("VsSolutionOrganizerBaseSearchPath");
            try
            {
                StorageFolder solutionFolder = await searchFolder.GetFolderAsync(choice.name);
                StorageFile fileToLaunch = await solutionFolder.GetFileAsync(choice.name + ".sln");
                var success = await Windows.System.Launcher.LaunchFileAsync(fileToLaunch);                    
            }
            catch (Exception ex)
            {
                ShowToastNotification("Errore nell'aprire la soluzione....", $"Si è verificata un eccezione del tipo {ex.Message}", 5);
            }
            
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            CheckIfSerializationReady();
            Serializer.SaveConfiguration(solutions);
        }


        private void CheckIfSerializationReady()
        {
            foreach (var item in solutions)
            {
                item.PrepareDatesForJsonSerialize();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = e.OriginalSource as Button;            
            string requestedSolutionName = clickedButton.CommandParameter.ToString();                       
            if(requestedSolutionName == "SearchPanel_ClearFilters")
            {
                filteredSolutions = solutions;
                solutionsGrid.ItemsSource = filteredSolutions;
            }
            else
            {
                Solution solutionToEdit = solutions.Where(s => s.name == requestedSolutionName).FirstOrDefault();
                ShowToastNotification("Richiesto l'Editing!!", "Soluzione richiesta: " + solutionToEdit.name);
                //crudPopUp.Height = Window.Current.Bounds.Height;
                //crudPopUp.IsOpen = true;
                CreateDialog(solutionToEdit);
            }
            
        }

        private async void CreateDialog(Solution solution)
        {
            var dialog = new ContentDialog()
            {
                Title = "Modifica: " + solution.name,
                MaxWidth = this.ActualWidth
            };
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = solution.id });
            panel.Children.Add(new TextBlock { Text = "Modifica gli elementi di seguito per arricchire le informazioni sulla soluzione scelta.", TextWrapping = TextWrapping.Wrap });
            panel.Children.Add(new TextBox { Name = "txtSolutionName", PlaceholderText = "Nome della soluzione", Text =solution.name });

            ComboBox cmBoxStatus = new ComboBox();
            cmBoxStatus.Name = "cmbSolutionStatus";
            var _solutionStatus = Enum.GetValues(typeof(StatusOfSolution)).Cast<StatusOfSolution>();
            cmBoxStatus.ItemsSource = _solutionStatus.ToList();
            cmBoxStatus.SelectedIndex = (int)solution.stauts;            
            panel.Children.Add(cmBoxStatus);

            ComboBox cmBoxTech = new ComboBox();
            cmBoxTech.Name = "cmbSolutionMainTech";
            var _solutionsTechnologies = Enum.GetValues(typeof(Technologies)).Cast<Technologies>();
            cmBoxTech.ItemsSource = _solutionsTechnologies.ToList();
            cmBoxTech.SelectedIndex = (int)solution.MainTechnology;
            panel.Children.Add(cmBoxTech);

            panel.Children.Add(new TextBox { Name = "txtTechnology", PlaceholderText = "Tecnology", Text = solution.tecnology != null ? solution.tecnology : "" });
            string tags = "";
            if (solution.tags != null)
                foreach (var tag in solution.tags)
                {
                    tags += tag + ",";
                }
            panel.Children.Add(new TextBox { Name = "txtTags", PlaceholderText = "tags" });
            dialog.Content = panel;
            dialog.PrimaryButtonText = "Salva";
            dialog.PrimaryButtonClick += delegate 
            {
                solution.name = (panel.FindName("txtSolutionName") as TextBox).Text;
                solution.tecnology = (panel.FindName("txtTechnology") as TextBox).Text;
                string tagsEdited = (panel.FindName("txtTags") as TextBox).Text;
                if(!string.IsNullOrEmpty(tagsEdited))
                {
                    if (solution.tags == null)
                        solution.tags = new List<string>();
                    else
                        solution.tags.Clear();
                    foreach (var item in tagsEdited.Split(','))
                    {
                        solution.tags.Add(item);
                    }
                }
                var comboStatus = panel.FindName("cmbSolutionStatus") as ComboBox;                
                solution.stauts = (StatusOfSolution)Enum.Parse(typeof(StatusOfSolution), comboStatus.SelectedItem.ToString());

                var tech = panel.FindName("cmbSolutionMainTech") as ComboBox;
                solution.MainTechnology = (Technologies)Enum.Parse(typeof(Technologies), tech.SelectedItem.ToString());

                var index = solutions.IndexOf(solution);
                solutions[index] = solution;
                CheckIfSerializationReady();
                Serializer.SaveConfiguration(solutions);
                solutionsGrid.ItemsSource = solutions;
            };
            dialog.SecondaryButtonText = "Annulla";
            dialog.SecondaryButtonClick += delegate { };
            var result = await dialog.ShowAsync();
        }

        private void txtSearchBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if(sender.Text.Length >= 2)
            {
                //var search = from s in solutions where s.name.Contains(sender.Text) || (s.tags != null && s.tags.Contains(sender.Text)) || s.tecnology.Contains(sender.Text) select s;
                //var search = from s in solutions where s.name.ToLowerInvariant().Contains(sender.Text.ToLowerInvariant())  select s;

                filteredSolutions = Searcher(sender.Text);
            }
            else
            {
                filteredSolutions = solutions;
            }
            solutionsGrid.ItemsSource = filteredSolutions;
        }

        private void ResultStatusFilter(StatusOfSolution statusToFilter)
        {
            var list = (from s in solutions where s.stauts == statusToFilter select s).ToList();
            if (list == null || list.Count <= 0)
                return;
            filteredSolutions.Clear();
            foreach (var item in list)
            {
                filteredSolutions.Add(item);
            }
            solutionsGrid.ItemsSource = filteredSolutions;
        }


        private ObservableCollection<Solution> Searcher(string key)
        {
            Solution sol = new Solution();
            
            var searchNames = (from s in solutions where s.name != null && s.name.ToLowerInvariant().Contains(key.ToLowerInvariant()) select s).ToList();
            var solutionsWithTag = (from s in solutions where s.tags != null select s).ToList();
            var solutionsWithTechnology = (from s in solutions where !string.IsNullOrEmpty(s.tecnology) select s).ToList();

            var searchTags = (from s in solutions where s.tags != null && s.tags.Contains(key) select s).ToList();
            var searchTechs = (from s in solutions where s.tecnology != null && s.tecnology.ToLowerInvariant().Contains(key.ToLowerInvariant()) select s).ToList();
            ObservableCollection<Solution> resultSet = new ObservableCollection<Solution>();
            if (searchNames != null || searchTags != null || searchTechs != null)
            {
                if (searchNames != null)
                { 
                    foreach (var item in searchNames)
                    {
                        if (resultSet.Any(s => s.id == item.id) == false)
                            resultSet.Add(item);
                    }
                }

                if(solutionsWithTag != null && solutionsWithTag.Count > 0)
                {
                    foreach (Solution item in solutionsWithTag)
                    {
                        foreach (var tag in item.tags)
                        {
                            if (tag.ToLowerInvariant().Contains(key.ToLowerInvariant()) && !resultSet.Any(s => s.id == item.id))
                                resultSet.Add(item);

                        }
                    }
                }

                if (solutionsWithTechnology != null && solutionsWithTechnology.Count > 0)
                {
                    foreach (var item in solutionsWithTechnology)
                    {
                        if (item.tecnology.ToLowerInvariant().Contains(key.ToLowerInvariant()) && !resultSet.Any(s => s.id == item.id))
                            resultSet.Add(item);
                    }
                }

                return resultSet;
            }
            else
                return null;

        }







        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            SearchForSolutions();
        }

        private void btnSearchPanelToggle_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarToggleButton;
            if (button.CommandParameter.ToString() == "AppBarBtn_ToggleSearchPanel")
            {
                if (searchPanel.Visibility == Visibility.Collapsed)
                {
                    SetPanelVisibility(leggendPanel, btnLeggendPanelToggle, false);
                    SetPanelVisibility(searchPanel, btnSearchPanelToggle, true);                    
                }
                else
                {
                    SetPanelVisibility(searchPanel, btnSearchPanelToggle, false);
                }
            }
            if(button.CommandParameter.ToString() == "AppBarBtn_ToggleLeggendPanel")
            {
                if (leggendPanel.Visibility == Visibility.Collapsed)
                {
                    SetPanelVisibility(searchPanel, btnSearchPanelToggle, false);
                    SetPanelVisibility(leggendPanel, btnLeggendPanelToggle, true);
                }
                else
                    SetPanelVisibility(leggendPanel, btnLeggendPanelToggle, false);
            }
        }

        private void SetPanelVisibility(StackPanel panel, AppBarToggleButton panelBtnControl, bool newVisibility)
        {
            panelBtnControl.IsChecked = newVisibility;
            panel.Visibility = newVisibility ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton link = sender as HyperlinkButton;
            string searchPatternRequested = link.CommandParameter.ToString().Split('-').LastOrDefault();
            if (!string.IsNullOrEmpty(searchPatternRequested))
            {
                if (searchPatternRequested == "ClearFilter")
                {
                    filteredSolutions = solutions;
                    solutionsGrid.ItemsSource = filteredSolutions;
                }
                else
                {
                    StatusOfSolution statusToFilter = (StatusOfSolution)Enum.Parse(typeof(StatusOfSolution), searchPatternRequested);
                    ResultStatusFilter(statusToFilter);
                }
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            if (button.CommandParameter.ToString() == "AppBarBtn_Settings")
            {
                this.Frame.Navigate(typeof(SettingsPage));
            }
        }





        //EOC
    }
}
