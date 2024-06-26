﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodePreview
{
    public class Config
    {
        public bool ShowAlways { get; set; } = true;

        public bool ShowStack { get; set; } = true;

        public int Offset { get; set; } = 1;

        public bool ShowMuseumHint { get; set; } = true;

        public bool ShowMysteryboxPreview { get; set; } = true;
    }
}
