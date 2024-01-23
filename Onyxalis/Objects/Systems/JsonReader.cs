using MiNET.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Onyxalis.Objects.Systems
{
    internal class JsonReader
    {
        public dynamic LoadJson(string file)
        {
            // https://stackoverflow.com/questions/4796254/relative-path-to-absolute-path-in-c
            string exactPath = Path.GetFullPath(file);

            using (StreamReader r = new StreamReader(exactPath))
            {
                // https://stackoverflow.com/questions/6620165/how-can-i-deserialize-json-with-c
                // https://stackoverflow.com/questions/3142495/deserialize-json-into-c-sharp-dynamic-object/9326146#9326146
                string json = r.ReadToEnd();
                //dynamic stuff = JsonConvert.DeserializeObject(json);
                //Debug.WriteLine((string)stuff.SNOW1);

                JObject jsonObj = JObject.Parse(json);
                Dictionary<string, string> stuff = jsonObj.ToObject<Dictionary<string, string>>();

                return stuff;
            }

        }
    }

}
