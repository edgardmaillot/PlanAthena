// --- START OF FILE CreationToolboxView.Designer.cs ---

using Krypton.Toolkit;
using System.Drawing;
using System.Windows.Forms;

namespace PlanAthena.View.TaskManager
{
    partial class CreationToolboxView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            panelMain = new KryptonPanel();
            panelDynamicButtons = new TableLayoutPanel();
            headerMetiers = new KryptonHeader();
            btnAddBloc = new KryptonButton();
            btnCreerJalon = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)panelMain).BeginInit();
            panelMain.SuspendLayout();
            SuspendLayout();
            // 
            // panelMain
            // 
            panelMain.Controls.Add(panelDynamicButtons);
            panelMain.Controls.Add(headerMetiers);
            panelMain.Controls.Add(btnAddBloc);
            panelMain.Controls.Add(btnCreerJalon);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 0);
            panelMain.Margin = new Padding(0);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(180, 700);
            panelMain.TabIndex = 0;
            // 
            // panelDynamicButtons
            // 
            panelDynamicButtons.AutoScroll = true;
            panelDynamicButtons.BackColor = Color.Transparent;
            panelDynamicButtons.ColumnCount = 2;
            panelDynamicButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F));
            panelDynamicButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelDynamicButtons.Dock = DockStyle.Fill;
            panelDynamicButtons.Location = new Point(0, 68);
            panelDynamicButtons.Margin = new Padding(0);
            panelDynamicButtons.Name = "panelDynamicButtons";
            panelDynamicButtons.Size = new Size(180, 586);
            panelDynamicButtons.TabIndex = 2;
            // 
            // headerMetiers
            // 
            headerMetiers.Dock = DockStyle.Top;
            headerMetiers.Location = new Point(0, 46);
            headerMetiers.Name = "headerMetiers";
            headerMetiers.Size = new Size(180, 22);
            headerMetiers.StateCommon.Content.ShortText.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            headerMetiers.TabIndex = 1;
            headerMetiers.Values.Description = "";
            headerMetiers.Values.Heading = "Créer Tâches";
            headerMetiers.Values.Image = null;
            // 
            // btnAddBloc
            // 
            btnAddBloc.Dock = DockStyle.Top;
            btnAddBloc.Location = new Point(0, 0);
            btnAddBloc.Margin = new Padding(4, 3, 4, 3);
            btnAddBloc.Name = "btnAddBloc";
            btnAddBloc.Size = new Size(180, 46);
            btnAddBloc.TabIndex = 0;
            btnAddBloc.Values.DropDownArrowColor = Color.Empty;
            btnAddBloc.Values.Text = "◻ Ajouter un Bloc";
            btnAddBloc.Values.UACShieldIconSize = UACShieldIconSize.Small;
            btnAddBloc.Click += btnAddBloc_Click;
            // 
            // btnCreerJalon
            // 
            btnCreerJalon.Dock = DockStyle.Bottom;
            btnCreerJalon.Location = new Point(0, 654);
            btnCreerJalon.Margin = new Padding(4, 3, 4, 3);
            btnCreerJalon.Name = "btnCreerJalon";
            btnCreerJalon.Size = new Size(180, 46);
            btnCreerJalon.TabIndex = 3;
            btnCreerJalon.Values.DropDownArrowColor = Color.Empty;
            btnCreerJalon.Values.Text = " ◇ Créer Jalon";
            btnCreerJalon.Values.UACShieldIconSize = UACShieldIconSize.Medium;
            btnCreerJalon.Click += btnCreerJalon_Click;
            // 
            // CreationToolboxView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelMain);
            Margin = new Padding(4, 3, 4, 3);
            Name = "CreationToolboxView";
            Size = new Size(180, 700);
            ((System.ComponentModel.ISupportInitialize)panelMain).EndInit();
            panelMain.ResumeLayout(false);
            panelMain.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private KryptonPanel panelMain;
        private KryptonButton btnAddBloc;
        private TableLayoutPanel panelDynamicButtons;
        private KryptonHeader headerMetiers;
        private KryptonButton btnCreerJalon;
    }
}