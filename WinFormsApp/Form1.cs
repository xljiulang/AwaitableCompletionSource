using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            using var source = AwaitableCompletionSource.Create<string>();
            ThreadPool.QueueUserWorkItem(s => ((IAwaitableCompletionSource)s).TrySetResult("AwaitableCompletionSource"), source);
            this.Text = await source.Task;
        }
    }
}
