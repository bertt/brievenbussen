using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace brievenbussen
{

    class Program
    {
        static void Main(string[] args)
        {
            var allemaal = new List<Brievenbus>();

            var minx = 3.10;
            var maxx = 7.20;
            var miny = 50.70;
            var maxy = 53.60;

            var step = 0.1;


            for (var x = minx; x <= maxx; x += step)
            {
                for (var y = miny; y <= maxy; y += step)
                {
                    var brievenbussen = GetBrievenbussen(x, y);

                    foreach(var brievenbus in brievenbussen)
                    {
                        var exists = allemaal.Any(bb => bb.id == brievenbus.id);
                        if (!exists)
                        {
                            allemaal.Add(brievenbus);
                        }
                    }
                    Console.WriteLine($"{x},{y}: {brievenbussen.Length}");
                }
            }

            var geojson = new GeojsonFile();
            geojson.type = "FeatureCollection";
            var features = new List<Feature>();
            foreach(var b in allemaal)
            {
                var f = new Feature();
                var g = new Geometry();

                var coords = new float[2];
                coords[0] = b.lng;
                coords[1] = b.lat;
                g.coordinates = coords;
                g.type = "Point";
                f.geometry = g;
                f.type = "Feature";

                var props = new Dictionary<string, object>();
                props.Add("id", b.id);
                props.Add("name", b.name);
                f.properties = props;
                features.Add(f);

            }
            geojson.features = features.ToArray();

            string json = JsonConvert.SerializeObject(geojson, Formatting.Indented);
            File.WriteAllText(@"d:\aaa\brievenbussen.json", json);


            Console.WriteLine("Klaar!");
            Console.WriteLine("Aantal brievenbussen:" + allemaal.Count);
            Console.ReadKey();

        }

        public static Brievenbus[] GetBrievenbussen(double longitude, double latitude)
        {

            var client = new HttpClient();

            var sampleurl = $"https://www.postnl.nl/services/adreskenmerken/api/GetNearestV2?lat={latitude}&lng={longitude}&product=2&productid=2";

            var response = client.GetAsync(sampleurl).Result;

            var brievenbussenJson = response.Content.ReadAsStringAsync().Result;

            var rootobject = JsonConvert.DeserializeObject<Rootobject>(brievenbussenJson);

            return rootobject.list;
        }
    }
}