namespace Poetry_Helper
{
    partial class Form1
    {
        private RadioButton nounRadio = null!;
        private RadioButton verbRadio = null!;
        private RadioButton adjRadio = null!;
        private RadioButton advRadio = null!;

        private RadioButton[] partOfSpeechRadios = null!;

        private void GlobalInit()
        {
            nounRadio = radioButton1;
            verbRadio = radioButton2;
            adjRadio = radioButton3;
            advRadio = radioButton4;

            partOfSpeechRadios = new[]
            {
                nounRadio,
                verbRadio,
                adjRadio,
                advRadio
            };
        }

        private void NamesInit()
        {
            nounRadio.Name = "Noun";
            nounRadio.Text = "Noun";

            verbRadio.Name = "Verb";
            verbRadio.Text = "Verb";

            adjRadio.Name = "Adjective";
            adjRadio.Text = "Adjective";

            advRadio.Name = "Adverb";
            advRadio.Text = "Adverb";

            // AutoSize updates each control's dimensions after its text changes.
            foreach (RadioButton radio in partOfSpeechRadios)
            {
                radio.AutoSize = true;
            }

            CentralizeNames();
        }

        private void CentralizeNames()
        {
            if (partOfSpeechRadios is null || partOfSpeechRadios.Length == 0)
            {
                return;
            }

            const int spacing = 16;
            int totalWidth = partOfSpeechRadios.Sum(radio => radio.Width)
                + spacing * (partOfSpeechRadios.Length - 1);
            int x = Math.Max(0, (ClientSize.Width - totalWidth) / 2);

            foreach (RadioButton radio in partOfSpeechRadios)
            {
                radio.Left = x;
                x += radio.Width + spacing;
            }
        }
    }
}
