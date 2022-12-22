using System;
using System.Collections.Generic;
using TL;

namespace Test.DataTypes
{
    public class News
    {
        public News(Message msg)
        {
            Original = msg.message;
        }

        public void Update()
        {
            LastUpdate = DateTime.Now;
        }

        public DateTime LastUpdate { get; private set; } = DateTime.Now;
        public List<(string Name, byte[] Data)> Embeds { get; set; } = new List<(string Name, byte[] Data)>();
        public string Original { get; }
        public string Formatted { get; set; }
    }
}
