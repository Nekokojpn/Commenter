using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Commenter
{
    /// <summary>
    /// Interaction logic for input.xaml
    /// </summary>
    public partial class input : Window
    {
        public Action enterAction = null;
        public input()
        {
            InitializeComponent();
        }

        private void inputText_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                enterAction.Invoke();
                this.Close();
            }
        }
    }
}
