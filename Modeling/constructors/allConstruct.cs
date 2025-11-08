using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kompas6API5;

namespace Modeling.Constructors
{
    internal class allConstruct
    {

        public bool resolutionSupported { get; set; }
        public string machineName { get; set; }
        public string userName { get; set; }
        public int screenWidth { get; set; }
        public int screenHeight { get; set; }

        public string[,] material { get; set; }


        public void setResolutionInfo(bool resolutionSupport, int screenWidth, int screenHeight)
        {
            this.resolutionSupported = resolutionSupport;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
        }

        public void setUserInfo(string machineName, string userName)
        {
            this.userName = userName;
            this.machineName = machineName; 
        }
    }
}
