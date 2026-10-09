using System;
using System.Collections.Generic;
using System.Text;

namespace Poetry_Helper
{
    partial class Form1
    {
        RadioButton nounRadio;
        RadioButton verbRadio;
        RadioButton adjRadio;
        RadioButton advRadio;

        public void GlobalInit()
        {
            nounRadio = radioButton1;
            verbRadio = radioButton2;
            adjRadio = radioButton3;
            advRadio = radioButton4;
        }
        public void NamesInit()
        {
            nounRadio.Name = "Noun";
            verbRadio.Name = "Verb";
            adjRadio.Name = "Adjective";
            advRadio.Name = "Adverb";
        }
    }
}
