using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234238

namespace Vs_Solution_Organizer
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private Windows.Storage.ApplicationDataContainer localSettings;

        ObservableCollection<Paths> PathList = new ObservableCollection<Paths>();

        public SettingsPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

            if (localSettings == null)
                localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            UpdatePathList();
            



            


        }

        private void UpdatePathList()
        {
            if (localSettings.Values["FutureAccessList_PathIdentifiers"] != null && !string.IsNullOrEmpty(localSettings.Values["FutureAccessList_PathIdentifiers"].ToString()))
            {
                List<string> localPathsIdentifiers = localSettings.Values["FutureAccessList_PathIdentifiers"].ToString().Split(',').ToList();
                foreach (var localPath in localPathsIdentifiers)
                {
                    if (!string.IsNullOrEmpty(localPath))
                        PathList.Add(new Paths { falId = "Percorso individuato con id: " + localPath });                    
                }
            }
            else
            {
                PathList.Add(new Paths { falId = "Nessun percorso ancora salvato...." });
                
            }
            listViewSavedPaths.ItemsSource = PathList;
        }

        private async void AddFolderToMonitor()
        {
            FolderPicker picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".sln");
            var searchFolder = await picker.PickSingleFolderAsync();
            if (searchFolder != null)
            {
                string uniqueNameForFutureAccessList = $"VsSO_{Guid.NewGuid().ToString()}";
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(uniqueNameForFutureAccessList, searchFolder);
                if (localSettings == null)
                    localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values["FutureAccessList_PathIdentifiers"] != null && !string.IsNullOrEmpty(localSettings.Values["FutureAccessList_PathIdentifiers"].ToString()))
                {
                    List<string> listOfUniquePathFinders = localSettings.Values["FutureAccessList_PathIdentifiers"].ToString().Split(',').ToList();
                    if (!listOfUniquePathFinders.Any(s => s.Equals(uniqueNameForFutureAccessList)))
                    {
                        listOfUniquePathFinders.Add(uniqueNameForFutureAccessList);
                    }
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < listOfUniquePathFinders.Count; i++)
                    {
                        builder.Append(listOfUniquePathFinders[i]);
                        if (i < listOfUniquePathFinders.Count - 1)
                            builder.Append(',');
                    }
                    localSettings.Values["FutureAccessList_PathIdentifiers"] = builder.ToString();
                }
                else
                {
                    localSettings.Values["FutureAccessList_PathIdentifiers"] = uniqueNameForFutureAccessList;
                }
            }
        }
        

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //base.OnNavigatedTo(e);
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void btnAddFolderPath_Click(object sender, RoutedEventArgs e)
        {
            AddFolderToMonitor();
        }

    }

    public class Paths
    {
        public string falId { get; set; }
    }
}
