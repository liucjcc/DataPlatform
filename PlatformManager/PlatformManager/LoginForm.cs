using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace PlatformManager
{
    public partial class LoginForm : Form
    {
        private readonly ApiService _apiService;
        public string? Token { get; private set; }

        public LoginForm(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            txtUsername.Text = "admin";
            txtPassword.Text = "12345";
            lblError.Text = string.Empty;
            btnLogin.Click += btnLogin_Click;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            lblError.Text = string.Empty;
            btnLogin.Enabled = false;

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "用户名或密码不能为空";
                btnLogin.Enabled = true;
                return;
            }

            try
            {
                var result = await _apiService.LoginAsync(username, password);
                if (result != null)
                {
                    // 登录成功
                    _apiService.SetToken(result.AccessToken);  // 更新 ApiService 的 Bearer Token
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblError.Text = "用户名或密码错误";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "登录异常: " + ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
