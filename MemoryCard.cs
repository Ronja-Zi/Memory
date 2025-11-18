using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Memory
{
    public class MemoryCard
    {
        
            public string ImagePath { get; set; }
            public string CoverPath { get; set; }
            public bool IsFlipped { get; set; } = false;
            public bool IsMatched { get; set; } = false;
            public Button Button { get; set; }
        }

    }

