using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    public class LogPropertyReader
    {
        public string ReadPath;

        public LogPropertyReader(string filename)
        {
            ReadPath = Path.Combine(Application.streamingAssetsPath, filename);
        }

        public IEnumerator<StringDictionary> GetEnumerator()
        {
            if (!File.Exists(ReadPath))
                yield break;

            StringDictionary propertyInFile = null;
            foreach (string line in File.ReadAllLines(ReadPath).Select(l => l.Trim()))
            {
                if (line == string.Empty || line.StartsWith("//")) continue;

                string[] lineData = line.Split(':');

                if (lineData.Length > 1)
                {
                    string header = lineData[0];
                    string value = lineData[1];

                    if (header == "filename")
                    {
                        if (propertyInFile != null)
                            yield return propertyInFile; //Data collection has finished for the last entry - return the data

                        //Start a new entry for the next set of data fields
                        propertyInFile = new StringDictionary();
                    }

                    if (line.Length > 2) //This shouldn't be a thing, but lets handle it to be safe
                        value = line.Substring(line.IndexOf(":") + 1);

                    //Store each data field in a dictionary until all lines pertaining to the current property are read
                    propertyInFile[header] = value;
                }
            }

            //The loop has finished, but the last entry has not yet been returned
            if (propertyInFile != null)
                yield return propertyInFile;
        }
    }
}
