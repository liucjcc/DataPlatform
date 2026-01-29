using Microsoft.VisualBasic.ApplicationServices;
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
using System.Windows.Forms.Design;

namespace PlatformManager
{
    public partial class UserForm : Form
    {
        private readonly UserManager _userManager;
        private List<UserDto> _users = new List<UserDto>();
        public UserForm(UserManager userManager)
        {
            InitializeComponent();
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));


            dgvUsers.AutoGenerateColumns = false;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.MultiSelect = false;

            // 创建列
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "用户名", DataPropertyName = "Username", Width = 150 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "显示名", DataPropertyName = "DisplayName", Width = 150 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "角色", DataPropertyName = "Role", Width = 100 });
            dgvUsers.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "启用", DataPropertyName = "IsEnabled", Width = 60 });

            btnRefresh.Click += async (s, e) => await LoadUsersAsync();
            btnAdd.Click += async (s, e) => await AddUserAsync();
            btnEdit.Click += async (s, e) => await EditUserAsync();
            btnDelete.Click += async (s, e) => await DeleteUserAsync();

        }

        private void UserForm_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 加载用户列表
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                dgvUsers.DataSource = null;
                _users = await _userManager.GetAllUsersAsync();
                dgvUsers.DataSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载用户失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        private async Task AddUserAsync()
        {
            using var editForm = new UserEditForm(); // 自己创建的用户编辑窗体
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                var user = editForm.User;
                bool ok = await _userManager.AddUserAsync(user);
                if (ok) await LoadUsersAsync();
                else MessageBox.Show("添加用户失败");
            }
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        private async Task EditUserAsync()
        {
            if (dgvUsers.CurrentRow == null) return;
            var selectedUser = dgvUsers.CurrentRow.DataBoundItem as UserDto;
            if (selectedUser == null) return;

            using var editForm = new UserEditForm(selectedUser); // 可以传入原用户数据
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                var updatedUser = editForm.User;
                bool ok = await _userManager.UpdateUserAsync(selectedUser.Username, updatedUser);
                if (ok) await LoadUsersAsync();
                else MessageBox.Show("更新用户失败");
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        private async Task DeleteUserAsync()
        {
            if (dgvUsers.CurrentRow == null) return;
            var selectedUser = dgvUsers.CurrentRow.DataBoundItem as UserDto;
            if (selectedUser == null) return;

            if (MessageBox.Show($"确定删除用户 {selectedUser.Username} ?", "删除确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                bool ok = await _userManager.DeleteUserAsync(selectedUser.Username);
                if (ok) await LoadUsersAsync();
                else MessageBox.Show("删除用户失败");
            }
        }
    }
}
