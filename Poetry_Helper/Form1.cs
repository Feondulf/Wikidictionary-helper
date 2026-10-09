namespace Poetry_Helper
{
    public partial class Form1 : Form
    {
        private readonly ToolTip toolTip = new();
        private readonly List<GroupBox> inputGroups = new();

        private TextBox lemmaInput = null!;
        private TextBox alternativesInput = null!;
        private TextBox labelsInput = null!;
        private TextBox definitionsInput = null!;

        private ComboBox etymologyTemplateInput = null!;
        private TextBox[] etymologyTerms = null!;
        private TextBox[] etymologyGlosses = null!;
        private TextBox customEtymologyInput = null!;
        private TextBox additionalEtymologyInput = null!;

        private ComboBox genderInput = null!;
        private ComboBox declensionInput = null!;

        private TextBox quoteInput = null!;
        private TextBox quoteBoldInput = null!;
        private TextBox quoteTitleInput = null!;
        private TextBox quoteYearInput = null!;
        private TextBox translationBoldInput = null!;
        private FlowLayoutPanel translationRows = null!;

        private TextBox derivedTermsInput = null!;
        private TextBox relatedTermsInput = null!;
        private TextBox descendantsInput = null!;

        private TextBox bosworthEntryInput = null!;
        private TextBox bosworthIdInput = null!;
        private TextBox doeEntryInput = null!;
        private TextBox doeIdInput = null!;

        private RichTextBox outputBox = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing) toolTip.Dispose();
            base.Dispose(disposing);
        }

        public Form1()
        {
            Text = "Old English Wiktionary Entry Builder";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 760);
            Size = new Size(1540, 980);
            AutoScaleMode = AutoScaleMode.Font;
            BuildInterface();
        }

        private void BuildInterface()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 700,
                Panel1MinSize = 500,
                Panel2MinSize = 420,
                FixedPanel = System.Windows.Forms.FixedPanel.None
            };
            Controls.Add(split);

            var formScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8)
            };
            split.Panel1.Controls.Add(formScroll);

            var formStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(4)
            };
            formScroll.Controls.Add(formStack);

            var heading = new Label
            {
                Text = "Old English noun entry",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(2, 0, 2, 8)
            };
            formStack.Controls.Add(heading);

            BuildEntrySection(formStack);
            BuildEtymologySection(formStack);
            BuildMorphologySection(formStack);
            BuildQuotationSection(formStack);
            BuildRelatedTermsSection(formStack);
            BuildReferencesSection(formStack);

            var outputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            outputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            outputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            outputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            split.Panel2.Controls.Add(outputLayout);

            outputBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                WordWrap = false,
                DetectUrls = false,
                AcceptsTab = true,
                HideSelection = false
            };
            var outputGroup = new GroupBox
            {
                Text = "Generated wikitext",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            outputGroup.Controls.Add(outputBox);
            outputLayout.Controls.Add(outputGroup, 0, 0);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };
            var generateButton = new Button { Text = "Make wikitext", AutoSize = true };
            generateButton.Click += (_, _) => GeneratePreview();
            var copyButton = new Button { Text = "Copy", AutoSize = true };
            copyButton.Click += (_, _) => CopyOutput();
            var saveButton = new Button { Text = "Save…", AutoSize = true };
            saveButton.Click += (_, _) => SaveOutput();
            var sampleButton = new Button { Text = "Load sample", AutoSize = true };
            sampleButton.Click += (_, _) => LoadSample();
            var clearButton = new Button { Text = "Clear", AutoSize = true };
            clearButton.Click += (_, _) => ClearInputs();
            buttons.Controls.Add(generateButton);
            buttons.Controls.Add(copyButton);
            buttons.Controls.Add(saveButton);
            buttons.Controls.Add(sampleButton);
            buttons.Controls.Add(clearButton);
            outputLayout.Controls.Add(buttons, 0, 1);

            void ResizeGroups()
            {
                int groupWidth = Math.Max(420, formScroll.ClientSize.Width - 38);
                foreach (GroupBox group in inputGroups)
                {
                    group.Width = groupWidth;
                    if (group.Controls.OfType<TableLayoutPanel>().FirstOrDefault() is { } table)
                        table.Width = groupWidth - 20;
                }

                if (translationRows is not null)
                {
                    int width = Math.Max(280, formScroll.ClientSize.Width - 225);
                    translationRows.Width = width;
                    foreach (TextBox line in translationRows.Controls.OfType<TextBox>())
                        line.Width = Math.Max(240, width - 12);
                }
            }

            formScroll.SizeChanged += (_, _) => ResizeGroups();
            split.Panel1.SizeChanged += (_, _) => ResizeGroups();
            ResizeGroups();

            toolTip.SetToolTip(labelsInput, "Optional. Leave empty unless the labels apply to the whole headword.");
            toolTip.SetToolTip(customEtymologyInput,
                "Optional full etymology text. When filled, it replaces the structured template builder above.");
            toolTip.SetToolTip(descendantsInput,
                "One language per line, for example: enm: andweard, aundward, anwerd");
        }

        private void BuildEntrySection(FlowLayoutPanel stack)
        {
            TableLayoutPanel table = AddSection(stack, "Entry and definitions");
            lemmaInput = NewTextBox("Headword, e.g. medudrynċ");
            alternativesInput = NewMultilineTextBox(48, "Alternative forms, one per line or comma-separated");
            labelsInput = NewTextBox("Optional labels, e.g. poetic, hapax");
            definitionsInput = NewMultilineTextBox(82,
                "One definition per line. Wikitext is preserved, e.g. [[mead]]-[[drink]].");

            AddRow(table, "Headword", lemmaInput);
            AddRow(table, "Alternative forms", alternativesInput);
            AddRow(table, "Headword labels", labelsInput);
            AddRow(table, "Definitions", definitionsInput);
        }

        private void BuildEtymologySection(FlowLayoutPanel stack)
        {
            TableLayoutPanel table = AddSection(stack, "Etymology");
            etymologyTemplateInput = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 240
            };
            etymologyTemplateInput.Items.AddRange(new object[] { "com", "af", "Custom text" });
            etymologyTemplateInput.SelectedItem = "com";

            AddRow(table, "Template", etymologyTemplateInput);

            etymologyTerms = new TextBox[3];
            etymologyGlosses = new TextBox[3];
            string[] numbers = { "1st", "2nd", "3rd" };
            for (int i = 0; i < 3; i++)
            {
                etymologyTerms[i] = NewTextBox("Component term");
                etymologyGlosses[i] = NewTextBox("Optional gloss (t" + (i + 1) + ")");
                var row = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    WrapContents = false,
                    FlowDirection = FlowDirection.LeftToRight,
                    Margin = new Padding(0)
                };
                var term = etymologyTerms[i];
                var gloss = etymologyGlosses[i];
                term.Width = 210;
                gloss.Width = 210;
                row.Controls.Add(term);
                row.Controls.Add(gloss);
                AddRow(table, numbers[i] + " component", row);
            }

            customEtymologyInput = NewMultilineTextBox(58,
                "Optional complete etymology text, e.g. From {{inh|ang|gmw-pro|*andaward}}.");
            additionalEtymologyInput = NewMultilineTextBox(48,
                "Optional extra statement(s), e.g. Equivalent to {{af|ang|and-|-weard}}.");
            AddRow(table, "Custom text", customEtymologyInput);
            AddRow(table, "Additional text", additionalEtymologyInput);

            etymologyTemplateInput.SelectedIndexChanged += (_, _) =>
            {
                bool custom = Equals(etymologyTemplateInput.SelectedItem, "Custom text");
                customEtymologyInput.Enabled = custom || customEtymologyInput.TextLength > 0;
            };
            customEtymologyInput.TextChanged += (_, _) =>
            {
                if (customEtymologyInput.TextLength > 0)
                    customEtymologyInput.Enabled = true;
            };
        }

        private void BuildMorphologySection(FlowLayoutPanel stack)
        {
            TableLayoutPanel table = AddSection(stack, "Noun morphology");
            genderInput = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160
            };
            genderInput.Items.AddRange(new object[] { "", "m", "f", "n" });
            genderInput.SelectedIndex = 0;

            declensionInput = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160
            };
            declensionInput.Items.AddRange(new object[] { "", "a", "ō", "i", "nd", "u" });
            declensionInput.SelectedIndex = 0;

            AddRow(table, "Gender", genderInput);
            AddRow(table, "Declension class", declensionInput);
            var note = new Label
            {
                Text = "Pronunciation is generated automatically from the headword with pos=noun.",
                AutoSize = true,
                Margin = new Padding(3, 6, 3, 6)
            };
            AddRow(table, "Pronunciation", note);
        }

        private void BuildQuotationSection(FlowLayoutPanel stack)
        {
            TableLayoutPanel table = AddSection(stack, "Quotation (optional)");
            quoteTitleInput = NewTextBox("The Seafarer");
            quoteYearInput = NewTextBox("10th century");
            quoteInput = NewMultilineTextBox(94,
                "Paste the Old English quotation here. Line breaks become <br />.");
            quoteBoldInput = NewTextBox("Exact phrase to bold in the quotation");
            translationBoldInput = NewTextBox("Exact phrase to bold in the translation");

            var sourceRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };
            quoteTitleInput.Width = 260;
            quoteYearInput.Width = 150;
            sourceRow.Controls.Add(quoteTitleInput);
            sourceRow.Controls.Add(quoteYearInput);

            AddRow(table, "Title / year", sourceRow);
            AddRow(table, "Old English text", quoteInput);
            AddRow(table, "Bold in quotation", quoteBoldInput);

            translationRows = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            AddTranslationRow();

            var translationEditor = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            translationEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            translationEditor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            translationEditor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            translationEditor.Controls.Add(translationRows, 0, 0);

            var addTranslationButton = new Button
            {
                Text = "+ Add translation line",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 5, 0, 2)
            };
            addTranslationButton.Click += (_, _) => AddTranslationRow();
            translationEditor.Controls.Add(addTranslationButton, 0, 1);

            AddRow(table, "Translation lines", translationEditor);
            AddRow(table, "Bold in translation", translationBoldInput);
        }

        private void BuildRelatedTermsSection(FlowLayoutPanel stack)
        {
            TableLayoutPanel table = AddSection(stack, "Related entry sections (all optional)");
            derivedTermsInput = NewMultilineTextBox(52,
                "Derived Old English terms, comma-separated or one per line");
            relatedTermsInput = NewMultilineTextBox(52,
                "Related Old English terms, comma-separated or one per line");
            descendantsInput = NewMultilineTextBox(64,
                "One group per line, e.g. enm: andweard, aundward, anwerd");

            AddRow(table, "Derived terms", derivedTermsInput);
            AddRow(table, "Related terms", relatedTermsInput);
            AddRow(table, "Descendants", descendantsInput);
        }

        private void BuildReferencesSection(FlowLayoutPanel stack)
        {
            TableLayoutPanel table = AddSection(stack, "References (optional)");
            bosworthEntryInput = NewTextBox("Bosworth headword / entry");
            bosworthIdInput = NewTextBox("BT ref, e.g. 22563");
            doeEntryInput = NewTextBox("DOE headword / entry");
            doeIdInput = NewTextBox("DOE ID, e.g. E01263");

            AddRow(table, "Bosworth entry", bosworthEntryInput);
            AddRow(table, "Bosworth ref ID", bosworthIdInput);
            AddRow(table, "DOE entry", doeEntryInput);
            AddRow(table, "DOE ID", doeIdInput);
        }

        private TableLayoutPanel AddSection(FlowLayoutPanel stack, string title)
        {
            var group = new GroupBox
            {
                Text = title,
                Width = 640,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(8),
                Margin = new Padding(2, 2, 2, 10)
            };
            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 0,
                Width = 610,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Padding = new Padding(4),
                Margin = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            group.Controls.Add(table);
            stack.Controls.Add(group);
            inputGroups.Add(group);
            return table;
        }

        private static void AddRow(TableLayoutPanel table, string labelText, Control input)
        {
            int rowIndex = table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(3, 7, 6, 5)
            };

            input.Margin = new Padding(3, 3, 3, 5);
            if (input is TextBox textBox && !textBox.Multiline)
                textBox.Height = 28;
            input.Dock = DockStyle.Fill;

            table.Controls.Add(label, 0, rowIndex);
            table.Controls.Add(input, 1, rowIndex);
        }

        private static TextBox NewTextBox(string placeholder) => new()
        {
            PlaceholderText = placeholder,
            Dock = DockStyle.Fill,
            Margin = new Padding(3, 3, 3, 5)
        };

        private static TextBox NewMultilineTextBox(int height, string placeholder) => new()
        {
            PlaceholderText = placeholder,
            Multiline = true,
            AcceptsReturn = true,
            ScrollBars = ScrollBars.Vertical,
            Height = height,
            Dock = DockStyle.Fill,
            Margin = new Padding(3, 3, 3, 5)
        };

        private void AddTranslationRow(string initialText = "")
        {
            var line = NewTextBox("Translation line " + (translationRows.Controls.Count + 1));
            line.Width = Math.Max(280, translationRows.ClientSize.Width - 12);
            line.Dock = DockStyle.None;
            line.Text = initialText;
            line.Margin = new Padding(0, 2, 0, 3);
            translationRows.Controls.Add(line);
        }

        private void GeneratePreview()
        {
            try
            {
                outputBox.Text = OldEnglishWikitextGenerator.Generate(BuildDraft());
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot generate entry",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private NounEntryDraft BuildDraft() => new()
        {
            Lemma = lemmaInput.Text,
            AlternativeForms = alternativesInput.Text,
            HeadwordLabels = labelsInput.Text,
            Definitions = definitionsInput.Text,
            EtymologyTemplate = Convert.ToString(etymologyTemplateInput.SelectedItem) ?? "com",
            EtymologyTerms = etymologyTerms.Select(box => box.Text).ToArray(),
            EtymologyGlosses = etymologyGlosses.Select(box => box.Text).ToArray(),
            CustomEtymologyText = customEtymologyInput.Text,
            AdditionalEtymologyText = additionalEtymologyInput.Text,
            Gender = Convert.ToString(genderInput.SelectedItem) ?? "",
            DeclensionClass = Convert.ToString(declensionInput.SelectedItem) ?? "",
            QuoteText = quoteInput.Text,
            QuoteBoldPhrase = quoteBoldInput.Text,
            QuoteTitle = quoteTitleInput.Text,
            QuoteYear = quoteYearInput.Text,
            TranslationLines = translationRows.Controls.OfType<TextBox>().Select(box => box.Text).ToList(),
            TranslationBoldPhrase = translationBoldInput.Text,
            DerivedTerms = derivedTermsInput.Text,
            RelatedTerms = relatedTermsInput.Text,
            Descendants = descendantsInput.Text,
            BosworthEntry = bosworthEntryInput.Text,
            BosworthReferenceId = bosworthIdInput.Text,
            DoeEntry = doeEntryInput.Text,
            DoeId = doeIdInput.Text
        };

        private void CopyOutput()
        {
            if (outputBox.TextLength == 0) GeneratePreview();
            if (outputBox.TextLength == 0) return;
            try
            {
                Clipboard.SetText(outputBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not copy to the clipboard: " + ex.Message,
                    "Clipboard error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveOutput()
        {
            if (outputBox.TextLength == 0) GeneratePreview();
            if (outputBox.TextLength == 0) return;

            using var dialog = new SaveFileDialog
            {
                Title = "Save Wiktionary wikitext",
                Filter = "Wikitext / text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = (string.IsNullOrWhiteSpace(lemmaInput.Text) ? "entry" : lemmaInput.Text.Trim()) + ".txt",
                DefaultExt = "txt",
                AddExtension = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                File.WriteAllText(dialog.FileName, outputBox.Text, new System.Text.UTF8Encoding(true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not save the file: " + ex.Message,
                    "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSample()
        {
            lemmaInput.Text = "medudrynċ";
            alternativesInput.Text = "medudrinc" + Environment.NewLine + "medodrinc";
            labelsInput.Text = "poetic, hapax";
            definitionsInput.Text = "[[mead]]-[[drink]]";

            etymologyTemplateInput.SelectedItem = "com";
            etymologyTerms[0].Text = "medu";
            etymologyGlosses[0].Text = "mead";
            etymologyTerms[1].Text = "drynċ";
            etymologyGlosses[1].Text = "drink";
            etymologyTerms[2].Clear();
            etymologyGlosses[2].Clear();
            customEtymologyInput.Clear();
            additionalEtymologyInput.Clear();

            genderInput.SelectedItem = "m";
            declensionInput.SelectedItem = "i";

            quoteInput.Text = "Hwīlum ylfete song dyde iċ mē tō gomene, ganetes hlēoþor ond huilpan swēġ fore hleahtor wera, mǣw singende fore medodrince.";
            quoteBoldInput.Text = "medodrince";
            quoteTitleInput.Text = "The Seafarer";
            quoteYearInput.Text = "10th century";
            translationRows.Controls.Clear();
            AddTranslationRow("Sometimes I made the swan song as a mirth to myself, the gannet’s speech and the hwilp’s noise instead of laugher of men, the singing seagull instead of a mead-drink.");
            translationBoldInput.Text = "a mead-drink";

            derivedTermsInput.Clear();
            relatedTermsInput.Clear();
            descendantsInput.Clear();

            bosworthEntryInput.Text = "medu-drinc";
            bosworthIdInput.Text = "22563";
            doeEntryInput.Text = "and-weardian";
            doeIdInput.Text = "E01263";

            GeneratePreview();
        }

        private void ClearInputs()
        {
            foreach (TextBox box in new[]
            {
                lemmaInput, alternativesInput, labelsInput, definitionsInput, customEtymologyInput,
                additionalEtymologyInput, quoteInput, quoteBoldInput, quoteTitleInput, quoteYearInput,
                translationBoldInput, derivedTermsInput, relatedTermsInput, descendantsInput,
                bosworthEntryInput, bosworthIdInput, doeEntryInput, doeIdInput
            })
            {
                box.Clear();
            }

            foreach (TextBox box in etymologyTerms.Concat(etymologyGlosses)) box.Clear();
            translationRows.Controls.Clear();
            AddTranslationRow();
            etymologyTemplateInput.SelectedItem = "com";
            genderInput.SelectedIndex = 0;
            declensionInput.SelectedIndex = 0;
            outputBox.Clear();
            lemmaInput.Focus();
        }
    }
}
