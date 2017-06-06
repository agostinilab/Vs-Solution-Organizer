using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vs_Solution_Organizer.Model
{
    public class Solution
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> tags { get; set; }
        public string tecnology { get; set; }
        public DateTime createdOn { get; set; }
        public DateTime lastOpened { get; set; }
        public Technologies MainTechnology { get; set; }
        public StatusOfSolution stauts { get; set; }

        public void PrepareDatesForJsonSerialize()
        {
            if(createdOn == default(DateTime))
            {
                this.createdOn = DateTime.Now.Subtract(TimeSpan.FromDays(100));
                this.lastOpened = createdOn;
            }            
        }
    }

    public enum StatusOfSolution { NotSet, Production, Working, InDevelop, Brokered, Abandoned }

    public enum Technologies { Unknown, WebApp, WebSite, ApiApp, ConsoleApp, WinFormApp, UwpApp, WpfApp, GeneralGuiApp, Driver, TestUnit, IoT }

}
