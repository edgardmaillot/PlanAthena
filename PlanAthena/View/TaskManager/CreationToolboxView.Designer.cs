using Krypton.Toolkit;

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
            this.headerToolbox = new Krypton.Toolkit.KryptonHeaderGroup();
            this.panelDynamicButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.panelStaticButtons = new Krypton.Toolkit.KryptonPanel();
            this.btnAddBloc = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.headerToolbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.headerToolbox.Panel)).BeginInit();
            this.headerToolbox.Panel.SuspendLayout();
            this.headerToolbox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelStaticButtons)).BeginInit();
            this.panelStaticButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerToolbox
            // 
            this.headerToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerToolbox.HeaderVisibleSecondary = false;
            this.headerToolbox.Location = new System.Drawing.Point(0, 0);
            this.headerToolbox.Name = "headerToolbox";
            // 
            // headerToolbox.Panel
            // 
            this.headerToolbox.Panel.Controls.Add(this.panelDynamicButtons);
            this.headerToolbox.Panel.Controls.Add(this.panelStaticButtons);
            this.headerToolbox.Size = new System.Drawing.Size(200, 600);
            this.headerToolbox.TabIndex = 0;
            this.headerToolbox.ValuesPrimary.Heading = "Outils de Création";
            this.headerToolbox.ValuesPrimary.Image = null;
            // 
            // panelDynamicButtons
            // 
            this.panelDynamicButtons.AutoScroll = true;
            this.panelDynamicButtons.BackColor = System.Drawing.Color.Transparent;
            this.panelDynamicButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDynamicButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panelDynamicButtons.Location = new System.Drawing.Point(0, 70);
            this.panelDynamicButtons.Name = "panelDynamicButtons";
            this.panelDynamicButtons.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.panelDynamicButtons.Size = new System.Drawing.Size(198, 499);
            this.panelDynamicButtons.TabIndex = 1;
            this.panelDynamicButtons.WrapContents = false;
            // 
            // panelStaticButtons
            // 
            this.panelStaticButtons.Controls.Add(this.btnAddBloc);
            this.panelStaticButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelStaticButtons.Location = new System.Drawing.Point(0, 0);
            this.panelStaticButtons.Name = "panelStaticButtons";
            this.panelStaticButtons.Size = new System.Drawing.Size(198, 70);
            this.panelStaticButtons.TabIndex = 0;
            // 
            // btnAddBloc
            // 
            this.btnAddBloc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddBloc.Location = new System.Drawing.Point(15, 15);
            this.btnAddBloc.Name = "btnAddBloc";
            this.btnAddBloc.Size = new System.Drawing.Size(168, 40);
            this.btnAddBloc.TabIndex = 0;
            this.btnAddBloc.Values.Text = "Ajouter un Bloc";
            this.btnAddBloc.Click += new System.EventHandler(this.btnAddBloc_Click);
            // 
            // CreationToolboxView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.headerToolbox);
            this.Name = "CreationToolboxView";
            this.Size = new System.Drawing.Size(200, 600);
            ((System.ComponentModel.ISupportInitialize)(this.headerToolbox.Panel)).EndInit();
            this.headerToolbox.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.headerToolbox)).EndInit();
            this.headerToolbox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelStaticButtons)).EndInit();
            this.panelStaticButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private KryptonHeaderGroup headerToolbox;
        private System.Windows.Forms.FlowLayoutPanel panelDynamicButtons;
        private KryptonPanel panelStaticButtons;
        private KryptonButton btnAddBloc;
    }
}