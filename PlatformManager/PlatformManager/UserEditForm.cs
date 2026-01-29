using PlatformManager.DataModels;
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
    public partial class UserEditForm : Form
    {
        public UserDto User { get; private set; } = new UserDto();
        /// <summary>
        /// 无参构造 → 添加用户
        /// </summary>
        public UserEditForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 带参数构造 → 编辑已有用户
        /// </summary>
        /// <param name="user">要编辑的用户</param>
        public UserEditForm(UserDto user) : this()
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            User = user;

            //txtUsername.Text = user.Username;
            //txtPassword.Text = string.Empty; // 编辑时不显示原密码
            //txtDisplayName.Text = user.DisplayName;
            //cmbRole.SelectedItem = user.Role;
            //chkIsEnabled.Checked = user.IsEnabled;

            //// 如果是编辑，用户名通常不可修改
            //txtUsername.Enabled = false;
        }

        private void UserEditForm_Load(object sender, EventArgs e)
        {

        }
    }
}
