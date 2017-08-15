using System;
using System.IO;

namespace FinalProject
{
    class LoadSave
    {
        public string loadFile(string fileName)
        {
            string toReturn = "";

            using (StreamReader sr = File.OpenText(fileName))
            {
                toReturn = sr.ReadToEnd();
            }

            return toReturn;
        }

        public void saveFile(string fileName, string info)
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(fileName))
            {
                sw.WriteLine(info);
            }
        }
    }
}
