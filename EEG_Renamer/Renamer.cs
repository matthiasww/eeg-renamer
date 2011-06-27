using System; 
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace RenameEEG
{
	
	
    class Renamer
    {
        string m_pattern = "";
        Dictionary<int, string> m_redefinitions = null;

        public Renamer(String pattern, Dictionary<int, string> redefinitions)
        {
            Pattern = pattern;
            Redefinitions = redefinitions;
        }

        public String Pattern
        {
            get
            {
                return m_pattern;
            }
            set
            {
                m_pattern = value;
            }
        }

        public Dictionary<int, string> Redefinitions
        {
            get
            {
                return m_redefinitions;
            }
            set
            {
                m_redefinitions = value;
            }
        }

        public void Rename(String file)
        {
			int environment = (int)Environment.OSVersion.Platform;
			
			char slash = '\\';
			
			
			if(environment == 4 || environment == 128)
			{
				//running on linux, see http://mono.wikia.com/wiki/Detecting_the_execution_platform
				
				slash = '/';
			}
			
            int lastLocSlash = file.LastIndexOf(slash);
            List<string> fields = new List<string>();
            int numFields = 0;

            String folder = file.Substring(0, lastLocSlash + 1);
            String filename = file.Substring(lastLocSlash + 1);
            String baseFilename = "";

            int lastLocDot = filename.LastIndexOf('.');
            baseFilename = filename.Substring(0, lastLocDot);

            String newBaseName = "";

            Match match;

            match = Regex.Match(filename, Pattern);
            numFields = match.Groups.Count - 1;

            for (int i = 1; i < match.Groups.Count; i++)
            {
                fields.Add(match.Groups[i].Value);
            }

            

            for (int i = 0; i < numFields; i++)
            {
                
                if (Redefinitions.ContainsKey(i))
                {
                    newBaseName = newBaseName + Redefinitions[i];
                }
                else
                {
                    newBaseName = newBaseName + fields[i];
                }

                if(i<numFields - 1)
                    newBaseName = newBaseName + "_";
            }

            Directory.CreateDirectory(folder + "old");
            String oldFolder = folder + "old" + slash;

            MoveFiles(folder, oldFolder, baseFilename);
            CopyFiles(folder, oldFolder, newBaseName, baseFilename);

			#region Change Header file
            StreamReader fileReader = new StreamReader(oldFolder + baseFilename + ".vhdr");
            StreamWriter fileWriter = new StreamWriter(folder + newBaseName + ".vhdr");

            while (!fileReader.EndOfStream)
            {
                String line = fileReader.ReadLine();

                int firstLocEqual = line.IndexOf('=');
                
                if (firstLocEqual >= 0)
                {
                    String def = line.Substring(0, firstLocEqual);
                    //String val = line.Substring(firstLocEqual);

                    if (def == "DataFile")
                    {
                        line = def + "="  + newBaseName + ".eeg";
                    }
                    else if (def == "MarkerFile")
                    {
                        line = def + "=" + newBaseName + ".vmrk";
                    }
                }


                if (fileReader.EndOfStream)
                    fileWriter.Write(line);
                else
                    fileWriter.WriteLine(line);
            }

            fileReader.Close();
            fileWriter.Close();
			#endregion
			
			#region Change event file
			fileReader = new StreamReader(oldFolder + baseFilename + ".vmrk");
			fileWriter = new StreamWriter(folder + newBaseName + ".vmrk");
			
			while(!fileReader.EndOfStream)
			{
				string line = fileReader.ReadLine();
				
				int firstLocEqual = line.IndexOf('=');
				
				if(firstLocEqual >= 0)
				{
					String def = line.Substring(0, firstLocEqual).Trim();
					//String val = line.Substring(firstLocEqual).Trim();
					
					if(def == "DataFile")
					{
						line = def + "=" + newBaseName + ".eeg";
					}
				}
				
				if(fileReader.EndOfStream)
					fileWriter.Write(line);
				else
					fileWriter.WriteLine(line);
			}
			
			
			fileReader.Close();
			fileWriter.Close();
			
			#endregion
        }

        private static void MoveFiles(String sourceFolder, String destFolder, String baseFilename)
        {
            string[] ext = { ".vhdr", ".eeg", ".vmrk" };

            for (int i = 0; i < ext.Length; i++)
            {
                File.Move(sourceFolder + baseFilename + ext[i], destFolder + baseFilename + ext[i]);
            }
        }

        private static void CopyFiles(String folder, String oldFolder, String baseName, String oldBaseName)
        {
            //string[] ext = { ".vhdr", ".eeg", ".vmrk" };
			string[] ext = { ".eeg" };

            for (int i = 0; i < ext.Length; i++)
            {
                File.Copy(oldFolder + oldBaseName + ext[i], folder + baseName + ext[i]);
            }
        }
    }
}
