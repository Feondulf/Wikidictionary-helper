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

            WordBox.PlaceholderText = "Lemma / headword";
            textBox1.PlaceholderText = "Alternative forms, comma-separated";
            textBox2.PlaceholderText = "First etymology term";
            textBox3.PlaceholderText = "Second etymology term";
            textBox4.PlaceholderText = "First term gloss (t1)";
            textBox5.PlaceholderText = "Second term gloss (t2)";
            textBox6.PlaceholderText = "Third etymology term";
            textBox7.PlaceholderText = "Third term gloss (t3)";
            textBox8.PlaceholderText = "Definition / gloss (separate senses with ;)";
            textBox8.MaxLength = 4000;
            bosTitle.PlaceholderText = "Bosworth entry";
            bosNum.PlaceholderText = "BT ref. number";
            OEDTitle.PlaceholderText = "Dictionary of Old English entry";
            OEDnum.PlaceholderText = "DOE ID";
            textInput.Multiline = true;
            textInput.ScrollBars = RichTextBoxScrollBars.Vertical;
            tInput.Multiline = true;
            tInput.ScrollBars = RichTextBoxScrollBars.Vertical;
            titleInput.PlaceholderText = "Source title";
            yearInput.PlaceholderText = "Year";
            new ToolTip().SetToolTip(textInput, "Old English quotation. Line breaks are preserved.");
            new ToolTip().SetToolTip(tInput, "English translation. Use one line per translated line; + adds a translation line.");

            button1.Click += (_, _) => GenerateWikitext();
            button2.Click += (_, _) => ClearForm();
            AddLineButtpn.Click += (_, _) => AddQuoteLine();
            checkBox1.CheckedChanged += (_, _) => textBox1.Enabled = checkBox1.Checked;
            quoteBox.CheckedChanged += (_, _) => SetQuoteInputsEnabled(quoteBox.Checked);
            checkBox2.CheckedChanged += (_, _) =>
            {
                bosTitle.Enabled = checkBox2.Checked;
                bosNum.Enabled = checkBox2.Checked;
            };
            checkBox3.CheckedChanged += (_, _) =>
            {
                OEDTitle.Enabled = checkBox3.Checked;
                OEDnum.Enabled = checkBox3.Checked;
            };

            textBox1.Enabled = checkBox1.Checked;
            SetQuoteInputsEnabled(quoteBox.Checked);
            bosTitle.Enabled = bosNum.Enabled = checkBox2.Checked;
            OEDTitle.Enabled = OEDnum.Enabled = checkBox3.Checked;
        }

        private void SetQuoteInputsEnabled(bool enabled)
        {
            textInput.Enabled = enabled;
            tInput.Enabled = enabled;
            titleInput.Enabled = enabled;
            yearInput.Enabled = enabled;
            AddLineButtpn.Enabled = enabled;
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

            // The add button adds a translation line only; the source quotation remains untouched.
            // Always create the next translation row, including when the field is still empty.
            tInput.AppendText(Environment.NewLine);

            tInput.Focus();
            tInput.SelectionStart = tInput.TextLength;
        }

        private void ClearForm()
        {
            foreach (TextBox box in new[]
            {
                textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7,
                textBox8, WordBox, bosTitle, OEDTitle, bosNum, OEDnum, titleInput, yearInput
            })
            {
                box.Clear();
            }

            textInput.Clear();
            tInput.Clear();
            checkBox1.Checked = quoteBox.Checked = checkBox2.Checked = checkBox3.Checked = false;
            nounRadio.Checked = true;
            comboBox1.Text = "com";
            comboBox2.SelectedItem = "m";
            comboBox3.Text = comboBox4.Text = comboBox5.Text = "";
            WordBox.Focus();
        }

        private void GenerateWikitext()
        {
            string lemma = WordBox.Text.Trim();
            if (lemma.Length == 0)
            {
                MessageBox.Show("Enter the lemma/headword first.", "Missing headword",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                WordBox.Focus();
                return;
            }

            string pos = nounRadio.Checked ? "noun" : verbRadio.Checked ? "verb" : adjRadio.Checked ? "adj" : "adv";
            string section = nounRadio.Checked ? "Noun" : verbRadio.Checked ? "Verb" : adjRadio.Checked ? "Adjective" : "Adverb";
            var output = new System.Text.StringBuilder();
            output.AppendLine("==Old English==").AppendLine();

            if (checkBox1.Checked)
            {
                List<string> alternatives = SplitValues(textBox1.Text);
                if (alternatives.Count > 0)
                {
                    EnsureBlankLine(output);
                    output.AppendLine("===Alternative forms===");
                    output.Append("* {{alt|ang|")
                        .Append(string.Join("|", alternatives.Select(W)))
                        .AppendLine("}}");
                }
            }

            string etymology = comboBox1.Text.Trim();
            string[] terms = { textBox2.Text.Trim(), textBox3.Text.Trim(), textBox6.Text.Trim() };
            string[] glosses = { textBox4.Text.Trim(), textBox5.Text.Trim(), textBox7.Text.Trim() };

            if (terms.Any(s => s.Length > 0) || glosses.Any(s => s.Length > 0))
            {
                EnsureBlankLine(output);
                output.AppendLine("===Etymology===");
                string template = etymology == "af" ? "af" : "com";
                output.Append("From {{").Append(template).Append("|1=ang");

                // Explicit argument numbers preserve the relation between each term and t1/t2/t3.
                for (int i = 0; i < terms.Length; i++)
                {
                    if (terms[i].Length > 0)
                        output.Append('|').Append(i + 2).Append('=').Append(W(terms[i]));
                }
                for (int i = 0; i < glosses.Length; i++)
                {
                    if (glosses[i].Length > 0)
                        output.Append("|t").Append(i + 1).Append('=').Append(W(glosses[i]));
                }
                output.AppendLine("}}.");
            }

            EnsureBlankLine(output);
            output.AppendLine("===Pronunciation===");
            output.Append("* {{ang-IPA|").Append(W(lemma)).Append("|pos=").Append(pos).AppendLine("}}");

            EnsureBlankLine(output);
            output.Append("===").Append(section).AppendLine("===");

            if (nounRadio.Checked)
            {
                string gender = comboBox2.Text.Trim();
                output.Append("{{ang-noun");
                if (gender.Length > 0) output.Append('|').Append(W(gender));
                output.Append("|head=").Append(W(lemma)).Append("}}");
            }
            else
            {
                // Keep the basic POS path adaptable; noun-specific morphology remains the fully
                // supported form while each selection still supplies its own pronunciation pos.
                output.Append("{{ang-").Append(pos).Append('|').Append(W(lemma)).Append("}}");
            }
            output.AppendLine();

            var tags = new[] { comboBox3.Text.Trim(), comboBox4.Text.Trim() }
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<string> senses = SplitSenses(textBox8.Text);
            if (senses.Count == 0) senses.Add("[[" + lemma + "]]");
            foreach (string sense in senses)
            {
                output.Append("#");
                if (tags.Length > 0)
                    output.Append(" {{lb|ang|").Append(string.Join("|", tags.Select(W))).Append("}}");
                output.Append(' ').AppendLine(sense);
            }

            if (quoteBox.Checked && textInput.Text.Trim().Length > 0)
            {
                string title = titleInput.Text.Trim();
                if (title.Length == 0)
                {
                    MessageBox.Show("Enter the source title for the quotation.", "Missing quotation title",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    titleInput.Focus();
                    return;
                }

                string quotedText = FormatQuoteText(textInput.Text, lemma);
                string translations = FormatMultiline(tInput.Text);
                output.Append("#* {{quote-book|ang|title=").Append(W(title));
                if (yearInput.Text.Trim().Length > 0)
                    output.Append("|year=").Append(W(yearInput.Text.Trim()));
                output.Append("|text=").Append(quotedText);
                if (translations.Length > 0) output.Append("|t=").Append(translations);
                output.AppendLine("}}");
            }

            if (nounRadio.Checked && comboBox5.Text.Trim().Length > 0)
            {
                EnsureBlankLine(output);
                output.AppendLine("====Declension====");
                output.Append("{{ang-decl-noun-")
                    .Append(W(comboBox5.Text.Trim())).Append('-')
                    .Append(W(comboBox2.Text.Trim())).Append('|')
                    .Append(W(lemma)).AppendLine("}}");
            }

            if (checkBox2.Checked || checkBox3.Checked)
            {
                EnsureBlankLine(output);
                output.AppendLine("===References===");

                if (checkBox2.Checked)
                {
                    string btEntry = bosTitle.Text.Trim();
                    string btRef = bosNum.Text.Trim();
                    output.Append("* {{R:ang:BT");
                    if (btEntry.Length > 0) output.Append('|').Append(W(btEntry));
                    if (btRef.Length > 0) output.Append("|ref=").Append(W(btRef));
                    output.AppendLine("}}");
                }

                if (checkBox3.Checked)
                {
                    string doeEntry = OEDTitle.Text.Trim();
                    string doeId = OEDnum.Text.Trim();
                    output.Append("* {{R:ang:Dictionary of Old English");
                    if (doeEntry.Length > 0) output.Append("|entry=").Append(W(doeEntry));
                    if (doeId.Length > 0) output.Append("|id=").Append(W(doeId));
                    output.AppendLine("}}");
                }
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

        private static List<string> SplitSenses(string value) =>
            value.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

        private static string FormatQuoteText(string value, string lemma)
        {
            string normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            string[] lines = normalized.Split('\n');
            int count = lines.Length;
            while (count > 0 && lines[count - 1].Trim().Length == 0) count--;
            if (count == 0) return "";

            for (int i = 0; i < count; i++)
            {
                lines[i] = lines[i].Trim();
                if (lines[i].Length > 0) lines[i] = Emphasize(lines[i], lemma);
                lines[i] = W(lines[i]);
            }
            return string.Join("<br />", lines.Take(count));
        }

        private static string FormatMultiline(string value)
        {
            string normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            string[] lines = normalized.Split('\n');
            int count = lines.Length;
            while (count > 0 && lines[count - 1].Trim().Length == 0) count--;
            for (int i = 0; i < count; i++) lines[i] = W(lines[i].Trim());
            return string.Join("<br />", lines.Take(count));
        }

        private static void EnsureBlankLine(System.Text.StringBuilder output)
        {
            string current = output.ToString();
            if (current.Length == 0) return;

            string newline = Environment.NewLine;
            if (current.EndsWith(newline + newline, StringComparison.Ordinal)) return;
            if (!current.EndsWith(newline, StringComparison.Ordinal)) output.AppendLine();
            output.AppendLine();
        }

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
