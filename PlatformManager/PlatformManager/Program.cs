namespace PlatformManager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var apiService = new ApiService("https://localhost:7136", token: ""); // token 暂时空

            using var loginForm = new LoginForm(apiService);
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // 登录成功，拿到 Token
                var token = loginForm.Token;
                apiService.SetToken(token); // 可在 ApiService 中提供 SetToken 方法更新 Header

                // 打开主窗体
                Application.Run(new MainForm(apiService));
            }
        }
    }
}