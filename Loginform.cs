using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FilmStudioWinForms
{
    public class LoginForm : Form
    {
        private static readonly Color BgDark    = Color.FromArgb( 15,  23,  42); // slate-900
        private static readonly Color BgCard    = Color.FromArgb( 30,  41,  59); // slate-800
        private static readonly Color BgInput   = Color.FromArgb( 15,  23,  42); // slate-900
        private static readonly Color Accent    = Color.FromArgb( 56, 189, 248); // sky-400
        private static readonly Color BgPanel   = Color.FromArgb( 30,  41,  59); // slate-800
        private static readonly Color TextPri   = Color.FromArgb(248, 250, 252); // slate-50
        private static readonly Color TextSec   = Color.FromArgb(148, 163, 184); // slate-400
        private static readonly Color Success   = Color.FromArgb( 16, 185, 129); // emerald-500
        private static readonly Color Danger    = Color.FromArgb(239,  68,  68); // red-500

        private static readonly Color AdminColor    = Color.FromArgb(239,  68,  68);
        private static readonly Color DirectorColor = Color.FromArgb( 59, 130, 246);
        private static readonly Color ActorColor    = Color.FromArgb(168,  85, 247);
        private static readonly Color CrewColor     = Color.FromArgb(234, 179,   8); // Yellow/Gold

        private string _selectedRole = "Admin";
        public  string LoggedInRole        { get; private set; } = "";
        public  int?   LoggedInPartyId     { get; private set; }
        public  string LoggedInDisplayName { get; private set; } = "";
        public  int    LoggedInUserId      { get; private set; } = 0;

        private Panel   cardPanel      = null!;
        private Button  btnAdmin       = null!;
        private Button  btnDirector    = null!;
        private Button  btnActor       = null!;
        private Button  btnCrew        = null!;
        private Label   roleLabel      = null!;
        private Label   hintLabel      = null!;
        private TextBox txtUsername    = null!;
        private TextBox txtPassword    = null!;
        private Button  btnLogin       = null!;
        private Label   errorLabel     = null!;
        private Label   connStatusLbl  = null!;

        public LoginForm()
        {
            this.Text = "Film Studio — Login";
            // Made size exactly match Form1 to create a seamless transition
            this.Size = new Size(1400, 860);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDark;
            this.AutoScaleMode = AutoScaleMode.Dpi; // Helps with screen scaling
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            this.Load += (s, e) => CenterCard();
            BuildUI();
            CheckConnection();
        }

        private Panel topPanel = null!;
        private void CenterCard()
        {
            if (cardPanel != null && topPanel != null) {
                cardPanel.Location = new Point(
                    (this.ClientSize.Width - cardPanel.Width) / 2, 
                    topPanel.Height + Math.Max(20, (this.ClientSize.Height - topPanel.Height - cardPanel.Height) / 2)
                );
                if (connStatusLbl != null) {
                    connStatusLbl.Location = new Point((this.ClientSize.Width - connStatusLbl.Width) / 2, this.ClientSize.Height - 40);
                }
            }
        }

        private void BuildUI()
        {
            var filmBar = new Panel { Width = 6, Dock = DockStyle.Left, BackColor = Accent };
            this.Controls.Add(filmBar);

            topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, BackColor = BgPanel, Padding = new Padding(30, 20, 30, 20) };
            topPanel.Controls.Add(new Label { Text = "🎬 FilmStudio", Font = new Font("Segoe UI", 26f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 4) });
            topPanel.Controls.Add(new Label { Text = "Management System", Font = new Font("Segoe UI", 12f), ForeColor = TextSec, AutoSize = true, Margin = new Padding(6, 0, 0, 0) });
            this.Controls.Add(topPanel);

            // Make card much wider and taller
            cardPanel = new Panel { Width = 520, Height = 600, BackColor = BgCard };
            // Center the card dynamically
            this.Resize += (s, e) => CenterCard();
            this.Controls.Add(cardPanel);

            int y = 28;
            cardPanel.Controls.Add(new Label { Text = "Sign In", ForeColor = TextPri, Font = new Font("Segoe UI", 15f, FontStyle.Bold), AutoSize = true, Location = new Point(30, y) }); y += 38;
            cardPanel.Controls.Add(new Label { Text = "Select your role to continue", ForeColor = TextSec, AutoSize = true, Location = new Point(30, y) }); y += 36;
            cardPanel.Controls.Add(new Label { Text = "Select Role", ForeColor = TextSec, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Location = new Point(30, y) }); y += 24;

            // Increased height to 100 so buttons can wrap to the next line without hiding
            var rolePanel = new FlowLayoutPanel { Location = new Point(30, y), Size = new Size(420, 100), FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };
            
            btnAdmin    = MakeRoleBtn("⚙ Admin", AdminColor);
            btnDirector = MakeRoleBtn("🎥 Director", DirectorColor);
            btnActor    = MakeRoleBtn("🌟 Actor", ActorColor);
            btnCrew     = MakeRoleBtn("🛠 Crew", CrewColor);

            btnAdmin.Click    += (s, e) => SelectRole("Admin");
            btnDirector.Click += (s, e) => SelectRole("Director");
            btnActor.Click    += (s, e) => SelectRole("Actor");
            btnCrew.Click     += (s, e) => SelectRole("Crew");

            rolePanel.Controls.AddRange(new Control[] { btnAdmin, btnDirector, btnActor, btnCrew });
            cardPanel.Controls.Add(rolePanel); y += 110;

            roleLabel = new Label { Location = new Point(30, y), Size = new Size(420, 22), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            cardPanel.Controls.Add(roleLabel); y += 30;

            hintLabel = new Label { Location = new Point(30, y), Size = new Size(420, 18), ForeColor = TextSec, Font = new Font("Segoe UI", 8f) };
            cardPanel.Controls.Add(hintLabel); y += 34;

            cardPanel.Controls.Add(new Label { Text = "Username", ForeColor = TextSec, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Location = new Point(30, y) }); y += 22;
            txtUsername = new TextBox { Location = new Point(30, y), Size = new Size(420, 38), BackColor = BgInput, ForeColor = TextPri, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f), PlaceholderText = "Enter your username" };
            cardPanel.Controls.Add(txtUsername); y += 48;

            cardPanel.Controls.Add(new Label { Text = "Password", ForeColor = TextSec, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Location = new Point(30, y) }); y += 22;
            txtPassword = new TextBox { Location = new Point(30, y), Size = new Size(420, 38), BackColor = BgInput, ForeColor = TextPri, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f), PasswordChar = '●', PlaceholderText = "Enter your password" };
            cardPanel.Controls.Add(txtPassword); y += 50;

            errorLabel = new Label { Location = new Point(30, y), Size = new Size(420, 22), ForeColor = Danger, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            cardPanel.Controls.Add(errorLabel); y += 30;

            btnLogin = new Button { Text = "Sign In  →", Location = new Point(30, y), Size = new Size(420, 46), FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = TextPri, Font = new Font("Segoe UI", 11f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            cardPanel.Controls.Add(btnLogin);

            connStatusLbl = new Label { Text = "● Checking connection...", ForeColor = TextSec, Font = new Font("Segoe UI", 8f), AutoSize = true, Location = new Point(43, 700) };
            this.Controls.Add(connStatusLbl);
            this.AcceptButton = btnLogin;

            SelectRole("Admin");
        }

        private Button MakeRoleBtn(string text, Color accent)
        {
            var btn = new Button { Text = text, Size = new Size(95, 42), Margin = new Padding(0, 0, 8, 8), FlatStyle = FlatStyle.Flat, ForeColor = TextSec, BackColor = BgInput, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 1; btn.FlatAppearance.BorderColor = Color.FromArgb( 51,  65,  85);
            return btn;
        }

        private void SelectRole(string role)
        {
            _selectedRole = role; errorLabel.Text = "";
            btnAdmin.BackColor = btnDirector.BackColor = btnActor.BackColor = btnCrew.BackColor = BgInput;
            btnAdmin.ForeColor = btnDirector.ForeColor = btnActor.ForeColor = btnCrew.ForeColor = TextSec;

            if (role == "Admin") { btnAdmin.BackColor = Color.FromArgb(40, AdminColor); btnAdmin.ForeColor = roleLabel.ForeColor = AdminColor; roleLabel.Text = "ADMINISTRATOR — Full system access"; hintLabel.Text = "Default: admin / admin123"; }
            else if (role == "Director") { btnDirector.BackColor = Color.FromArgb(40, DirectorColor); btnDirector.ForeColor = roleLabel.ForeColor = DirectorColor; roleLabel.Text = "DIRECTOR — Manage your movies"; hintLabel.Text = "Example: shoaib.mansoor / dir001"; }
            else if (role == "Actor") { btnActor.BackColor = Color.FromArgb(40, ActorColor); btnActor.ForeColor = roleLabel.ForeColor = ActorColor; roleLabel.Text = "ACTOR — View roles & accept offers"; hintLabel.Text = "Example: fawad.khan / act001"; }
            else if (role == "Crew") { btnCrew.BackColor = Color.FromArgb(40, CrewColor); btnCrew.ForeColor = roleLabel.ForeColor = CrewColor; roleLabel.Text = "CREW — View your assignments"; hintLabel.Text = "Example: rana.kamran / crew001"; }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            errorLabel.Text = "";
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text)) { errorLabel.Text = "⚠  Enter username and password."; return; }

            btnLogin.Enabled = false; btnLogin.Text = "Signing in...";
            var (role, partyId, displayName, userId) = DatabaseHelper.ValidateLogin(txtUsername.Text.Trim(), txtPassword.Text.Trim());
            btnLogin.Enabled = true; btnLogin.Text = "Sign In  →";

            if (role == null) { errorLabel.Text = "✖  Invalid credentials."; txtPassword.Clear(); return; }
            if (!role.Equals(_selectedRole, StringComparison.OrdinalIgnoreCase)) { errorLabel.Text = $"✖  Account is not a {_selectedRole}."; return; }

            LoggedInRole = role; LoggedInPartyId = partyId; LoggedInDisplayName = displayName; LoggedInUserId = userId;
            this.DialogResult = DialogResult.OK; this.Close();
        }

        private void CheckConnection()
        {
            if (DatabaseHelper.TestConnection(out _)) { connStatusLbl.Text = "● Connected to Oracle"; connStatusLbl.ForeColor = Success; }
            else { connStatusLbl.Text = "● Database disconnected"; connStatusLbl.ForeColor = Danger; }
        }
    }
}