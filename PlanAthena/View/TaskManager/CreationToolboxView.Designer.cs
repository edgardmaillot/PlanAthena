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
            headerToolbox = new KryptonHeaderGroup();
            panelDynamicButtons = new FlowLayoutPanel();
            panelStaticButtons = new KryptonPanel();
            btnAddBloc = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)headerToolbox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerToolbox.Panel).BeginInit();
            headerToolbox.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelStaticButtons).BeginInit();
            panelStaticButtons.SuspendLayout();
            SuspendLayout();
            // 
            // headerToolbox
            // 
            headerToolbox.Dock = DockStyle.Fill;
            headerToolbox.HeaderVisiblePrimary = false;
            headerToolbox.HeaderVisibleSecondary = false;
            headerToolbox.Location = new Point(0, 0);
            headerToolbox.Margin = new Padding(4, 3, 4, 3);
            // 
            // 
            // 
            headerToolbox.Panel.Controls.Add(panelDynamicButtons);
            headerToolbox.Panel.Controls.Add(panelStaticButtons);
            headerToolbox.Size = new Size(233, 692);
            headerToolbox.TabIndex = 0;
            // 
            // panelDynamicButtons
            // 
            panelDynamicButtons.AutoScroll = true;
            panelDynamicButtons.BackColor = Color.Transparent;
            panelDynamicButtons.Dock = DockStyle.Fill;
            panelDynamicButtons.FlowDirection = FlowDirection.TopDown;
            panelDynamicButtons.Location = new Point(0, 85);
            panelDynamicButtons.Margin = new Padding(4, 3, 4, 3);
            panelDynamicButtons.Name = "panelDynamicButtons";
            panelDynamicButtons.Padding = new Padding(12, 6, 12, 6);
            panelDynamicButtons.Size = new Size(231, 605);
            panelDynamicButtons.TabIndex = 1;
            panelDynamicButtons.WrapContents = false;
            // 
            // panelStaticButtons
            // 
            panelStaticButtons.Controls.Add(btnAddBloc);
            panelStaticButtons.Dock = DockStyle.Top;
            panelStaticButtons.Location = new Point(0, 0);
            panelStaticButtons.Margin = new Padding(4, 3, 4, 3);
            panelStaticButtons.Name = "panelStaticButtons";
            panelStaticButtons.Size = new Size(231, 85);
            panelStaticButtons.TabIndex = 0;
            // 
            // btnAddBloc
            // 
            btnAddBloc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnAddBloc.Location = new Point(18, 17);
            btnAddBloc.Margin = new Padding(4, 3, 4, 3);
            btnAddBloc.Name = "btnAddBloc";
            btnAddBloc.Size = new Size(196, 46);
            btnAddBloc.TabIndex = 0;
            btnAddBloc.Values.DropDownArrowColor = Color.Empty;
            btnAddBloc.Values.Text = "Ajouter un Bloc";
            btnAddBloc.Click += btnAddBloc_Click;
            // 
            // CreationToolboxView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(headerToolbox);
            Margin = new Padding(4, 3, 4, 3);
            Name = "CreationToolboxView";
            Size = new Size(233, 692);
            ((System.ComponentModel.ISupportInitialize)headerToolbox.Panel).EndInit();
            headerToolbox.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)headerToolbox).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelStaticButtons).EndInit();
            panelStaticButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private KryptonHeaderGroup headerToolbox;
        private System.Windows.Forms.FlowLayoutPanel panelDynamicButtons;
        private KryptonPanel panelStaticButtons;
        private KryptonButton btnAddBloc;
    }
}