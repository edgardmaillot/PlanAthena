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
            this.panelMain = new Krypton.Toolkit.KryptonPanel();
            this.panelDynamicButtons = new System.Windows.Forms.TableLayoutPanel();
            this.headerMetiers = new Krypton.Toolkit.KryptonHeader();
            this.btnAddBloc = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.panelMain)).BeginInit();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.panelDynamicButtons);
            this.panelMain.Controls.Add(this.headerMetiers);
            this.panelMain.Controls.Add(this.btnAddBloc);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Margin = new System.Windows.Forms.Padding(0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(180, 700);
            this.panelMain.TabIndex = 0;
            // 
            // panelDynamicButtons
            // 
            this.panelDynamicButtons.AutoScroll = true;
            this.panelDynamicButtons.BackColor = System.Drawing.Color.Transparent;
            this.panelDynamicButtons.ColumnCount = 2;
            this.panelDynamicButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.panelDynamicButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelDynamicButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDynamicButtons.Location = new System.Drawing.Point(0, 76);
            this.panelDynamicButtons.Margin = new System.Windows.Forms.Padding(0);
            this.panelDynamicButtons.Name = "panelDynamicButtons";
            this.panelDynamicButtons.RowCount = 0;
            this.panelDynamicButtons.Size = new System.Drawing.Size(180, 624);
            this.panelDynamicButtons.TabIndex = 2;
            // 
            // headerMetiers
            // 
            this.headerMetiers.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerMetiers.Location = new System.Drawing.Point(0, 46);
            this.headerMetiers.Name = "headerMetiers";
            this.headerMetiers.Size = new System.Drawing.Size(180, 30);
            this.headerMetiers.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerMetiers.TabIndex = 1;
            this.headerMetiers.Values.Description = "";
            this.headerMetiers.Values.Heading = "Créer Tâches";
            this.headerMetiers.Values.Image = null;
            // 
            // btnAddBloc
            // 
            this.btnAddBloc.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAddBloc.Location = new System.Drawing.Point(0, 0);
            this.btnAddBloc.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnAddBloc.Name = "btnAddBloc";
            this.btnAddBloc.Size = new System.Drawing.Size(180, 46);
            this.btnAddBloc.TabIndex = 0;
            this.btnAddBloc.Values.Text = "Ajouter un Bloc";
            this.btnAddBloc.Click += new System.EventHandler(this.btnAddBloc_Click);
            // 
            // CreationToolboxView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelMain);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CreationToolboxView";
            this.Size = new System.Drawing.Size(180, 700);
            ((System.ComponentModel.ISupportInitialize)(this.panelMain)).EndInit();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private KryptonPanel panelMain;
        private KryptonButton btnAddBloc;
        private TableLayoutPanel panelDynamicButtons;
        private KryptonHeader headerMetiers; // <-- NOUVEAU
    }
}