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
            partOfSpeechRadios = new[] { nounRadio, verbRadio, adjRadio, advRadio };

            textBox1.PlaceholderText = "Alternative forms, comma-separated";
            textBox2.PlaceholderText = "First etymology term";
            textBox3.PlaceholderText = "Second etymology term";
            textBox4.PlaceholderText = "First term gloss";
            textBox5.PlaceholderText = "Second term gloss";
            textBox6.PlaceholderText = "Pronunciation spelling (optional)";
            textBox7.PlaceholderText = "Extra IPA parameter (optional)";
            textBox8.PlaceholderText = "Lemma / headword";
            textBox9.PlaceholderText = "Bosworth entry";
            textBox10.PlaceholderText = "Dictionary of Old English entry";
            richTextBox1.Multiline = true;
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            new ToolTip().SetToolTip(richTextBox1, "One quote per line: Old English text | translation | title | year");

            button1.Click += (_, _) => GenerateWikitext();
            button2.Click += (_, _) => ClearForm();
            AddLineButtpn.Click += (_, _) => AddQuoteLine();
            checkBox1.CheckedChanged += (_, _) => textBox1.Enabled = checkBox1.Checked;
            quoteBox.CheckedChanged += (_, _) => richTextBox1.Enabled = quoteBox.Checked;
            checkBox2.CheckedChanged += (_, _) => textBox9.Enabled = checkBox2.Checked;
            checkBox3.CheckedChanged += (_, _) => textBox10.Enabled = checkBox3.Checked;
            textBox1.Enabled = checkBox1.Checked;
            richTextBox1.Enabled = quoteBox.Checked;
            textBox9.Enabled = checkBox2.Checked;
            textBox10.Enabled = checkBox3.Checked;
        }

        private void NamesInit()
        {
            nounRadio.Name = "Noun"; nounRadio.Text = "Noun";
            verbRadio.Name = "Verb"; verbRadio.Text = "Verb";
            adjRadio.Name = "Adjective"; adjRadio.Text = "Adjective";
            advRadio.Name = "Adverb"; advRadio.Text = "Adverb";
            foreach (RadioButton radio in partOfSpeechRadios) radio.AutoSize = true;
            nounRadio.Checked = true;
            comboBox1.Text = "com";
            comboBox2.SelectedItem = "m";
            comboBox3.Text = "poetic";
            comboBox4.Text = "hapax";
            CentralizeNames();
        }

        private void CentralizeNames()
        {
            if (partOfSpeechRadios is null || partOfSpeechRadios.Length == 0) return;
            const int spacing = 16;
            int width = partOfSpeechRadios.Sum(r => r.Width) + spacing * (partOfSpeechRadios.Length - 1);
            int x = Math.Max(0, (ClientSize.Width - width) / 2);
            foreach (RadioButton radio in partOfSpeechRadios) { radio.Left = x; x += radio.Width + spacing; }
        }

        private void AddQuoteLine()
        {
            if (!quoteBox.Checked) quoteBox.Checked = true;
            if (richTextBox1.TextLength > 0 && !richTextBox1.Text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.Focus();
            richTextBox1.SelectionStart = richTextBox1.TextLength;
        }

        private void ClearForm()
        {
            foreach (TextBox box in new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10 }) box.Clear();
            richTextBox1.Clear();
            checkBox1.Checked = quoteBox.Checked = checkBox2.Checked = checkBox3.Checked = false;
            nounRadio.Checked = true;
            comboBox1.Text = "com"; comboBox2.SelectedItem = "m";
            comboBox3.Text = comboBox4.Text = comboBox5.Text = "";
            textBox8.Focus();
        }

        private void GenerateWikitext()
        {
            string lemma = textBox8.Text.Trim();
            if (lemma.Length == 0)
            {
                MessageBox.Show("Enter the lemma/headword first.", "Missing headword", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox8.Focus(); return;
            }

            string pos = nounRadio.Checked ? "noun" : verbRadio.Checked ? "verb" : adjRadio.Checked ? "adj" : "adv";
            string section = nounRadio.Checked ? "Noun" : verbRadio.Checked ? "Verb" : adjRadio.Checked ? "Adjective" : "Adverb";
            var output = new System.Text.StringBuilder();
            output.AppendLine("==Old English==").AppendLine();

            if (checkBox1.Checked && textBox1.Text.Trim().Length > 0)
            {
                output.AppendLine("===Alternative forms===");
                foreach (string alt in SplitValues(textBox1.Text)) output.AppendLine("* {{alt|ang|" + W(alt) + "}}");
                output.AppendLine();
            }

            string etymology = comboBox1.Text.Trim();
            string first = textBox2.Text.Trim(), second = textBox3.Text.Trim();
            if (first.Length > 0 || second.Length > 0)
            {
                output.AppendLine("===Etymology===");
                if (etymology == "com" && first.Length > 0 && second.Length > 0)
                {
                    output.Append("From {{com|ang|").Append(W(first));
                    if (textBox4.Text.Trim().Length > 0) output.Append("|t1=").Append(W(textBox4.Text.Trim()));
                    output.Append('|').Append(W(second));
                    if (textBox5.Text.Trim().Length > 0) output.Append("|t2=").Append(W(textBox5.Text.Trim()));
                    output.AppendLine("}}.");
                }
                else
                {
                    output.Append("From {{").Append(etymology.Length > 0 ? W(etymology) : "inh+").Append("|ang");
                    if (first.Length > 0) output.Append('|').Append(W(first));
                    if (second.Length > 0) output.Append('|').Append(W(second));
                    output.AppendLine("}}.");
                }
                output.AppendLine();
            }

            output.AppendLine("===Pronunciation===");
            output.Append("* {{ang-IPA|").Append(W(textBox6.Text.Trim().Length > 0 ? textBox6.Text.Trim() : lemma)).Append("|pos=").Append(pos);
            if (textBox7.Text.Trim().Length > 0) output.Append('|').Append(W(textBox7.Text.Trim()));
            output.AppendLine("}}").AppendLine();

            output.Append("===").Append(section).AppendLine("===");
            if (nounRadio.Checked)
                output.Append("{{ang-noun|").Append(W(lemma)).Append(comboBox2.Text.Trim().Length > 0 ? "|" + W(comboBox2.Text.Trim()) : "").Append("}}");
            else
                output.Append("{{ang-").Append(pos).Append('|').Append(W(lemma)).Append("}}");

            var tags = new[] { comboBox3.Text.Trim(), comboBox4.Text.Trim() }.Where(s => s.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (tags.Length > 0) output.Append(" {{tlb|ang|").Append(string.Join("|", tags.Select(W))).Append("}}");
            output.AppendLine().AppendLine("# [[").Append(lemma).AppendLine("]]");

            if (quoteBox.Checked)
            {
                foreach (string line in richTextBox1.Lines.Select(s => s.Trim()).Where(s => s.Length > 0))
                {
                    string[] f = line.Split('|').Select(s => s.Trim()).ToArray();
                    string quoteText = f.ElementAtOrDefault(0) ?? "";
                    string translation = f.ElementAtOrDefault(1) ?? "";
                    string boldQuote = f.ElementAtOrDefault(4) ?? "";
                    string boldTranslation = f.ElementAtOrDefault(5) ?? "";
                    if (boldQuote.Length > 0) quoteText = Emphasize(quoteText, boldQuote);
                    if (boldTranslation.Length > 0) translation = Emphasize(translation, boldTranslation);
                    output.Append("#* {{quote-book|ang|text=''").Append(quoteText).Append("''");
                    if (translation.Length > 0) output.Append("|t=").Append(W(translation));
                    if (f.Length > 2 && f[2].Length > 0) output.Append("|title=").Append(W(f[2]));
                    if (f.Length > 3 && f[3].Length > 0) output.Append("|year=").Append(W(f[3]));
                    output.AppendLine("}}");
                }
                output.AppendLine();
            }

            if (nounRadio.Checked && comboBox5.Text.Trim().Length > 0)
                output.AppendLine("====Declension====").Append("{{ang-decl-noun-").Append(W(comboBox5.Text.Trim())).Append('-').Append(W(comboBox2.Text.Trim())).Append('|').Append(W(lemma)).AppendLine("}}").AppendLine();

            if (checkBox2.Checked || checkBox3.Checked)
            {
                output.AppendLine("===References===");
                if (checkBox2.Checked && textBox9.Text.Trim().Length > 0) output.AppendLine("* {{R:ang:BT|" + W(textBox9.Text.Trim()) + "}}");
                if (checkBox3.Checked && textBox10.Text.Trim().Length > 0) output.AppendLine("* {{R:ang:Dictionary of Old English|" + W(textBox10.Text.Trim()) + "}}");
            }

            ShowGeneratedWikitext(output.ToString(), lemma);
        }

        private void ShowGeneratedWikitext(string wikitext, string lemma)
        {
            using Form preview = new()
            {
                Text = "Generated Wiktionary wikitext",
                StartPosition = FormStartPosition.CenterParent,
                Width = 900,
                Height = 700,
                MinimizeBox = false
            };
            var text = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10),
                WordWrap = false,
                DetectUrls = false,
                ReadOnly = false,
                Text = wikitext
            };
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(6)
            };
            var close = new Button { Text = "Close", AutoSize = true, DialogResult = DialogResult.Cancel };
            var save = new Button { Text = "Save…", AutoSize = true };
            var copy = new Button { Text = "Copy", AutoSize = true };
            copy.Click += (_, _) =>
            {
                Clipboard.SetText(text.Text);
                MessageBox.Show(preview, "Wikitext copied to the clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            save.Click += (_, _) =>
            {
                using var dialog = new SaveFileDialog
                {
                    Title = "Save Wiktionary wikitext",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = lemma + ".txt",
                    DefaultExt = "txt",
                    AddExtension = true
                };
                if (dialog.ShowDialog(preview) == DialogResult.OK)
                    System.IO.File.WriteAllText(dialog.FileName, text.Text, new System.Text.UTF8Encoding(true));
            };
            buttons.Controls.Add(close);
            buttons.Controls.Add(save);
            buttons.Controls.Add(copy);
            preview.Controls.Add(text);
            preview.Controls.Add(buttons);
            preview.CancelButton = close;
            preview.ShowDialog(this);
        }

        private static List<string> SplitValues(string value) =>
            value.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        private static string Emphasize(string text, string phrase)
        {
            int index = text.IndexOf(phrase, StringComparison.Ordinal);
            if (index < 0) return text;
            return text.Substring(0, index) + "'''"
                + text.Substring(index, phrase.Length) + "'''"
                + text.Substring(index + phrase.Length);
        }

        private static string W(string value) => value.Replace("|", "{{!}}", StringComparison.Ordinal);
    }
}
