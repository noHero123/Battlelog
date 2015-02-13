using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;


namespace Battlelog.Mod
{
    public class Settings
    {


        public bool showNames = true;
        public bool showCoordinates = false;


        public string saveFolder = "";
        public bool alwayssave = false;


        public Settings(string fldr)
        {



            this.saveFolder = fldr;

            Directory.CreateDirectory(this.saveFolder + System.IO.Path.DirectorySeparatorChar);
            string[] aucfiles = Directory.GetFiles(this.saveFolder, "settings.txt");
            if (aucfiles.Contains(this.saveFolder + System.IO.Path.DirectorySeparatorChar + "settings.txt"))//File.Exists() was slower
            {
                loadSettings();
            }
            else
            {
                saveSettings();
            }


        }


        public void saveSettings()
        {
            string txt = "names=" + this.showNames.ToString().ToLower();



            System.IO.File.WriteAllText(this.saveFolder + System.IO.Path.DirectorySeparatorChar + "settings.txt", txt);
        }

        public void loadSettings()
        {
            string text = System.IO.File.ReadAllText(this.saveFolder + System.IO.Path.DirectorySeparatorChar + "settings.txt");
            this.showNames = false;
            if (text.Contains("names=true")) { this.showNames = true; }
        }


    }

}
