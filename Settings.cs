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
        public bool showMove = false;
        public bool showsummon = false;
        public int size = 0;

        public string saveFolder = "";
        public bool alwayssave = false;

        public bool openonStart = true;




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
            txt += " showCoordinates=" + this.showCoordinates.ToString().ToLower();
            txt += " showMove=" + this.showMove.ToString().ToLower();
            txt += " showsummon=" + this.showsummon.ToString().ToLower();
            txt += " size=" + this.size +";";
            txt += " openonStart=" + this.openonStart.ToString().ToLower();

            System.IO.File.WriteAllText(this.saveFolder + System.IO.Path.DirectorySeparatorChar + "settings.txt", txt);
        }

        public void loadSettings()
        {
            string text = System.IO.File.ReadAllText(this.saveFolder + System.IO.Path.DirectorySeparatorChar + "settings.txt");
            this.showNames = false;
            if (text.Contains("names=true")) { this.showNames = true; }

            this.showCoordinates = false;
            if (text.Contains("showCoordinates=true")) { this.showCoordinates = true; }

            this.showMove = false;
            if (text.Contains("showMove=true")) { this.showMove = true; }

            this.showsummon = false;
            if (text.Contains("showsummon=true")) { this.showsummon = true; }

            this.size = 0;
            if (text.Contains("size="))
            {
                string sisi = text.Split(new string[] { "size=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                sisi = sisi.Split(';')[0];
                this.size = Convert.ToInt32(sisi);
            }

            this.openonStart = true;
            if (text.Contains("openonStart=false")) { this.openonStart = false; }
        }


        
    }

}
