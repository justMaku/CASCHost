﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CASCEdit.Handlers;
using CASCEdit.IO;

namespace CASCEdit.Configs
{
    public class SingleConfig
    {
        public class Column {
            public string Name;
            public string Type;
            public int Length;


            public Column(string name, string type, int length) 
            {
                Name = name;
                Type = type;
                Length = length;
            }

            public static Column withString(string name)
            {
                return new Column(name, "STRING", 0);
            }

            public static Column withHex(string name, int length)
            {
                return new Column(name, "HEX", length);
            }

            override public string ToString() {
                return String.Format("{0}!{1}:{2}", Name, Type, Length);
            }
        }

        private readonly Dictionary<Column, string> Data = new Dictionary<Column, string>();
        private List<string> Lines;
        private string BaseFile;
        private string NewLineChar;

        public string this[string key]
        {
            get {
                var column = Data.Keys.FirstOrDefault(c => c.Name == key);

                if (column == null) {
                    return "";
                }

                return Data[column];
            }
            set
            {
                var column = Data.Keys.FirstOrDefault(c => c.Name == key);

                if (column == null) {
                    return;
                }

                Data[column] = value;
            }
        }

        public SingleConfig(string fileName, Dictionary<Column, string> defaults)
        {
            Data = defaults;
            Lines = new List<string>();
            BaseFile = fileName;
        }

        public SingleConfig(string file, string key, string value)
        {
            BaseFile = file;

			Stream stream;

            if (Uri.IsWellFormedUriString(file, UriKind.Absolute)) // URLs require streaming
            {
                BaseFile = Path.Combine(CASContainer.Settings.OutputPath, Path.GetFileName(file)); // set the correct Output path
				stream = DataHandler.Stream(file);
            }
			else
			{
				stream = new FileStream(BaseFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // get file stream
			}

			if(stream != null)
			{
				using (BinaryReader br = new BinaryReader(stream))
					Parse(br, key, value);

				stream?.Close();
			}
			else
			{
				CASContainer.Logger.LogCritical($"Unable to download or open {file}.");
			}
		}

        public void Write()
        {
            var path = Helper.FixOutputPath(Path.Combine(CASContainer.Settings.OutputPath, Path.GetFileName(BaseFile)), "config");
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(string.Join("|", Data.Keys.Select(c => c.ToString())));

                switch(BaseFile) {
                    case "versions":
                        PopulateLocales(Data.Keys.First(), writer);
                        break;
                    case "cdns":
                        PopulateLocales(Data.Keys.First(), writer);
                        break;
                    default: break;
                }
                
                writer.WriteLine();
                stream.Flush();
            }
        }


        private void Parse(BinaryReader reader, string key, string value)
        {
			// reader.BaseStream.Position = 0;

            // string content = Encoding.UTF8.GetString(reader.ReadBytes((int)reader.BaseStream.Length));

            // NewLineChar = GetLineTerminator(content);
            // Lines = content.Split(new string[] { NewLineChar }, StringSplitOptions.None).ToList();

            // List<string> fields = new List<string>();

            // for (int i = 0; i < Lines.Count; i++)
            // {
            //     // ignore comments and blank
            //     if (string.IsNullOrWhiteSpace(Lines[i]) || Lines[i].StartsWith("#"))
            //         continue;

            //     string[] tokens = Lines[i].Split('|');

            //     if (fields.Count == 0)
            //     {
            //         // get header row
            //         for (int x = 0; x < tokens.Length; x++)
            //             fields.Add(tokens[x].Split('!').First());
            //     }
            //     else
            //     {
            //         // ignore lines not matching the indentifier key value
            //         int keyidx = fields.IndexOf(key);
            //         if (keyidx >= 0 && tokens[keyidx] != value)
            //         {
            //             Lines.RemoveAt(i--);
            //             continue;
            //         }

            //         for (int x = 0; x < tokens.Length; x++)
            //             Data.Add(fields[x], tokens[x]);
            //     }
            // }

            // if (Data.Count == 0)
            //     CASContainer.Logger.LogError($"Invalid config file: {Path.GetFileName(BaseFile)}");
        }

        private string GetLineTerminator(string content)
        {
            if (content.EndsWith("\r\n"))
                return "\r\n";
            else if (content.EndsWith("\r"))
                return "\r";
            else
                return "\n";
        }

        private void PopulateLocales(Column column, TextWriter writer)
        {
            string[] locales = new[] { "eu", "tw", "us", "kr", "cn" };

            for (int i = 0; i < locales.Length; i++)
            {
                Data[column] = locales[i];
                writer.Write(string.Join("|", Data.Values));

                if (i < locales.Length - 1)
                    writer.WriteLine();
            }

        }
    }
}
