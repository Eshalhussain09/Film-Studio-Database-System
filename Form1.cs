using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace FilmStudioWinForms
{
    public partial class Form1 : Form
    {
        // ── UI Palette ──────────────────────────────────────────────────
        private static readonly Color BgDark   = Color.FromArgb( 15,  23,  42); // slate-900
        private static readonly Color BgPanel  = Color.FromArgb( 30,  41,  59); // slate-800
        private static readonly Color BgCard   = Color.FromArgb( 30,  41,  59); // slate-800
        private static readonly Color BgInput  = Color.FromArgb( 15,  23,  42); // slate-900
        private static readonly Color Accent   = Color.FromArgb( 56, 189, 248); // sky-400
        private static readonly Color TextPri  = Color.FromArgb(248, 250, 252); // slate-50
        private static readonly Color TextSec  = Color.FromArgb(148, 163, 184); // slate-400
        private static readonly Color Success  = Color.FromArgb( 16, 185, 129); // emerald-500
        private static readonly Color Danger   = Color.FromArgb(239,  68,  68); // red-500
        private static readonly Color GridLine = Color.FromArgb( 51,  65,  85); // slate-700
        private static readonly Color HeaderBg = Color.FromArgb( 15,  23,  42); // slate-900

        // ── NavItem model ────────────────────────────────────────────────
        private class NavItem
        {
            public string Label      { get; set; } = "";
            public string Icon       { get; set; } = "";
            // Full SELECT used for display (with joins for readable names)
            public string SelectSql  { get; set; } = "";
            // The base table for INSERT/UPDATE/DELETE (empty = read-only view)
            public string TableName  { get; set; } = "";
            // Whether admin/director can add rows to this nav item
            public bool   CanAdd     { get; set; } = false;
            // Whether admin/director can delete from this nav item
            public bool   CanDelete  { get; set; } = true;
        }

        // ── Controls ─────────────────────────────────────────────────────
        private Panel           sidebarPanel      = null!;
        private Panel           topBar            = null!;
        private Panel           contentPanel      = null!;
        private FlowLayoutPanel statsBar          = null!;
        private DataGridView    dataGrid          = null!;
        private Label           tableTitle        = null!;
        private Label           headerUserLabel   = null!;
        private Button          btnRefresh        = null!;
        private Button          btnSave           = null!;
        private Button          btnAdd            = null!;
        private Button          btnDelete         = null!;
        private Button          btnAcceptRole     = null!;
        private Button          btnEditProfile    = null!;

        private List<NavItem> navItems  = new();
        private Button[]      navButtons = null!;
        private int           selectedNavIndex = 0;

        // ── Data state ───────────────────────────────────────────────────
        private OracleDataAdapter _adapter  = null!;
        private DataTable         _dataTable = null!;

        public Form1()
        {
            InitializeComponent();
            BuildNavItems();
            BuildUI();
            this.Shown += (s, e) => { if (navItems.Count > 0) LoadData(0); };
        }

        // ════════════════════════════════════════════════════════════════
        // NAV ITEMS — role-specific, with full JOIN queries for display
        // ════════════════════════════════════════════════════════════════
        private void BuildNavItems()
        {
            string role = (DatabaseHelper.CurrentRole ?? "").Trim();
            int    pid  = DatabaseHelper.CurrentPartyId;

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                navItems.Add(new NavItem
                {
                    Label = "Movies", Icon = "🎬", TableName = "movie", CanAdd = true,
                    SelectSql = @"SELECT m.movie_id       AS ""Movie ID"",
                                         m.title          AS ""Title"",
                                         m.genre          AS ""Genre"",
                                         m.release_year   AS ""Year"",
                                         m.rating         AS ""Rating"",
                                         m.status         AS ""Status"",
                                         TO_CHAR(m.budget,'FM999,999,999') AS ""Budget (PKR)"",
                                         d.full_name      AS ""Director""
                                  FROM movie m
                                  JOIN director d ON m.director_id = d.director_id
                                  ORDER BY m.movie_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Directors", Icon = "🎥", TableName = "director", CanAdd = true,
                    SelectSql = @"SELECT director_id         AS ""Director ID"",
                                         full_name           AS ""Full Name"",
                                         TO_CHAR(dob,'DD-Mon-YYYY') AS ""Date of Birth"",
                                         nationality         AS ""Nationality"",
                                         biography           AS ""Biography""
                                  FROM director
                                  ORDER BY director_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Actors", Icon = "🌟", TableName = "actor", CanAdd = true,
                    SelectSql = @"SELECT actor_id            AS ""Actor ID"",
                                         full_name           AS ""Full Name"",
                                         TO_CHAR(dob,'DD-Mon-YYYY') AS ""Date of Birth"",
                                         nationality         AS ""Nationality"",
                                         contact             AS ""Contact Email""
                                  FROM actor
                                  ORDER BY actor_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Crew Members", Icon = "🎞️", TableName = "crew_member", CanAdd = true,
                    SelectSql = @"SELECT crew_id             AS ""Crew ID"",
                                         full_name           AS ""Full Name"",
                                         department          AS ""Department"",
                                         specialization      AS ""Specialization""
                                  FROM crew_member
                                  ORDER BY crew_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Cast Roles", Icon = "🎭", TableName = "cast_role", CanAdd = true,
                    SelectSql = @"SELECT cr.role_id          AS ""Role ID"",
                                         m.title             AS ""Movie"",
                                         a.full_name         AS ""Actor"",
                                         cr.character_name   AS ""Character"",
                                         cr.role_type        AS ""Role Type"",
                                         cr.billing_order    AS ""Billing Order"",
                                         cr.acceptance_status AS ""Offer Status""
                                  FROM cast_role cr
                                  JOIN movie  m ON cr.movie_id = m.movie_id
                                  JOIN actor  a ON cr.actor_id = a.actor_id
                                  ORDER BY m.title, cr.billing_order"
                });

                navItems.Add(new NavItem
                {
                    Label = "Contracts", Icon = "📝", TableName = "contract", CanAdd = true,
                    SelectSql = @"SELECT c.contract_id       AS ""Contract ID"",
                                         m.title             AS ""Movie"",
                                         c.party_type        AS ""Party Type"",
                                         c.party_id          AS ""Party ID"",
                                         TO_CHAR(c.signed_date,'DD-Mon-YYYY')  AS ""Signed Date"",
                                         TO_CHAR(c.expiry_date,'DD-Mon-YYYY')  AS ""Expiry Date"",
                                         TO_CHAR(c.total_value,'FM999,999,999') AS ""Value (PKR)"",
                                         c.terms_summary     AS ""Terms""
                                  FROM contract c
                                  JOIN movie m ON c.movie_id = m.movie_id
                                  ORDER BY c.contract_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Locations", Icon = "📍", TableName = "shooting_location", CanAdd = true,
                    SelectSql = @"SELECT location_id         AS ""Location ID"",
                                         name                AS ""Location Name"",
                                         address             AS ""Address"",
                                         location_type       AS ""Type"",
                                         country             AS ""Country"",
                                         TO_CHAR(daily_rate,'FM999,999') AS ""Daily Rate (PKR)""
                                  FROM shooting_location
                                  ORDER BY location_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Schedule", Icon = "📅", TableName = "schedule", CanAdd = true,
                    SelectSql = @"SELECT sch.schedule_id     AS ""Schedule ID"",
                                         m.title             AS ""Movie"",
                                         sc.scene_no         AS ""Scene #"",
                                         sl.name             AS ""Location"",
                                         TO_CHAR(sch.shoot_date,'DD-Mon-YYYY') AS ""Shoot Date"",
                                         TO_CHAR(sch.start_time,'HH24:MI')     AS ""Start Time"",
                                         TO_CHAR(sch.end_time,'HH24:MI')       AS ""End Time"",
                                         sch.status          AS ""Status""
                                  FROM schedule sch
                                  JOIN movie               m  ON sch.movie_id    = m.movie_id
                                  JOIN scene              sc  ON sch.scene_id    = sc.scene_id
                                  JOIN shooting_location  sl  ON sch.location_id = sl.location_id
                                  ORDER BY sch.shoot_date"
                });

                navItems.Add(new NavItem
                {
                    Label = "Scenes", Icon = "🎬", TableName = "scene", CanAdd = true,
                    SelectSql = @"SELECT sc.scene_id         AS ""Scene ID"",
                                         m.title             AS ""Movie"",
                                         sc.scene_no         AS ""Scene #"",
                                         sl.name             AS ""Location"",
                                         sc.description      AS ""Description"",
                                         sc.duration         AS ""Duration (min)"",
                                         sc.status           AS ""Status""
                                  FROM scene sc
                                  JOIN movie m ON sc.movie_id = m.movie_id
                                  LEFT JOIN shooting_location sl ON sc.location_id = sl.location_id
                                  ORDER BY sc.scene_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Crew Assignments", Icon = "🔧", TableName = "crew_assignment", CanAdd = true,
                    SelectSql = @"SELECT ca.assignment_id    AS ""Assignment ID"",
                                         m.title             AS ""Movie"",
                                         cm.full_name        AS ""Crew Member"",
                                         cm.department       AS ""Department"",
                                         ca.job_title        AS ""Job Title"",
                                         TO_CHAR(ca.start_date,'DD-Mon-YYYY') AS ""Start Date"",
                                         TO_CHAR(ca.end_date,'DD-Mon-YYYY')   AS ""End Date""
                                  FROM crew_assignment ca
                                  JOIN movie       m  ON ca.movie_id = m.movie_id
                                  JOIN crew_member cm ON ca.crew_id  = cm.crew_id
                                  ORDER BY ca.assignment_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Financials", Icon = "💰", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = @"SELECT m.title             AS ""Movie"",
                                         m.status            AS ""Status"",
                                         TO_CHAR(m.budget,'FM999,999,999') AS ""Total Budget (PKR)"",
                                         TO_CHAR(NVL(SUM(c.total_value),0),'FM999,999,999') AS ""Contracted (PKR)"",
                                         TO_CHAR(m.budget - NVL(SUM(c.total_value),0),'FM999,999,999') AS ""Remaining (PKR)""
                                  FROM movie m
                                  LEFT JOIN contract c ON m.movie_id = c.movie_id
                                  GROUP BY m.movie_id, m.title, m.status, m.budget
                                  ORDER BY m.movie_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "System Users", Icon = "🔐", TableName = "app_user", CanAdd = true,
                    SelectSql = @"SELECT user_id             AS ""User ID"",
                                         role                AS ""Role"",
                                         username            AS ""Username"",
                                         full_name           AS ""Full Name"",
                                         party_id            AS ""Party ID""
                                  FROM app_user
                                  ORDER BY role, user_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Profile", Icon = "👤", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT user_id            AS ""User ID"",
                                          role               AS ""Role"",
                                          username           AS ""Username"",
                                          full_name          AS ""Full Name""
                                   FROM app_user
                                   WHERE user_id = {DatabaseHelper.CurrentUserId}"
                });
            }

            // ── DIRECTOR ────────────────────────────────────────────────
            else if (role.Equals("Director", StringComparison.OrdinalIgnoreCase))
            {
                navItems.Add(new NavItem
                {
                    Label = "My Movies", Icon = "🎬", TableName = "movie", CanAdd = true,
                    SelectSql = $@"SELECT m.movie_id         AS ""Movie ID"",
                                          m.title            AS ""Title"",
                                          m.genre            AS ""Genre"",
                                          m.release_year     AS ""Year"",
                                          m.rating           AS ""Rating"",
                                          m.status           AS ""Status"",
                                          TO_CHAR(m.budget,'FM999,999,999') AS ""Budget (PKR)""
                                   FROM movie m
                                   WHERE m.director_id = {pid}
                                   ORDER BY m.movie_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Cast", Icon = "🎭", TableName = "cast_role", CanAdd = true,
                    SelectSql = $@"SELECT cr.role_id         AS ""Role ID"",
                                          m.title            AS ""Movie"",
                                          a.full_name        AS ""Actor"",
                                          cr.character_name  AS ""Character"",
                                          cr.role_type       AS ""Role Type"",
                                          cr.billing_order   AS ""Billing Order"",
                                          cr.acceptance_status AS ""Offer Status""
                                   FROM cast_role cr
                                   JOIN movie m ON cr.movie_id = m.movie_id
                                   JOIN actor a ON cr.actor_id = a.actor_id
                                   WHERE m.director_id = {pid}
                                   ORDER BY m.title, cr.billing_order"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Crew", Icon = "🎞️", TableName = "crew_assignment", CanAdd = true,
                    SelectSql = $@"SELECT ca.assignment_id   AS ""Assignment ID"",
                                          m.title            AS ""Movie"",
                                          cm.full_name       AS ""Crew Member"",
                                          cm.department      AS ""Department"",
                                          ca.job_title       AS ""Job Title"",
                                          TO_CHAR(ca.start_date,'DD-Mon-YYYY') AS ""Start Date"",
                                          TO_CHAR(ca.end_date,'DD-Mon-YYYY')   AS ""End Date""
                                   FROM crew_assignment ca
                                   JOIN movie       m  ON ca.movie_id = m.movie_id
                                   JOIN crew_member cm ON ca.crew_id  = cm.crew_id
                                   WHERE m.director_id = {pid}
                                   ORDER BY m.title"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Schedule", Icon = "📅", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT sch.schedule_id    AS ""Schedule ID"",
                                          m.title            AS ""Movie"",
                                          sc.scene_no        AS ""Scene #"",
                                          sl.name            AS ""Location"",
                                          TO_CHAR(sch.shoot_date,'DD-Mon-YYYY') AS ""Shoot Date"",
                                          TO_CHAR(sch.start_time,'HH24:MI')     AS ""Start Time"",
                                          TO_CHAR(sch.end_time,'HH24:MI')       AS ""End Time"",
                                          sch.status         AS ""Status""
                                   FROM schedule sch
                                   JOIN movie              m  ON sch.movie_id    = m.movie_id
                                   JOIN scene             sc  ON sch.scene_id    = sc.scene_id
                                   JOIN shooting_location sl  ON sch.location_id = sl.location_id
                                   WHERE m.director_id = {pid}
                                   ORDER BY sch.shoot_date"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Contracts", Icon = "📝", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT c.contract_id      AS ""Contract ID"",
                                          m.title            AS ""Movie"",
                                          c.party_type       AS ""Party Type"",
                                          TO_CHAR(c.signed_date,'DD-Mon-YYYY') AS ""Signed Date"",
                                          TO_CHAR(c.expiry_date,'DD-Mon-YYYY') AS ""Expiry Date"",
                                          TO_CHAR(c.total_value,'FM999,999,999') AS ""Value (PKR)"",
                                          c.terms_summary    AS ""Terms""
                                   FROM contract c
                                   JOIN movie m ON c.movie_id = m.movie_id
                                   WHERE m.director_id = {pid}
                                   ORDER BY c.contract_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "Budget Overview", Icon = "💰", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT m.title            AS ""Movie"",
                                          m.status           AS ""Status"",
                                          TO_CHAR(m.budget,'FM999,999,999') AS ""Total Budget (PKR)"",
                                          TO_CHAR(NVL(SUM(c.total_value),0),'FM999,999,999') AS ""Contracted (PKR)"",
                                          TO_CHAR(m.budget - NVL(SUM(c.total_value),0),'FM999,999,999') AS ""Remaining (PKR)""
                                   FROM movie m
                                   LEFT JOIN contract c ON m.movie_id = c.movie_id
                                   WHERE m.director_id = {pid}
                                   GROUP BY m.movie_id, m.title, m.status, m.budget
                                   ORDER BY m.movie_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Profile", Icon = "👤", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT director_id        AS ""Director ID"",
                                          full_name          AS ""Full Name"",
                                          TO_CHAR(dob,'DD-Mon-YYYY') AS ""Date of Birth"",
                                          nationality        AS ""Nationality"",
                                          biography          AS ""Biography""
                                   FROM director
                                   WHERE director_id = {pid}"
                });
            }

            // ── ACTOR ────────────────────────────────────────────────────
            else if (role.Equals("Actor", StringComparison.OrdinalIgnoreCase))
            {
                navItems.Add(new NavItem
                {
                    Label = "My Roles", Icon = "🎭", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT cr.role_id         AS ""Role ID"",
                                          m.title            AS ""Movie"",
                                          d.full_name        AS ""Director"",
                                          cr.character_name  AS ""Character"",
                                          cr.role_type       AS ""Role Type"",
                                          cr.billing_order   AS ""Billing Order"",
                                          m.genre            AS ""Genre"",
                                          m.status           AS ""Movie Status"",
                                          cr.acceptance_status AS ""My Decision""
                                   FROM cast_role cr
                                   JOIN movie    m ON cr.movie_id = m.movie_id
                                   JOIN director d ON m.director_id = d.director_id
                                   WHERE cr.actor_id = {pid}
                                   ORDER BY m.title"
                });

                navItems.Add(new NavItem
                {
                    Label = "Pending Offers", Icon = "✉️", TableName = "cast_role", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT cr.role_id         AS ""Role ID"",
                                          m.title            AS ""Movie"",
                                          d.full_name        AS ""Director"",
                                          cr.character_name  AS ""Character"",
                                          cr.role_type       AS ""Role Type"",
                                          m.genre            AS ""Genre"",
                                          m.status           AS ""Movie Status"",
                                          TO_CHAR(m.budget,'FM999,999,999') AS ""Movie Budget (PKR)"",
                                          cr.acceptance_status AS ""Current Status""
                                   FROM cast_role cr
                                   JOIN movie    m ON cr.movie_id = m.movie_id
                                   JOIN director d ON m.director_id = d.director_id
                                   WHERE cr.actor_id = {pid}
                                     AND cr.acceptance_status = 'Pending'
                                   ORDER BY m.title"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Movies", Icon = "🎬", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT m.movie_id         AS ""Movie ID"",
                                          m.title            AS ""Title"",
                                          d.full_name        AS ""Director"",
                                          m.genre            AS ""Genre"",
                                          m.release_year     AS ""Year"",
                                          m.rating           AS ""Rating"",
                                          m.status           AS ""Status"",
                                          TO_CHAR(m.budget,'FM999,999,999') AS ""Budget (PKR)""
                                   FROM movie m
                                   JOIN director d ON m.director_id = d.director_id
                                   WHERE m.movie_id IN (SELECT movie_id FROM cast_role WHERE actor_id = {pid})
                                   ORDER BY m.movie_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Contracts", Icon = "📝", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT c.contract_id      AS ""Contract ID"",
                                          m.title            AS ""Movie"",
                                          TO_CHAR(c.signed_date,'DD-Mon-YYYY')  AS ""Signed Date"",
                                          TO_CHAR(c.expiry_date,'DD-Mon-YYYY')  AS ""Expiry Date"",
                                          TO_CHAR(c.total_value,'FM999,999,999') AS ""Value (PKR)"",
                                          c.terms_summary    AS ""Terms""
                                   FROM contract c
                                   JOIN movie m ON c.movie_id = m.movie_id
                                   WHERE c.party_id = {pid} AND c.party_type = 'Actor'
                                   ORDER BY c.contract_id"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Profile", Icon = "👤", TableName = "actor", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT actor_id           AS ""Actor ID"",
                                          full_name          AS ""Full Name"",
                                          TO_CHAR(dob,'DD-Mon-YYYY') AS ""Date of Birth"",
                                          nationality        AS ""Nationality"",
                                          contact            AS ""Contact Email""
                                   FROM actor
                                   WHERE actor_id = {pid}"
                });
            }

            // ── CREW ─────────────────────────────────────────────────────
            else if (role.Equals("Crew", StringComparison.OrdinalIgnoreCase))
            {
                navItems.Add(new NavItem
                {
                    Label = "My Assignments", Icon = "🔧", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT ca.assignment_id   AS ""Assignment ID"",
                                          m.title            AS ""Movie"",
                                          m.status           AS ""Movie Status"",
                                          d.full_name        AS ""Director"",
                                          ca.job_title       AS ""My Job Title"",
                                          TO_CHAR(ca.start_date,'DD-Mon-YYYY') AS ""Start Date"",
                                          TO_CHAR(ca.end_date,'DD-Mon-YYYY')   AS ""End Date""
                                   FROM crew_assignment ca
                                   JOIN movie    m ON ca.movie_id = m.movie_id
                                   JOIN director d ON m.director_id = d.director_id
                                   WHERE ca.crew_id = {pid}
                                   ORDER BY ca.start_date"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Schedule", Icon = "📅", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT sch.schedule_id    AS ""Schedule ID"",
                                          m.title            AS ""Movie"",
                                          sc.scene_no        AS ""Scene #"",
                                          sl.name            AS ""Location"",
                                          TO_CHAR(sch.shoot_date,'DD-Mon-YYYY') AS ""Shoot Date"",
                                          TO_CHAR(sch.start_time,'HH24:MI')     AS ""Start Time"",
                                          TO_CHAR(sch.end_time,'HH24:MI')       AS ""End Time"",
                                          sch.status         AS ""Status""
                                   FROM schedule sch
                                   JOIN movie              m  ON sch.movie_id    = m.movie_id
                                   JOIN scene             sc  ON sch.scene_id    = sc.scene_id
                                   JOIN shooting_location sl  ON sch.location_id = sl.location_id
                                   WHERE m.movie_id IN (SELECT movie_id FROM crew_assignment WHERE crew_id = {pid})
                                   ORDER BY sch.shoot_date"
                });

                navItems.Add(new NavItem
                {
                    Label = "My Profile", Icon = "👤", TableName = "", CanAdd = false, CanDelete = false,
                    SelectSql = $@"SELECT crew_id            AS ""Crew ID"",
                                          full_name          AS ""Full Name"",
                                          department         AS ""Department"",
                                          specialization     AS ""Specialization""
                                   FROM crew_member
                                   WHERE crew_id = {pid}"
                });
            }
        }

        // ════════════════════════════════════════════════════════════════
        // UI BUILDING
        // ════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            this.Text = $"🎬 Film Studio — {DatabaseHelper.CurrentRole} Dashboard";
            this.Size = new Size(1400, 860);
            this.BackColor = BgDark;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

            BuildSidebar();
            BuildTopBar();
            BuildContent();

            this.Controls.Add(contentPanel);
            this.Controls.Add(topBar);
            this.Controls.Add(sidebarPanel);

            sidebarPanel.SendToBack();
            topBar.SendToBack();
            contentPanel.BringToFront();
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = BgPanel };

            // Bottom controls (add them first so they dock bottom)
            var userInfoPanel = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = Color.FromArgb( 30,  41,  59), Padding = new Padding(14, 10, 8, 0) };
            userInfoPanel.Controls.Add(new Label
            {
                Text = $"👤  {DatabaseHelper.CurrentFullName}",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextPri, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });

            var btnLogout = new Button
            {
                Text = "  🚪  Logout", Dock = DockStyle.Bottom, Height = 50,
                FlatStyle = FlatStyle.Flat, ForeColor = Danger,
                BackColor = Color.FromArgb( 40,  20,  20),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { DatabaseHelper.ShouldLogout = true; this.Close(); };

            sidebarPanel.Controls.Add(userInfoPanel);
            sidebarPanel.Controls.Add(btnLogout);

            // Top controls
            var logoPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, BackColor = Color.FromArgb( 15,  23,  42), Padding = new Padding(14, 18, 0, 14) };
            logoPanel.Controls.Add(new Label { Text = "🎬 FilmStudio", Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 4) });
            logoPanel.Controls.Add(new Label { Text = "Management System", Font = new Font("Segoe UI", 9f), ForeColor = TextSec, AutoSize = true, Margin = new Padding(4, 0, 0, 0) });

            Color roleColor = DatabaseHelper.CurrentRole switch
            {
                "Admin"    => Color.FromArgb(239, 68, 68),
                "Director" => Color.FromArgb(59, 130, 246),
                "Actor"    => Color.FromArgb(168, 85, 247),
                _          => Color.FromArgb(234, 179, 8)
            };
            string roleIcon = DatabaseHelper.CurrentRole switch
            {
                "Admin"    => "⚙",
                "Director" => "🎥",
                "Actor"    => "🌟",
                _          => "🛠"
            };
            var roleBadge = new Label
            {
                Text = $"  {roleIcon}  {DatabaseHelper.CurrentRole.ToUpper()}",
                Dock = DockStyle.Top, Height = 28,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = roleColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                BackColor = Color.FromArgb(30, roleColor.R, roleColor.G, roleColor.B)
            };

            var navLabel = new Label { Text = "  NAVIGATION", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TextSec, Dock = DockStyle.Top, Height = 32, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(16, 0, 0, 0) };

            sidebarPanel.Controls.Add(navLabel);
            sidebarPanel.Controls.Add(roleBadge);
            sidebarPanel.Controls.Add(logoPanel);

            // Scrollable Nav Panel for buttons
            var navScrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            navButtons = new Button[navItems.Count];
            for (int i = navItems.Count - 1; i >= 0; i--)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = $"  {navItems[i].Icon}  {navItems[i].Label}",
                    Dock = DockStyle.Top, Height = 46,
                    FlatStyle = FlatStyle.Flat, ForeColor = TextSec,
                    BackColor = BgPanel, TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb( 51,  65,  85);
                btn.Click += (s, e) => LoadData(idx);
                navButtons[i] = btn;
                navScrollPanel.Controls.Add(btn);
            }

            sidebarPanel.Controls.Add(navScrollPanel);
            navScrollPanel.BringToFront(); // Ensure Fill behavior evaluates correctly
        }

        private void BuildTopBar()
        {
            topBar = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = BgPanel, Padding = new Padding(24, 0, 24, 0) };

            tableTitle = new Label { Text = "Dashboard", Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Location = new Point(24, 32) };
            topBar.Controls.Add(tableTitle);

            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, Padding = new Padding(0, 28, 0, 0),
                BackColor = Color.Transparent
            };

            btnSave = MakeTopBtn("💾 Save", Accent, TextPri, 120);
            btnSave.Click += BtnSave_Click;

            btnRefresh = MakeTopBtn("↻ Refresh", Color.FromArgb( 51,  65,  85), TextPri, 100);
            btnRefresh.Click += (s, e) => LoadData(selectedNavIndex);

            btnDelete = MakeTopBtn("🗑 Delete", Danger, Color.White, 90);
            btnDelete.Click += BtnDelete_Click;

            btnAdd = MakeTopBtn("➕ Add", Success, Color.White, 80);
            btnAdd.Click += BtnAdd_Click;

            btnEditProfile = MakeTopBtn("✏️ Edit Profile", Accent, TextPri, 120);
            btnEditProfile.Visible = false;
            btnEditProfile.Click += BtnEditProfile_Click;

            // Actor-specific: Accept/Decline role offer
            btnAcceptRole = MakeTopBtn("✅ Accept Offer", Color.FromArgb( 16, 185, 129), Color.White, 140);
            btnAcceptRole.Visible = false;
            btnAcceptRole.Click += BtnAcceptRole_Click;

            var declineBtn = MakeTopBtn("❌ Decline Offer", Danger, Color.White, 140);
            declineBtn.Name = "btnDeclineRole";
            declineBtn.Visible = false;
            declineBtn.Click += BtnDeclineRole_Click;

            headerUserLabel = new Label
            {
                Text = $"👤 {DatabaseHelper.CurrentFullName}  |  {DatabaseHelper.CurrentRole}",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = TextPri, AutoSize = true,
                Margin = new Padding(8, 6, 24, 0), TextAlign = ContentAlignment.MiddleLeft
            };

            actionPanel.Controls.Add(btnSave);
            actionPanel.Controls.Add(btnRefresh);
            actionPanel.Controls.Add(btnDelete);
            actionPanel.Controls.Add(btnAdd);
            actionPanel.Controls.Add(btnEditProfile);
            actionPanel.Controls.Add(btnAcceptRole);
            actionPanel.Controls.Add(declineBtn);
            actionPanel.Controls.Add(headerUserLabel);

            topBar.Controls.Add(actionPanel);
        }

        private Button MakeTopBtn(string text, Color bg, Color fg, int width)
        {
            var btn = new Button
            {
                Text = text, Width = width, Height = 40,
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(6, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void BuildContent()
        {
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = BgDark, Padding = new Padding(24, 16, 24, 24) };

            statsBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = BgDark, Padding = new Padding(0, 0, 0, 14) };

            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = BgCard, Padding = new Padding(0) };

            dataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = BgCard,
                BorderStyle = BorderStyle.None,
                GridColor = GridLine,

                // ── Column headers ────────────────────────────────────────
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 44,
                EnableHeadersVisualStyles = false,   // MUST be false to apply custom style
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor    = HeaderBg,          // Light blue header background
                    ForeColor    = TextPri,           // Dark text
                    Font         = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    SelectionBackColor = HeaderBg,
                    SelectionForeColor = TextPri,
                    Alignment    = DataGridViewContentAlignment.MiddleLeft,
                    Padding      = new Padding(8, 0, 0, 0)
                },

                // ── Cell style ────────────────────────────────────────────
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor    = BgCard, ForeColor = TextPri,
                    SelectionBackColor = Color.FromArgb( 46,  51,  64),
                    SelectionForeColor = Accent,
                    Font = new Font("Segoe UI", 9.5f),
                    WrapMode = DataGridViewTriState.False,
                    Padding = new Padding(6, 0, 0, 0)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb( 30,  41,  59)
                },

                // ── Row headers ───────────────────────────────────────────
                RowHeadersVisible = true, RowHeadersWidth = 30,
                RowHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = HeaderBg },

                AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToAddRows   = false,
                AllowUserToDeleteRows = true,
                ReadOnly             = false,
                SelectionMode        = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle      = DataGridViewCellBorderStyle.SingleHorizontal,
                RowTemplate          = { Height = 40 }
            };

            dataGrid.DataError += (s, e) =>
            {
                MessageBox.Show("Invalid input format for this field.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.ThrowException = false;
            };

            gridCard.Controls.Add(dataGrid);
            contentPanel.Controls.Add(gridCard);
            contentPanel.Controls.Add(statsBar);
            gridCard.BringToFront(); // Ensure gridCard fills remaining space below statsBar
        }

        // ════════════════════════════════════════════════════════════════
        // LOAD DATA
        // ════════════════════════════════════════════════════════════════
        private void LoadData(int navIndex)
        {
            if (navItems.Count == 0 || navIndex < 0 || navIndex >= navItems.Count) return;
            selectedNavIndex = navIndex;
            var item = navItems[navIndex];
            tableTitle.Text = $"{item.Icon}  {item.Label}";

            // Highlight active nav button
            for (int i = 0; i < navButtons.Length; i++)
            {
                navButtons[i].BackColor = (i == navIndex) ? Color.FromArgb( 51,  65,  85) : BgPanel;
                navButtons[i].ForeColor = (i == navIndex) ? Accent : TextSec;
                navButtons[i].Font = new Font("Segoe UI", 9.5f, i == navIndex ? FontStyle.Bold : FontStyle.Regular);
            }

            string role = (DatabaseHelper.CurrentRole ?? "").Trim();

            // ── Button visibility per nav item ────────────────────────────
            bool isAdmin    = role.Equals("Admin",    StringComparison.OrdinalIgnoreCase);
            bool isDirector = role.Equals("Director", StringComparison.OrdinalIgnoreCase);
            bool isActor    = role.Equals("Actor",    StringComparison.OrdinalIgnoreCase);

            btnAdd.Visible    = item.CanAdd;
            btnDelete.Visible = item.CanDelete && (isAdmin || isDirector);
            btnSave.Visible   = !string.IsNullOrEmpty(item.TableName) && (isAdmin || isDirector);
            dataGrid.ReadOnly = !(!string.IsNullOrEmpty(item.TableName) && (isAdmin || isDirector));

            btnEditProfile.Visible = item.Label == "My Profile";

            // Accept/Decline offer buttons — only for Actor > Pending Offers
            bool isPendingOffers = isActor && item.Label == "Pending Offers";
            btnAcceptRole.Visible = isPendingOffers;
            // find decline button
            foreach (Control c in topBar.Controls)
                if (c is FlowLayoutPanel fp)
                    foreach (Control c2 in fp.Controls)
                        if (c2.Name == "btnDeclineRole") c2.Visible = isPendingOffers;

            // ── Execute SELECT ────────────────────────────────────────────
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                _adapter   = new OracleDataAdapter(item.SelectSql, conn);
                _dataTable = new DataTable();
                _adapter.Fill(_dataTable);
                dataGrid.DataSource = _dataTable;

                // Auto-size then cap wide columns
                foreach (DataGridViewColumn col in dataGrid.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.Automatic;
                    if (col.Width > 300) col.Width = 300;
                    col.MinimumWidth = 70;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Data load error:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            BuildStatsBar();
        }

        // ════════════════════════════════════════════════════════════════
        // STATS BAR
        // ════════════════════════════════════════════════════════════════
        private void BuildStatsBar()
        {
            statsBar.Controls.Clear();
            string role = (DatabaseHelper.CurrentRole ?? "").Trim();
            int    pid  = DatabaseHelper.CurrentPartyId;
            var    stats = new List<(string label, string query, Color color)>();

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                stats.Add(("Total Movies",    "SELECT COUNT(*) FROM movie",                                                   Color.FromArgb(59, 130, 246)));
                stats.Add(("In Production",   "SELECT COUNT(*) FROM movie WHERE status='In Production'",                     Success));
                stats.Add(("Total Actors",    "SELECT COUNT(*) FROM actor",                                                   Color.FromArgb(168, 85, 247)));
                stats.Add(("Total Directors", "SELECT COUNT(*) FROM director",                                                Accent));
                stats.Add(("System Users",    "SELECT COUNT(*) FROM app_user",                                               Color.FromArgb(251, 191, 36)));
            }
            else if (role.Equals("Director", StringComparison.OrdinalIgnoreCase))
            {
                stats.Add(("My Movies",       $"SELECT COUNT(*) FROM movie WHERE director_id={pid}",                         Color.FromArgb(59, 130, 246)));
                stats.Add(("Active Shoots",   $"SELECT COUNT(*) FROM schedule WHERE status='Scheduled' AND movie_id IN (SELECT movie_id FROM movie WHERE director_id={pid})", Success));
                stats.Add(("Cast Members",    $"SELECT COUNT(*) FROM cast_role WHERE movie_id IN (SELECT movie_id FROM movie WHERE director_id={pid})", Color.FromArgb(168, 85, 247)));
                stats.Add(("Crew Members",    $"SELECT COUNT(*) FROM crew_assignment WHERE movie_id IN (SELECT movie_id FROM movie WHERE director_id={pid})", Accent));
            }
            else if (role.Equals("Actor", StringComparison.OrdinalIgnoreCase))
            {
                stats.Add(("Total Roles",     $"SELECT COUNT(*) FROM cast_role WHERE actor_id={pid}",                        Color.FromArgb(168, 85, 247)));
                stats.Add(("Pending Offers",  $"SELECT COUNT(*) FROM cast_role WHERE actor_id={pid} AND acceptance_status='Pending'", Color.FromArgb(251, 191, 36)));
                stats.Add(("Accepted",        $"SELECT COUNT(*) FROM cast_role WHERE actor_id={pid} AND acceptance_status='Accepted'", Success));
                stats.Add(("Contracts",       $"SELECT COUNT(*) FROM contract WHERE party_id={pid} AND party_type='Actor'",  Color.FromArgb(59, 130, 246)));
            }
            else if (role.Equals("Crew", StringComparison.OrdinalIgnoreCase))
            {
                stats.Add(("Assignments",     $"SELECT COUNT(*) FROM crew_assignment WHERE crew_id={pid}",                   Color.FromArgb(59, 130, 246)));
                stats.Add(("Schedules",       $"SELECT COUNT(*) FROM schedule WHERE movie_id IN (SELECT movie_id FROM crew_assignment WHERE crew_id={pid})", Success));
            }

            foreach (var (label, query, color) in stats)
            {
                string val = "0";
                try { val = DatabaseHelper.ExecuteScalar(query)?.ToString() ?? "0"; } catch { }
                var card = new Panel { BackColor = BgCard, Width = 190, Height = 90, Margin = new Padding(0, 0, 14, 14) };
                var top  = new Panel { BackColor = color, Dock = DockStyle.Top, Height = 5 };
                var vLbl = new Label { Text = val, ForeColor = TextPri, Font = new Font("Segoe UI", 22f, FontStyle.Bold), Dock = DockStyle.Top, Height = 48, Padding = new Padding(14, 0, 0, 0), TextAlign = ContentAlignment.BottomLeft };
                var nLbl = new Label { Text = label.ToUpper(), ForeColor = TextSec, Font = new Font("Segoe UI", 8f, FontStyle.Bold), Dock = DockStyle.Top, Height = 28, Padding = new Padding(14, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft };
                card.Controls.AddRange(new Control[] { vLbl, nLbl, top });
                statsBar.Controls.Add(card);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // ADD — TABLE-SPECIFIC FORMS
        // ════════════════════════════════════════════════════════════════
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var item = navItems[selectedNavIndex];
            if (!item.CanAdd || string.IsNullOrEmpty(item.TableName)) return;

            switch (item.TableName)
            {
                case "movie":           ShowAddMovieForm();          break;
                case "director":        ShowAddDirectorForm();       break;
                case "actor":           ShowAddActorForm();          break;
            case "crew_member":     ShowAddCrewMemberForm();     break;
                case "cast_role":       ShowAddCastRoleForm();       break;
                case "contract":        ShowAddContractForm();       break;
                case "shooting_location": ShowAddLocationForm();     break;
                case "schedule":        ShowAddScheduleForm();       break;
                case "scene":           ShowAddSceneForm();          break;
                case "crew_assignment": ShowAddCrewAssignmentForm(); break;
                case "app_user":        ShowAddUserForm();           break;
                default:
                    MessageBox.Show("Add is not supported for this table.", "Info");
                    break;
            }
        }

        private void AutoGenerateUser(string role, string fullName, string partyId)
        {
            string username = fullName.Replace(" ", ".").ToLower() + new Random().Next(10, 99);
            string password = Guid.NewGuid().ToString().Substring(0, 6);
            int newUserId = 1;
            try {
                using var conn = new OracleConnection(DatabaseHelper.ActiveConnectionString);
                conn.Open();
                using var cmd1 = new OracleCommand("SELECT NVL(MAX(user_id),0) FROM app_user", conn);
                newUserId = Convert.ToInt32(cmd1.ExecuteScalar()) + 1;
                
                using var cmd2 = new OracleCommand("INSERT INTO app_user (user_id, party_id, role, username, password, full_name) VALUES (:p_uid,:p_pid,:p_role,:p_user,:p_pass,:p_name)", conn);
                cmd2.BindByName = true;
                cmd2.Parameters.Add(new OracleParameter("p_uid", newUserId));
                cmd2.Parameters.Add(new OracleParameter("p_pid", partyId));
                cmd2.Parameters.Add(new OracleParameter("p_role", role));
                cmd2.Parameters.Add(new OracleParameter("p_user", username));
                cmd2.Parameters.Add(new OracleParameter("p_pass", password));
                cmd2.Parameters.Add(new OracleParameter("p_name", fullName));
                cmd2.ExecuteNonQuery();
                
                MessageBox.Show($"New account created for {fullName}!\n\nUsername: {username}\nPassword: {password}", "Credentials Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch(Exception ex) {
                MessageBox.Show("Auto-user generation failed: " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEditProfile_Click(object? sender, EventArgs e)
        {
            try {
                using var conn = new OracleConnection(DatabaseHelper.ActiveConnectionString);
                conn.Open();
                using var cmd = new OracleCommand("SELECT username, password, full_name FROM app_user WHERE user_id = :p_uid", conn);
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("p_uid", DatabaseHelper.CurrentUserId));
                using var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    string curUser = reader["username"].ToString() ?? "";
                    string curPass = reader["password"].ToString() ?? "";
                    string curName = reader["full_name"].ToString() ?? "";

                    var (frm, flow) = MakeAddForm("Edit Profile");
                    flow.Controls.Add(new Label { Text = "Edit Your Profile", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

                    flow.Controls.Add(MakeFrmLabel("Full Name")); var tbName = MakeFrmTextBox(curName); flow.Controls.Add(tbName); tbName.Text = curName;
                    flow.Controls.Add(MakeFrmLabel("Username"));  var tbUser = MakeFrmTextBox(curUser); flow.Controls.Add(tbUser); tbUser.Text = curUser;
                    flow.Controls.Add(MakeFrmLabel("Password"));  var tbPass = MakeFrmTextBox(); flow.Controls.Add(tbPass); tbPass.Text = curPass;

                    var btnSave = MakeFrmSaveBtn("Save Changes");
                    btnSave.Click += (s, ev) => {
                        try {
                            using var uCmd = new OracleCommand("UPDATE app_user SET full_name = :p_n, username = :p_u, password = :p_p WHERE user_id = :p_uid", conn);
                            uCmd.BindByName = true;
                            uCmd.Parameters.Add(new OracleParameter("p_n", tbName.Text));
                            uCmd.Parameters.Add(new OracleParameter("p_u", tbUser.Text));
                            uCmd.Parameters.Add(new OracleParameter("p_p", tbPass.Text));
                            uCmd.Parameters.Add(new OracleParameter("p_uid", DatabaseHelper.CurrentUserId));
                            uCmd.ExecuteNonQuery();

                            // Cascade full_name to specific role tables if possible
                            string r = DatabaseHelper.CurrentRole;
                            if (r == "Actor") new OracleCommand($"UPDATE actor SET full_name = '{tbName.Text.Replace("'", "''")}' WHERE actor_id = {DatabaseHelper.CurrentPartyId}", conn).ExecuteNonQuery();
                            else if (r == "Director") new OracleCommand($"UPDATE director SET full_name = '{tbName.Text.Replace("'", "''")}' WHERE director_id = {DatabaseHelper.CurrentPartyId}", conn).ExecuteNonQuery();
                            else if (r == "Crew") new OracleCommand($"UPDATE crew_member SET full_name = '{tbName.Text.Replace("'", "''")}' WHERE crew_id = {DatabaseHelper.CurrentPartyId}", conn).ExecuteNonQuery();

                            MessageBox.Show("Profile updated successfully! Next login will reflect changes if username changed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            frm.Close();
                            LoadData(selectedNavIndex);
                        } catch(Exception ex) {
                            MessageBox.Show("Update failed: " + ex.Message, "Error");
                        }
                    };
                    flow.Controls.Add(btnSave);
                    frm.ShowDialog(this);
                }
            } catch (Exception ex) { MessageBox.Show("Failed to load profile: " + ex.Message); }
        }

        private bool RunInsert(string sql, OracleParameter[] parms, Form frm)
        {
            try
            {
                using var conn = new OracleConnection(DatabaseHelper.ActiveConnectionString);
                conn.Open();
                using var cmd = new OracleCommand(sql, conn);
                cmd.BindByName = true;
                if (parms != null) 
                {
                    foreach (var p in parms)
                    {
                        if (p.Value is string s && string.IsNullOrWhiteSpace(s))
                            p.Value = DBNull.Value;
                    }
                    cmd.Parameters.AddRange(parms);
                }
                cmd.ExecuteNonQuery();
                MessageBox.Show("Successfully added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                frm.Close();
                LoadData(selectedNavIndex); // refresh
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Insert failed:\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ── Shared form shell ────────────────────────────────────────────
        private (Form frm, FlowLayoutPanel flow) MakeAddForm(string title)
        {
            var frm = new Form
            {
                Text = title, Size = new Size(520, 640),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = BgCard, Font = new Font("Segoe UI", 9.5f),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true,
                Padding = new Padding(24, 16, 24, 16)
            };
            frm.Controls.Add(flow);
            return (frm, flow);
        }

        private Label MakeFrmLabel(string text) =>
            new Label { Text = text, AutoSize = true, ForeColor = TextSec, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Margin = new Padding(0, 10, 0, 4) };

        private TextBox MakeFrmTextBox(string placeholder = "", int width = 440) =>
            new TextBox { Width = width, BackColor = BgInput, ForeColor = TextPri, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = placeholder };

        private ComboBox MakeFrmCombo(string[] items, int width = 440)
        {
            var cb = new ComboBox { Width = width, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f), BackColor = BgInput, ForeColor = TextPri };
            cb.Items.AddRange(items);
            if (cb.Items.Count > 0) cb.SelectedIndex = 0;
            return cb;
        }

        private Button MakeFrmSaveBtn(string text = "Add Record")
        {
            var btn = new Button
            {
                Text = text, Width = 440, Height = 44,
                BackColor = Success, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Margin = new Padding(0, 18, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }


        // ── Add Movie ────────────────────────────────────────────────────
        private void ShowAddMovieForm()
        {
            var (frm, flow) = MakeAddForm("Add New Movie");
            flow.Controls.Add(new Label { Text = "Add New Movie", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Movie ID")); var tbId    = MakeFrmTextBox("e.g. 16");                        flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Title"));    var tbTitle = MakeFrmTextBox("e.g. Lahore Se Aagey");          flow.Controls.Add(tbTitle);
            flow.Controls.Add(MakeFrmLabel("Genre"));
            var cbGenre = MakeFrmCombo(new[] { "Action","Romance","Comedy","Drama","Thriller","Horror","Biography","Historical","Musical","Crime" });
            flow.Controls.Add(cbGenre);
            flow.Controls.Add(MakeFrmLabel("Rating"));
            var cbRating = MakeFrmCombo(new[] { "U","U/A","A" });
            flow.Controls.Add(cbRating);
            flow.Controls.Add(MakeFrmLabel("Budget (PKR)"));    var tbBudget   = MakeFrmTextBox("e.g. 150000000");      flow.Controls.Add(tbBudget);
            flow.Controls.Add(MakeFrmLabel("Status"));
            var cbStatus = MakeFrmCombo(new[] { "Pre-Production","In Production","Post-Production","Released","Cancelled" });
            flow.Controls.Add(cbStatus);
            flow.Controls.Add(MakeFrmLabel("Director ID"));     var tbDirId    = MakeFrmTextBox("1-15");                flow.Controls.Add(tbDirId);

            var btnSave = MakeFrmSaveBtn("Add Movie");
            btnSave.Click += (s, e) =>
            {
                RunInsert(
                    "INSERT INTO movie (movie_id,title,genre,rating,budget,status,director_id,release_year) VALUES (:id,:title,:genre,:rating,:budget,:p_status,:dirid,EXTRACT(YEAR FROM SYSDATE))",
                    new[]
                    {
                        new OracleParameter("id",     tbId.Text),
                        new OracleParameter("title",  tbTitle.Text),
                        new OracleParameter("genre",  cbGenre.Text),
                        new OracleParameter("rating", cbRating.Text),
                        new OracleParameter("budget", tbBudget.Text),
                        new OracleParameter("p_status", cbStatus.Text),
                        new OracleParameter("dirid",  tbDirId.Text)
                    }, frm);
            };
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Director ─────────────────────────────────────────────────
        private void ShowAddDirectorForm()
        {
            var (frm, flow) = MakeAddForm("Add New Director");
            flow.Controls.Add(new Label { Text = "Add New Director", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Director ID")); var tbId   = MakeFrmTextBox("e.g. 16");           flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Full Name"));   var tbName = MakeFrmTextBox("e.g. Asad Malik");   flow.Controls.Add(tbName);
            flow.Controls.Add(MakeFrmLabel("Date of Birth (DD-MM-YYYY)")); var tbDob = MakeFrmTextBox("e.g. 01-01-1980"); flow.Controls.Add(tbDob);
            flow.Controls.Add(MakeFrmLabel("Nationality")); var tbNat  = MakeFrmTextBox("Pakistani");         flow.Controls.Add(tbNat);
            flow.Controls.Add(MakeFrmLabel("Biography"));   var tbBio  = MakeFrmTextBox("Short bio...");      flow.Controls.Add(tbBio);

            var btnSave = MakeFrmSaveBtn("Add Director");
            btnSave.Click += (s, e) => {
                if (RunInsert("INSERT INTO director VALUES (:id,:p_name,TO_DATE(:p_dob,'DD-MM-YYYY'),:p_nat,:p_bio)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("p_name",tbName.Text), new OracleParameter("p_dob",tbDob.Text), new OracleParameter("p_nat",tbNat.Text), new OracleParameter("p_bio",tbBio.Text) }, frm))
                {
                    AutoGenerateUser("Director", tbName.Text, tbId.Text);
                }
            };
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Actor ────────────────────────────────────────────────────
        private void ShowAddActorForm()
        {
            var (frm, flow) = MakeAddForm("Add New Actor");
            flow.Controls.Add(new Label { Text = "Add New Actor", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Actor ID"));   var tbId   = MakeFrmTextBox("e.g. 16");          flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Full Name"));  var tbName = MakeFrmTextBox("e.g. Ali Hassan");  flow.Controls.Add(tbName);
            flow.Controls.Add(MakeFrmLabel("Date of Birth (DD-MM-YYYY)")); var tbDob = MakeFrmTextBox("e.g. 01-01-1990"); flow.Controls.Add(tbDob);
            flow.Controls.Add(MakeFrmLabel("Nationality")); var tbNat = MakeFrmTextBox("Pakistani");        flow.Controls.Add(tbNat);
            flow.Controls.Add(MakeFrmLabel("Contact Email")); var tbContact = MakeFrmTextBox("email@example.com"); flow.Controls.Add(tbContact);

            var btnSave = MakeFrmSaveBtn("Add Actor");
            btnSave.Click += (s, e) => {
                if (RunInsert("INSERT INTO actor VALUES (:id,:p_name,TO_DATE(:p_dob,'DD-MM-YYYY'),:p_nat,:p_contact)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("p_name",tbName.Text), new OracleParameter("p_dob",tbDob.Text), new OracleParameter("p_nat",tbNat.Text), new OracleParameter("p_contact",tbContact.Text) }, frm))
                {
                    AutoGenerateUser("Actor", tbName.Text, tbId.Text);
                }
            };
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Crew Member ──────────────────────────────────────────────
        private void ShowAddCrewMemberForm()
        {
            var (frm, flow) = MakeAddForm("Add New Crew Member");
            flow.Controls.Add(new Label { Text = "Add Crew Member", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Crew ID"));        var tbId   = MakeFrmTextBox("e.g. 16");      flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Full Name"));      var tbName = MakeFrmTextBox();               flow.Controls.Add(tbName);
            flow.Controls.Add(MakeFrmLabel("Department"));
            var cbDept = MakeFrmCombo(new[] { "Cinematography","Production","Sound","Visual Effects","Art Direction","Editing","Costume","Stunts","Makeup","Lighting","Script","Music" });
            flow.Controls.Add(cbDept);
            flow.Controls.Add(MakeFrmLabel("Specialization")); var tbSpec = MakeFrmTextBox("e.g. Gaffer");  flow.Controls.Add(tbSpec);

            var btnSave = MakeFrmSaveBtn("Add Crew Member");
            btnSave.Click += (s, e) => {
                if (RunInsert("INSERT INTO crew_member VALUES (:id,:p_name,:p_dept,:p_spec)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("p_name",tbName.Text), new OracleParameter("p_dept",cbDept.Text), new OracleParameter("p_spec",tbSpec.Text) }, frm))
                {
                    AutoGenerateUser("Crew", tbName.Text, tbId.Text);
                }
            };
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Cast Role ────────────────────────────────────────────────
        private void ShowAddCastRoleForm()
        {
            var (frm, flow) = MakeAddForm("Add Cast Role / Send Offer");
            flow.Controls.Add(new Label { Text = "Add Cast Role", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Role ID"));        var tbId     = MakeFrmTextBox("e.g. 16");    flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Movie ID"));       var tbMovId  = MakeFrmTextBox("1-15");       flow.Controls.Add(tbMovId);
            flow.Controls.Add(MakeFrmLabel("Actor ID"));       var tbActId  = MakeFrmTextBox("1-15");       flow.Controls.Add(tbActId);
            flow.Controls.Add(MakeFrmLabel("Character Name")); var tbChar   = MakeFrmTextBox();             flow.Controls.Add(tbChar);
            flow.Controls.Add(MakeFrmLabel("Role Type"));
            var cbType = MakeFrmCombo(new[] { "Lead","Supporting","Cameo","Narrator" });
            flow.Controls.Add(cbType);
            flow.Controls.Add(MakeFrmLabel("Billing Order"));  var tbBill   = MakeFrmTextBox("e.g. 1");     flow.Controls.Add(tbBill);

            var btnSave = MakeFrmSaveBtn("Send Offer");
            btnSave.Click += (s, e) => RunInsert(
                "INSERT INTO cast_role (role_id,movie_id,actor_id,character_name,role_type,billing_order,acceptance_status) VALUES (:id,:mov,:act,:p_char,:p_type,:bill,'Pending')",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("mov",tbMovId.Text), new OracleParameter("act",tbActId.Text), new OracleParameter("p_char",tbChar.Text), new OracleParameter("p_type",cbType.Text), new OracleParameter("bill",tbBill.Text) }, frm);
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Contract ─────────────────────────────────────────────────
        private void ShowAddContractForm()
        {
            var (frm, flow) = MakeAddForm("Add New Contract");
            flow.Controls.Add(new Label { Text = "Add Contract", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Contract ID"));    var tbId    = MakeFrmTextBox("e.g. 16");      flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Movie ID"));       var tbMovId = MakeFrmTextBox("1-15");         flow.Controls.Add(tbMovId);
            flow.Controls.Add(MakeFrmLabel("Party ID"));       var tbParty = MakeFrmTextBox("actor/crew ID"); flow.Controls.Add(tbParty);
            flow.Controls.Add(MakeFrmLabel("Party Type"));
            var cbPartyType = MakeFrmCombo(new[] { "Actor","Director","Crew","Vendor" });
            flow.Controls.Add(cbPartyType);
            flow.Controls.Add(MakeFrmLabel("Signed Date (DD-MM-YYYY)"));  var tbSign  = MakeFrmTextBox("e.g. 01-01-2025"); flow.Controls.Add(tbSign);
            flow.Controls.Add(MakeFrmLabel("Expiry Date (DD-MM-YYYY)"));  var tbExp   = MakeFrmTextBox("e.g. 31-12-2025"); flow.Controls.Add(tbExp);
            flow.Controls.Add(MakeFrmLabel("Total Value (PKR)"));          var tbVal   = MakeFrmTextBox("e.g. 10000000");   flow.Controls.Add(tbVal);
            flow.Controls.Add(MakeFrmLabel("Terms Summary"));              var tbTerms = MakeFrmTextBox("Terms...");         flow.Controls.Add(tbTerms);

            var btnSave = MakeFrmSaveBtn("Add Contract");
            btnSave.Click += (s, e) => RunInsert(
                "INSERT INTO contract VALUES (:id,:mov,:p_party,:p_ptype,TO_DATE(:p_sign,'DD-MM-YYYY'),TO_DATE(:p_exp,'DD-MM-YYYY'),:p_val,:p_terms)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("mov",tbMovId.Text), new OracleParameter("p_party",tbParty.Text), new OracleParameter("p_ptype",cbPartyType.Text), new OracleParameter("p_sign",tbSign.Text), new OracleParameter("p_exp",tbExp.Text), new OracleParameter("p_val",tbVal.Text), new OracleParameter("p_terms",tbTerms.Text) }, frm);
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Location ─────────────────────────────────────────────────
        private void ShowAddLocationForm()
        {
            var (frm, flow) = MakeAddForm("Add Shooting Location");
            flow.Controls.Add(new Label { Text = "Add Location", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Location ID"));   var tbId   = MakeFrmTextBox("e.g. 16");          flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Name"));           var tbName = MakeFrmTextBox();                   flow.Controls.Add(tbName);
            flow.Controls.Add(MakeFrmLabel("Address"));        var tbAddr = MakeFrmTextBox();                   flow.Controls.Add(tbAddr);
            flow.Controls.Add(MakeFrmLabel("Type"));           var cbType = MakeFrmCombo(new[] { "Indoor","Outdoor" }); flow.Controls.Add(cbType);
            flow.Controls.Add(MakeFrmLabel("Country"));        var tbCoun = MakeFrmTextBox("Pakistan");          flow.Controls.Add(tbCoun);
            flow.Controls.Add(MakeFrmLabel("Daily Rate (PKR)")); var tbRate = MakeFrmTextBox("e.g. 50000");    flow.Controls.Add(tbRate);

            var btnSave = MakeFrmSaveBtn("Add Location");
            btnSave.Click += (s, e) => RunInsert(
                "INSERT INTO shooting_location VALUES (:id,:p_name,:addr,:p_type,:coun,:rate)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("p_name",tbName.Text), new OracleParameter("addr",tbAddr.Text), new OracleParameter("p_type",cbType.Text), new OracleParameter("coun",tbCoun.Text), new OracleParameter("rate",tbRate.Text) }, frm);
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Schedule ─────────────────────────────────────────────────
        private void ShowAddScheduleForm()
        {
            var (frm, flow) = MakeAddForm("Add Schedule Entry");
            flow.Controls.Add(new Label { Text = "Add Schedule", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Schedule ID"));   var tbId    = MakeFrmTextBox("e.g. 16");                   flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Movie ID"));      var tbMovId = MakeFrmTextBox("1-15");                      flow.Controls.Add(tbMovId);
            flow.Controls.Add(MakeFrmLabel("Scene ID"));      var tbScId  = MakeFrmTextBox("1-20");                      flow.Controls.Add(tbScId);
            flow.Controls.Add(MakeFrmLabel("Location ID"));   var tbLocId = MakeFrmTextBox("1-15");                      flow.Controls.Add(tbLocId);
            flow.Controls.Add(MakeFrmLabel("Shoot Date (DD-MM-YYYY)")); var tbDate = MakeFrmTextBox("e.g. 15-06-2025"); flow.Controls.Add(tbDate);
            flow.Controls.Add(MakeFrmLabel("Start Time (HH24:MI)"));   var tbStart = MakeFrmTextBox("e.g. 08:00");       flow.Controls.Add(tbStart);
            flow.Controls.Add(MakeFrmLabel("End Time (HH24:MI)"));     var tbEnd   = MakeFrmTextBox("e.g. 18:00");       flow.Controls.Add(tbEnd);
            flow.Controls.Add(MakeFrmLabel("Status"));
            var cbStatus = MakeFrmCombo(new[] { "Scheduled","In Progress","Completed","Postponed" });
            flow.Controls.Add(cbStatus);

            var btnSave = MakeFrmSaveBtn("Add Schedule");
            btnSave.Click += (s, e) => RunInsert(
                @"INSERT INTO schedule VALUES (:id,:mov,:sc,:loc,TO_DATE(:dt,'DD-MM-YYYY'),
                  TO_DATE(:dt||' '||:p_st,'DD-MM-YYYY HH24:MI'),
                  TO_DATE(:dt||' '||:p_en,'DD-MM-YYYY HH24:MI'),:p_status)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("mov",tbMovId.Text), new OracleParameter("sc",tbScId.Text), new OracleParameter("loc",tbLocId.Text), new OracleParameter("dt",tbDate.Text), new OracleParameter("p_st",tbStart.Text), new OracleParameter("p_en",tbEnd.Text), new OracleParameter("p_status",cbStatus.Text) }, frm);
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Scene ────────────────────────────────────────────────────
        private void ShowAddSceneForm()
        {
            var (frm, flow) = MakeAddForm("Add Scene");
            flow.Controls.Add(new Label { Text = "Add Scene", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Scene ID"));      var tbId    = MakeFrmTextBox("e.g. 21");     flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Movie ID"));      var tbMovId = MakeFrmTextBox("1-15");        flow.Controls.Add(tbMovId);
            flow.Controls.Add(MakeFrmLabel("Location ID"));   var tbLocId = MakeFrmTextBox("1-15");        flow.Controls.Add(tbLocId);
            flow.Controls.Add(MakeFrmLabel("Scene Number"));  var tbNo    = MakeFrmTextBox("e.g. 1");      flow.Controls.Add(tbNo);
            flow.Controls.Add(MakeFrmLabel("Description"));   var tbDesc  = MakeFrmTextBox();              flow.Controls.Add(tbDesc);
            flow.Controls.Add(MakeFrmLabel("Duration (min)")); var tbDur  = MakeFrmTextBox("e.g. 15.0");  flow.Controls.Add(tbDur);
            flow.Controls.Add(MakeFrmLabel("Status"));
            var cbStatus = MakeFrmCombo(new[] { "Planned","Shooting","Completed","Cancelled" });
            flow.Controls.Add(cbStatus);

            var btnSave = MakeFrmSaveBtn("Add Scene");
            btnSave.Click += (s, e) => RunInsert(
                "INSERT INTO scene VALUES (:id,:mov,:loc,:p_no,:p_desc,:dur,:p_status)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("mov",tbMovId.Text), new OracleParameter("loc",tbLocId.Text), new OracleParameter("p_no",tbNo.Text), new OracleParameter("p_desc",tbDesc.Text), new OracleParameter("dur",tbDur.Text), new OracleParameter("p_status",cbStatus.Text) }, frm);
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add Crew Assignment ──────────────────────────────────────────
        private void ShowAddCrewAssignmentForm()
        {
            var (frm, flow) = MakeAddForm("Add Crew Assignment");
            flow.Controls.Add(new Label { Text = "Assign Crew to Movie", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("Assignment ID")); var tbId    = MakeFrmTextBox("e.g. 16");          flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Movie ID"));      var tbMovId = MakeFrmTextBox("1-15");             flow.Controls.Add(tbMovId);
            flow.Controls.Add(MakeFrmLabel("Crew ID"));       var tbCrew  = MakeFrmTextBox("1-15");             flow.Controls.Add(tbCrew);
            flow.Controls.Add(MakeFrmLabel("Job Title"));     var tbJob   = MakeFrmTextBox("e.g. Gaffer");      flow.Controls.Add(tbJob);
            flow.Controls.Add(MakeFrmLabel("Start Date (DD-MM-YYYY)")); var tbStart = MakeFrmTextBox("e.g. 01-01-2025"); flow.Controls.Add(tbStart);
            flow.Controls.Add(MakeFrmLabel("End Date (DD-MM-YYYY)"));   var tbEnd   = MakeFrmTextBox("e.g. 31-12-2025"); flow.Controls.Add(tbEnd);

            var btnSave = MakeFrmSaveBtn("Assign Crew");
            btnSave.Click += (s, e) => RunInsert(
                "INSERT INTO crew_assignment VALUES (:id,:mov,:p_crew,:p_job,TO_DATE(:p_start,'DD-MM-YYYY'),TO_DATE(:p_end,'DD-MM-YYYY'))",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("mov",tbMovId.Text), new OracleParameter("p_crew",tbCrew.Text), new OracleParameter("p_job",tbJob.Text), new OracleParameter("p_start",tbStart.Text), new OracleParameter("p_end",tbEnd.Text) }, frm);
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ── Add User ─────────────────────────────────────────────────────
        private void ShowAddUserForm()
        {
            var (frm, flow) = MakeAddForm("Add System User");
            flow.Controls.Add(new Label { Text = "Add System User", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TextPri, AutoSize = true, Margin = new Padding(0, 0, 0, 10) });

            flow.Controls.Add(MakeFrmLabel("User ID"));   var tbId   = MakeFrmTextBox("e.g. 400");               flow.Controls.Add(tbId);
            flow.Controls.Add(MakeFrmLabel("Party ID"));  var tbPid  = MakeFrmTextBox("director/actor/crew ID"); flow.Controls.Add(tbPid);
            flow.Controls.Add(MakeFrmLabel("Role"));      var cbRole = MakeFrmCombo(new[] { "Admin","Director","Actor","Crew" }); flow.Controls.Add(cbRole);
            flow.Controls.Add(MakeFrmLabel("Username"));  var tbUser = MakeFrmTextBox();                          flow.Controls.Add(tbUser);
            flow.Controls.Add(MakeFrmLabel("Password"));  var tbPass = MakeFrmTextBox();                          flow.Controls.Add(tbPass);
            flow.Controls.Add(MakeFrmLabel("Full Name")); var tbName = MakeFrmTextBox();                          flow.Controls.Add(tbName);

            var btnSave = MakeFrmSaveBtn("Add User");
            btnSave.Click += (s, e) => RunInsert(
                "INSERT INTO app_user VALUES (:id,:p_pid,:p_role,:p_user,:p_pass,:p_name)",
                new[] { new OracleParameter("id",tbId.Text), new OracleParameter("p_pid",tbPid.Text), new OracleParameter("p_role",cbRole.Text), new OracleParameter("p_user",tbUser.Text), new OracleParameter("p_pass",tbPass.Text), new OracleParameter("p_name",tbName.Text) }, frm);
            flow.Controls.Add(btnSave);
            frm.ShowDialog(this);
        }

        // ════════════════════════════════════════════════════════════════
        // DELETE
        // ════════════════════════════════════════════════════════════════
        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count == 0) { MessageBox.Show("Select a row to delete.", "Info"); return; }
            var item = navItems[selectedNavIndex];
            if (string.IsNullOrEmpty(item.TableName)) { MessageBox.Show("This view cannot be modified directly.", "Info"); return; }

            var confirm = MessageBox.Show("Delete the selected row from the database?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                string pkCol = item.TableName switch {
                    "movie" => "movie_id",
                    "director" => "director_id",
                    "actor" => "actor_id",
                    "crew_member" => "crew_id",
                    "cast_role" => "role_id",
                    "contract" => "contract_id",
                    "shooting_location" => "location_id",
                    "schedule" => "schedule_id",
                    "scene" => "scene_id",
                    "crew_assignment" => "assignment_id",
                    "app_user" => "user_id",
                    _ => ""
                };

                if (pkCol == "") {
                    MessageBox.Show("Delete is not configured for this table.");
                    return;
                }

                object idValue = dataGrid.SelectedRows[0].Cells[0].Value;
                
                DatabaseHelper.ExecuteNonQuery($"DELETE FROM {item.TableName} WHERE {pkCol} = :id", new[] { new OracleParameter("id", idValue.ToString()) });
                
                MessageBox.Show("Record deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(selectedNavIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // SAVE (inline edits)
        // ════════════════════════════════════════════════════════════════
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var item = navItems[selectedNavIndex];
            if (string.IsNullOrEmpty(item.TableName)) { MessageBox.Show("This view cannot be modified directly.", "Info"); return; }
            try
            {
                dataGrid.EndEdit();
                // Need a CommandBuilder for the raw table adapter to generate UPDATE
                new OracleCommandBuilder(_adapter);
                _adapter.Update(_dataTable);
                MessageBox.Show("Changes saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(selectedNavIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed. Note: joined views cannot be edited inline — use the Add button instead.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // ACTOR — ACCEPT / DECLINE OFFER
        // ════════════════════════════════════════════════════════════════
        private void BtnAcceptRole_Click(object? sender, EventArgs e) => UpdateOfferStatus("Accepted");
        private void BtnDeclineRole_Click(object? sender, EventArgs e) => UpdateOfferStatus("Declined");

        private void UpdateOfferStatus(string status)
        {
            if (dataGrid.SelectedRows.Count == 0) { MessageBox.Show("Select an offer row first.", "Info"); return; }
            var row = dataGrid.SelectedRows[0];
            object? roleId = row.Cells["Role ID"].Value;
            if (roleId == null) return;

            string confirmMsg = status == "Accepted"
                ? $"Accept the role offer for '{row.Cells["Character"].Value}'?"
                : $"Decline the role offer for '{row.Cells["Character"].Value}'?";

            if (MessageBox.Show(confirmMsg, $"{status} Offer", MessageBoxButtons.YesNo,
                status == "Accepted" ? MessageBoxIcon.Question : MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE cast_role SET acceptance_status = :s WHERE role_id = :id",
                    new[] { new OracleParameter("s", status), new OracleParameter("id", roleId.ToString()) });
                MessageBox.Show($"Offer {status.ToLower()} successfully!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(selectedNavIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}