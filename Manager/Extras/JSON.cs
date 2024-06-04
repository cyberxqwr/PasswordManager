using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Manager.Extras
{
    internal class JSON
    {

        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize<T>(obj);
        }
        public static T Deserialize<T>(string json)
        {
            
            return JsonSerializer.Deserialize<T>(json);
        }


    }
}
