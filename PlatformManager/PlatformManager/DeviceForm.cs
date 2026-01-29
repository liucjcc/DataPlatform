using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlatformManager
{
    public partial class DeviceForm : Form
    {
        private readonly ApiService _apiService;
        public DeviceForm(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        private void DeviceForm_Load(object sender, EventArgs e)
        {

        }
    }
}
