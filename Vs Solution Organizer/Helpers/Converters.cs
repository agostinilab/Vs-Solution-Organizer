using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vs_Solution_Organizer.Model;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Vs_Solution_Organizer.Helpers
{
    
    public class MyListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            StringBuilder builder = new StringBuilder();
            if (value != null)
            {
                foreach (var tag in (value as List<string>))
                {
                    builder.Append($"{tag},");
                }
                int indexOfLastChar = builder.Length - 1;
                if (builder[indexOfLastChar] == ',')
                    builder.Remove(indexOfLastChar, 1);
                return builder.ToString();
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SolutionStatusToSolidColorBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                StatusOfSolution status = (StatusOfSolution)Enum.Parse(typeof(StatusOfSolution), value.ToString());
                Color color;
                SolidColorBrush returnBrush;
                switch (status)
                {
                    case StatusOfSolution.NotSet:
                        returnBrush = new SolidColorBrush(Windows.UI.Colors.Orange);
                        color = Colors.Orange;
                        break;
                    case StatusOfSolution.Production:
                        returnBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
                        color = Colors.Blue;
                        break;
                    case StatusOfSolution.Working:
                        returnBrush = new SolidColorBrush(Windows.UI.Colors.Green);
                        color = Colors.Green;
                        break;
                    case StatusOfSolution.InDevelop:
                        returnBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                        color = Colors.Black;
                        break;
                    case StatusOfSolution.Brokered:
                        returnBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                        color = Colors.Red;
                        break;
                    case StatusOfSolution.Abandoned:
                        returnBrush = new SolidColorBrush(Windows.UI.Colors.Purple);
                        color = Colors.Purple;
                        break;
                    default:
                        returnBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
                        color = Colors.Transparent;
                        break;
                }
                return color;
            }
            else
                return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class TechnologyToImagePath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                StringBuilder pathToImage = new StringBuilder();
                pathToImage.Append("Assets/TechIcons/");
                Technologies tech = (Technologies)Enum.Parse(typeof(Technologies), value.ToString());
                switch (tech)
                {
                    case Technologies.Unknown:
                        pathToImage.Append("unknown.png");
                        break;
                    case Technologies.WebApp:
                        pathToImage.Append("webapp.png");
                        break;
                    case Technologies.WebSite:
                        pathToImage.Append("website.png");
                        break;
                    case Technologies.ApiApp:
                        pathToImage.Append("api.png");
                        break;
                    case Technologies.ConsoleApp:
                        pathToImage.Append("consoleapp.png");
                        break;
                    case Technologies.WinFormApp:
                        pathToImage.Append("winform.png");
                        break;
                    case Technologies.UwpApp:
                        pathToImage.Append("uwpapp.png");
                        break;
                    case Technologies.WpfApp:
                        pathToImage.Append("wpfapp.png");
                        break;
                    case Technologies.GeneralGuiApp:
                        pathToImage.Append("generalguiapp.png");
                        break;
                    case Technologies.Driver:
                        pathToImage.Append("driverapp.png");
                        break;
                    case Technologies.TestUnit:
                        pathToImage.Append("testUnit.png");
                        break;
                    case Technologies.IoT:
                        pathToImage.Append("iotapp.png");
                        break;
                    default:
                        break;
                }
                return pathToImage.ToString();
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }




}
