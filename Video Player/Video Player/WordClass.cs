using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Video_Player
{
    class WordClass
    {
        public WordClass()
        {

        }
        public WordClass(String word)
        {
            this.word = word+" ";
        }
            
        public String word { get; set; }
    }
}
