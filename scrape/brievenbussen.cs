namespace brievenbussen
{

    public class Rootobject
    {
        public Brievenbus[] list { get; set; }
        public object uirelated { get; set; }
    }

    public class Brievenbus
    {
        public int id { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
        public int locationtype_id { get; set; }
        public string icon_path { get; set; }
        public string location_code { get; set; }
        public string name { get; set; }
        public string verlaagdebus { get; set; }
        public string lichtingstijd { get; set; }
        public string lichtingsdagen { get; set; }
        public string zaterdagopen { get; set; }
        public string na18uuropen { get; set; }
        public string medisch { get; set; }
        public string zondagopen { get; set; }
    }

}