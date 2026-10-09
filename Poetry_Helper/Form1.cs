namespace Poetry_Helper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GlobalInit();
            NamesInit();

            // Keep the group centered if the client area changes size.
            ClientSizeChanged += (_, _) => CentralizeNames();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
