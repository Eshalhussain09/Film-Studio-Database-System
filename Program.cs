using System;
using System.Windows.Forms;

namespace FilmStudioWinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Catch unexpected errors gracefully
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show($"Unhandled Error:\n\n{e.Exception.Message}", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            try
            {
                ApplicationConfiguration.Initialize();

                FormWindowState globalState = FormWindowState.Normal;
                Rectangle globalBounds = Rectangle.Empty;

                while (true)
                {
                    DatabaseHelper.ShouldLogout = false;

                    using (var loginForm = new LoginForm())
                    {
                        if (globalBounds != Rectangle.Empty)
                        {
                            loginForm.StartPosition = FormStartPosition.Manual;
                            loginForm.Bounds = globalBounds;
                        }
                        if (globalState == FormWindowState.Maximized)
                            loginForm.WindowState = FormWindowState.Maximized;

                        if (loginForm.ShowDialog() != DialogResult.OK)
                            break; // User closed login form, exit app

                        // Save state from login form
                        if (loginForm.WindowState == FormWindowState.Maximized)
                            globalState = FormWindowState.Maximized;
                        else
                        {
                            globalState = FormWindowState.Normal;
                            globalBounds = loginForm.Bounds;
                        }

                        DatabaseHelper.SetSession(
                            loginForm.LoggedInUserId, 
                            loginForm.LoggedInPartyId ?? 0, 
                            loginForm.LoggedInRole, 
                            loginForm.LoggedInDisplayName
                        );
                    }

                    using (var form1 = new Form1())
                    {
                        if (globalBounds != Rectangle.Empty)
                        {
                            form1.StartPosition = FormStartPosition.Manual;
                            form1.Bounds = globalBounds;
                        }
                        if (globalState == FormWindowState.Maximized)
                            form1.WindowState = FormWindowState.Maximized;

                        Application.Run(form1);

                        // Save state from dashboard form
                        if (form1.WindowState == FormWindowState.Maximized)
                            globalState = FormWindowState.Maximized;
                        else
                        {
                            globalState = FormWindowState.Normal;
                            globalBounds = form1.Bounds;
                        }
                    }

                    if (!DatabaseHelper.ShouldLogout)
                        break; // Exit app if not explicitly logging out
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup Error:\n\n{ex.Message}", "Startup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}