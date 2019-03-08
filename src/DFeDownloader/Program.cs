using DFeDownloader.Controllers;
using DFeDownloader.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFeDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            #region fazer o console gravar em log
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("./console.log", FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open console.log for writing");
                Console.WriteLine(e.Message);
                return;
            }

            Console.SetOut(writer);
            #endregion

            var appconfig = LoadJson();
            var file = @"./pendentes.txt";
            var list = ReadFile(file);
            var pending = new List<string>();
            ObterDFE.CreateIfMissing(appconfig.XmlFolder);
            foreach (var chvnfe in list)
            {
                if (!ObterDFE.EfetuarDownload(chvnfe: chvnfe,appConfig:appconfig))
                {
                    pending.Add(chvnfe);
                }
            }

            File.WriteAllText(file, String.Empty);
            if (pending.Count > 0)
            {
                var pendentes = list.ToArray();
                File.WriteAllLines(file, pendentes);
            }

            ObterDFE.EfetuarDownload(appConfig: appconfig);

            SaveConfiguration(appconfig);

            #region defaz o console gravar em arquivo
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
            #endregion  
        }
        static List<string> ReadFile(string file)
        {
            var Return = new List<string>();

            if (!File.Exists(file))
            {
                File.Create(file);
            }

            var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    Return.Add(line);
                }
            }
            return Return;
        }
        public static AppConfig LoadJson()
        {
            using (StreamReader r = new StreamReader(@".\config.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
        }
        public static void SaveConfiguration(AppConfig _parametro)
        {

            //open file stream
            using (StreamWriter file = File.CreateText(@".\config.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;

                //serialize object directly into file stream
                serializer.Serialize(file, _parametro);
            }

        }
    }
}
