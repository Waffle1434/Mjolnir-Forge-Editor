namespace ForgeAssistant
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.lSelected = new System.Windows.Forms.Label();
            this.lIndex = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.nSpawnTime = new System.Windows.Forms.NumericUpDown();
            this.lSpawnTime = new System.Windows.Forms.Label();
            this.lMap = new System.Windows.Forms.Label();
            this.lTotal = new System.Windows.Forms.Label();
            this.lMovement = new System.Windows.Forms.Label();
            this.contextMenu = new System.Windows.Forms.MenuStrip();
            this.copyRotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteRotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteLocationAndRotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copymulti = new System.Windows.Forms.ToolStripMenuItem();
            this.pastemulti = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteLocationMultiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteRotationMultiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteLocationAndRotationMultiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateStructureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateAroundYawZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateAroundYpitchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateAroundXrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemList = new ListViewNF();
            this.colId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pRotation = new System.Windows.Forms.Panel();
            this.nRotInc = new System.Windows.Forms.NumericUpDown();
            this.lIncrement = new System.Windows.Forms.Label();
            this.pEuler = new System.Windows.Forms.Panel();
            this.Roll = new System.Windows.Forms.NumericUpDown();
            this.lPitch = new System.Windows.Forms.Label();
            this.Pitch = new System.Windows.Forms.NumericUpDown();
            this.lYaw = new System.Windows.Forms.Label();
            this.Yaw = new System.Windows.Forms.NumericUpDown();
            this.lRoll = new System.Windows.Forms.Label();
            this.pRotationMatrix = new System.Windows.Forms.Panel();
            this.n33 = new System.Windows.Forms.NumericUpDown();
            this.lRotationMatrix = new System.Windows.Forms.Label();
            this.n23 = new System.Windows.Forms.NumericUpDown();
            this.n13 = new System.Windows.Forms.NumericUpDown();
            this.n32 = new System.Windows.Forms.NumericUpDown();
            this.n22 = new System.Windows.Forms.NumericUpDown();
            this.n12 = new System.Windows.Forms.NumericUpDown();
            this.n31 = new System.Windows.Forms.NumericUpDown();
            this.n21 = new System.Windows.Forms.NumericUpDown();
            this.n11 = new System.Windows.Forms.NumericUpDown();
            this.lRotation = new System.Windows.Forms.Label();
            this.pMovement = new System.Windows.Forms.Panel();
            this.pMoveXYZ = new System.Windows.Forms.Panel();
            this.lMoveX = new System.Windows.Forms.Label();
            this.nMoveX = new System.Windows.Forms.NumericUpDown();
            this.lMoveY = new System.Windows.Forms.Label();
            this.nMoveY = new System.Windows.Forms.NumericUpDown();
            this.lMoveZ = new System.Windows.Forms.Label();
            this.nMoveZ = new System.Windows.Forms.NumericUpDown();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lX = new System.Windows.Forms.Label();
            this.nX = new System.Windows.Forms.NumericUpDown();
            this.lY = new System.Windows.Forms.Label();
            this.nY = new System.Windows.Forms.NumericUpDown();
            this.nZ = new System.Windows.Forms.NumericUpDown();
            this.lZ = new System.Windows.Forms.Label();
            this.lSpawnPoint = new System.Windows.Forms.Label();
            this.cbManualEdit = new System.Windows.Forms.CheckBox();
            this.nMoveInc = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.bTeleport = new System.Windows.Forms.Button();
            this.editProgress = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.attachToProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateFrequencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.msToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.keepInForegroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nSpawnTime)).BeginInit();
            this.contextMenu.SuspendLayout();
            this.pRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nRotInc)).BeginInit();
            this.pEuler.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Roll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Yaw)).BeginInit();
            this.pRotationMatrix.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.n33)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n23)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n13)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n32)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n31)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n11)).BeginInit();
            this.pMovement.SuspendLayout();
            this.pMoveXYZ.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveZ)).BeginInit();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveInc)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.lSelected);
            this.panel1.Controls.Add(this.lIndex);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.progressBar2);
            this.panel1.Controls.Add(this.nSpawnTime);
            this.panel1.Controls.Add(this.lSpawnTime);
            this.panel1.Controls.Add(this.lMap);
            this.panel1.Controls.Add(this.lTotal);
            this.panel1.Controls.Add(this.lMovement);
            this.panel1.Controls.Add(this.itemList);
            this.panel1.Controls.Add(this.contextMenu);
            this.panel1.Controls.Add(this.pRotation);
            this.panel1.Controls.Add(this.lRotation);
            this.panel1.Controls.Add(this.pMovement);
            this.panel1.Controls.Add(this.bTeleport);
            this.panel1.Controls.Add(this.editProgress);
            this.panel1.Location = new System.Drawing.Point(12, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(347, 564);
            this.panel1.TabIndex = 0;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(208, 204);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 47;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // lSelected
            // 
            this.lSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lSelected.AutoSize = true;
            this.lSelected.Location = new System.Drawing.Point(298, 215);
            this.lSelected.Name = "lSelected";
            this.lSelected.Size = new System.Drawing.Size(64, 13);
            this.lSelected.TabIndex = 46;
            this.lSelected.Text = "Selected : 0";
            // 
            // lIndex
            // 
            this.lIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lIndex.AutoSize = true;
            this.lIndex.Location = new System.Drawing.Point(298, 201);
            this.lIndex.Name = "lIndex";
            this.lIndex.Size = new System.Drawing.Size(48, 13);
            this.lIndex.TabIndex = 45;
            this.lIndex.Text = "Index : 0";
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(3, 212);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 18);
            this.button2.TabIndex = 44;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(157, 538);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(182, 18);
            this.progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar2.TabIndex = 43;
            this.progressBar2.Visible = false;
            // 
            // nSpawnTime
            // 
            this.nSpawnTime.Location = new System.Drawing.Point(85, 191);
            this.nSpawnTime.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nSpawnTime.Name = "nSpawnTime";
            this.nSpawnTime.Size = new System.Drawing.Size(51, 20);
            this.nSpawnTime.TabIndex = 41;
            this.nSpawnTime.ValueChanged += new System.EventHandler(this.nSpawnTime_ValueChanged);
            // 
            // lSpawnTime
            // 
            this.lSpawnTime.AutoSize = true;
            this.lSpawnTime.Location = new System.Drawing.Point(4, 193);
            this.lSpawnTime.Name = "lSpawnTime";
            this.lSpawnTime.Size = new System.Drawing.Size(75, 13);
            this.lSpawnTime.TabIndex = 40;
            this.lSpawnTime.Text = "Spawn Time : ";
            // 
            // lMap
            // 
            this.lMap.AutoSize = true;
            this.lMap.Location = new System.Drawing.Point(114, 538);
            this.lMap.Name = "lMap";
            this.lMap.Size = new System.Drawing.Size(37, 13);
            this.lMap.TabIndex = 38;
            this.lMap.Text = "Map : ";
            // 
            // lTotal
            // 
            this.lTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lTotal.AutoSize = true;
            this.lTotal.Location = new System.Drawing.Point(314, 188);
            this.lTotal.Name = "lTotal";
            this.lTotal.Size = new System.Drawing.Size(46, 13);
            this.lTotal.TabIndex = 36;
            this.lTotal.Text = "Total : 0";
            this.lTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lMovement
            // 
            this.lMovement.AutoSize = true;
            this.lMovement.Location = new System.Drawing.Point(145, 217);
            this.lMovement.Name = "lMovement";
            this.lMovement.Size = new System.Drawing.Size(57, 13);
            this.lMovement.TabIndex = 31;
            this.lMovement.Text = "Movement";
            // 
            // contextMenu
            // 
            this.contextMenu.AutoSize = false;
            this.contextMenu.Dock = System.Windows.Forms.DockStyle.None;
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyRotationToolStripMenuItem,
            this.pasteLocationToolStripMenuItem,
            this.pasteRotationToolStripMenuItem,
            this.pasteLocationAndRotationToolStripMenuItem,
            this.copymulti,
            this.pastemulti,
            this.rotateStructureToolStripMenuItem});
            this.contextMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.contextMenu.Location = new System.Drawing.Point(117, 33);
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(179, 142);
            this.contextMenu.Stretch = false;
            this.contextMenu.TabIndex = 37;
            this.contextMenu.Text = "menuStrip2";
            this.contextMenu.Visible = false;
            // 
            // copyRotationToolStripMenuItem
            // 
            this.copyRotationToolStripMenuItem.AutoSize = false;
            this.copyRotationToolStripMenuItem.Name = "copyRotationToolStripMenuItem";
            this.copyRotationToolStripMenuItem.Size = new System.Drawing.Size(169, 19);
            this.copyRotationToolStripMenuItem.Text = "Copy";
            this.copyRotationToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.copyRotationToolStripMenuItem.Click += new System.EventHandler(this.copyRotationToolStripMenuItem_Click);
            // 
            // pasteLocationToolStripMenuItem
            // 
            this.pasteLocationToolStripMenuItem.AutoSize = false;
            this.pasteLocationToolStripMenuItem.Enabled = false;
            this.pasteLocationToolStripMenuItem.Name = "pasteLocationToolStripMenuItem";
            this.pasteLocationToolStripMenuItem.Size = new System.Drawing.Size(169, 19);
            this.pasteLocationToolStripMenuItem.Text = "Paste Location";
            this.pasteLocationToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pasteLocationToolStripMenuItem.Click += new System.EventHandler(this.pasteLocationToolStripMenuItem_Click);
            // 
            // pasteRotationToolStripMenuItem
            // 
            this.pasteRotationToolStripMenuItem.AutoSize = false;
            this.pasteRotationToolStripMenuItem.Enabled = false;
            this.pasteRotationToolStripMenuItem.Name = "pasteRotationToolStripMenuItem";
            this.pasteRotationToolStripMenuItem.Size = new System.Drawing.Size(169, 19);
            this.pasteRotationToolStripMenuItem.Text = "Paste Rotation";
            this.pasteRotationToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pasteRotationToolStripMenuItem.Click += new System.EventHandler(this.pasteRotationToolStripMenuItem_Click);
            // 
            // pasteLocationAndRotationToolStripMenuItem
            // 
            this.pasteLocationAndRotationToolStripMenuItem.AutoSize = false;
            this.pasteLocationAndRotationToolStripMenuItem.Enabled = false;
            this.pasteLocationAndRotationToolStripMenuItem.Name = "pasteLocationAndRotationToolStripMenuItem";
            this.pasteLocationAndRotationToolStripMenuItem.Size = new System.Drawing.Size(169, 19);
            this.pasteLocationAndRotationToolStripMenuItem.Text = "Paste Location And Rotation";
            this.pasteLocationAndRotationToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pasteLocationAndRotationToolStripMenuItem.Click += new System.EventHandler(this.pasteLocationAndRotationToolStripMenuItem_Click);
            // 
            // copymulti
            // 
            this.copymulti.Name = "copymulti";
            this.copymulti.Size = new System.Drawing.Size(78, 19);
            this.copymulti.Text = "Copy Multi";
            this.copymulti.Click += new System.EventHandler(this.copymulti_Click);
            // 
            // pastemulti
            // 
            this.pastemulti.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteLocationMultiToolStripMenuItem,
            this.pasteRotationMultiToolStripMenuItem,
            this.pasteLocationAndRotationMultiToolStripMenuItem});
            this.pastemulti.Enabled = false;
            this.pastemulti.Name = "pastemulti";
            this.pastemulti.Size = new System.Drawing.Size(78, 19);
            this.pastemulti.Text = "Paste Multi";
            // 
            // pasteLocationMultiToolStripMenuItem
            // 
            this.pasteLocationMultiToolStripMenuItem.Name = "pasteLocationMultiToolStripMenuItem";
            this.pasteLocationMultiToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.pasteLocationMultiToolStripMenuItem.Text = "Paste Location";
            this.pasteLocationMultiToolStripMenuItem.Click += new System.EventHandler(this.pasteLocationMultiToolStripMenuItem_Click);
            // 
            // pasteRotationMultiToolStripMenuItem
            // 
            this.pasteRotationMultiToolStripMenuItem.Name = "pasteRotationMultiToolStripMenuItem";
            this.pasteRotationMultiToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.pasteRotationMultiToolStripMenuItem.Text = "Paste Rotation";
            this.pasteRotationMultiToolStripMenuItem.Click += new System.EventHandler(this.pasteRotationMultiToolStripMenuItem_Click);
            // 
            // pasteLocationAndRotationMultiToolStripMenuItem
            // 
            this.pasteLocationAndRotationMultiToolStripMenuItem.Name = "pasteLocationAndRotationMultiToolStripMenuItem";
            this.pasteLocationAndRotationMultiToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.pasteLocationAndRotationMultiToolStripMenuItem.Text = "Paste Location and Rotation";
            this.pasteLocationAndRotationMultiToolStripMenuItem.Click += new System.EventHandler(this.pasteLocationAndRotationMultiToolStripMenuItem_Click);
            // 
            // rotateStructureToolStripMenuItem
            // 
            this.rotateStructureToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rotateAroundYawZToolStripMenuItem,
            this.rotateAroundYpitchToolStripMenuItem,
            this.rotateAroundXrollToolStripMenuItem});
            this.rotateStructureToolStripMenuItem.Name = "rotateStructureToolStripMenuItem";
            this.rotateStructureToolStripMenuItem.Size = new System.Drawing.Size(104, 19);
            this.rotateStructureToolStripMenuItem.Text = "Rotate Structure";
            // 
            // rotateAroundYawZToolStripMenuItem
            // 
            this.rotateAroundYawZToolStripMenuItem.Name = "rotateAroundYawZToolStripMenuItem";
            this.rotateAroundYawZToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.rotateAroundYawZToolStripMenuItem.Text = "Rotate around Z (yaw)";
            this.rotateAroundYawZToolStripMenuItem.Click += new System.EventHandler(this.rotateAroundYawZToolStripMenuItem_Click);
            // 
            // rotateAroundYpitchToolStripMenuItem
            // 
            this.rotateAroundYpitchToolStripMenuItem.Name = "rotateAroundYpitchToolStripMenuItem";
            this.rotateAroundYpitchToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.rotateAroundYpitchToolStripMenuItem.Text = "Rotate around Y (pitch)";
            this.rotateAroundYpitchToolStripMenuItem.Click += new System.EventHandler(this.rotateAroundYpitchToolStripMenuItem_Click);
            // 
            // rotateAroundXrollToolStripMenuItem
            // 
            this.rotateAroundXrollToolStripMenuItem.Name = "rotateAroundXrollToolStripMenuItem";
            this.rotateAroundXrollToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.rotateAroundXrollToolStripMenuItem.Text = "Rotate around X (roll)";
            this.rotateAroundXrollToolStripMenuItem.Click += new System.EventHandler(this.rotateAroundXrollToolStripMenuItem_Click);
            // 
            // itemList
            // 
            this.itemList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.itemList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colId,
            this.colType,
            this.colLocation});
            this.itemList.FullRowSelect = true;
            this.itemList.HideSelection = false;
            this.itemList.Location = new System.Drawing.Point(3, 3);
            this.itemList.Name = "itemList";
            this.itemList.Size = new System.Drawing.Size(336, 182);
            this.itemList.TabIndex = 34;
            this.itemList.UseCompatibleStateImageBehavior = false;
            this.itemList.View = System.Windows.Forms.View.Details;
            this.itemList.SelectedIndexChanged += new System.EventHandler(this.itemList_SelectedIndexChanged);
            this.itemList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.itemList_MouseUp);
            // 
            // colId
            // 
            this.colId.Text = "Id";
            this.colId.Width = 41;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 142;
            // 
            // colLocation
            // 
            this.colLocation.Text = "Spawn Location";
            this.colLocation.Width = 148;
            // 
            // pRotation
            // 
            this.pRotation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pRotation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pRotation.Controls.Add(this.nRotInc);
            this.pRotation.Controls.Add(this.lIncrement);
            this.pRotation.Controls.Add(this.pEuler);
            this.pRotation.Controls.Add(this.pRotationMatrix);
            this.pRotation.Location = new System.Drawing.Point(3, 389);
            this.pRotation.Name = "pRotation";
            this.pRotation.Size = new System.Drawing.Size(336, 138);
            this.pRotation.TabIndex = 33;
            // 
            // nRotInc
            // 
            this.nRotInc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nRotInc.DecimalPlaces = 2;
            this.nRotInc.Location = new System.Drawing.Point(264, 8);
            this.nRotInc.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.nRotInc.Name = "nRotInc";
            this.nRotInc.Size = new System.Drawing.Size(60, 20);
            this.nRotInc.TabIndex = 33;
            this.nRotInc.ValueChanged += new System.EventHandler(this.nRotInc_ValueChanged);
            // 
            // lIncrement
            // 
            this.lIncrement.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lIncrement.AutoSize = true;
            this.lIncrement.Location = new System.Drawing.Point(204, 10);
            this.lIncrement.Name = "lIncrement";
            this.lIncrement.Size = new System.Drawing.Size(54, 13);
            this.lIncrement.TabIndex = 32;
            this.lIncrement.Text = "Increment";
            // 
            // pEuler
            // 
            this.pEuler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pEuler.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pEuler.Controls.Add(this.Roll);
            this.pEuler.Controls.Add(this.lPitch);
            this.pEuler.Controls.Add(this.Pitch);
            this.pEuler.Controls.Add(this.lYaw);
            this.pEuler.Controls.Add(this.Yaw);
            this.pEuler.Controls.Add(this.lRoll);
            this.pEuler.Location = new System.Drawing.Point(223, 34);
            this.pEuler.Name = "pEuler";
            this.pEuler.Size = new System.Drawing.Size(108, 98);
            this.pEuler.TabIndex = 32;
            // 
            // Roll
            // 
            this.Roll.DecimalPlaces = 2;
            this.Roll.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.Roll.Location = new System.Drawing.Point(40, 68);
            this.Roll.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.Roll.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.Roll.Name = "Roll";
            this.Roll.Size = new System.Drawing.Size(60, 20);
            this.Roll.TabIndex = 7;
            this.Roll.ValueChanged += new System.EventHandler(this.nRot_ValueChanged);
            // 
            // lPitch
            // 
            this.lPitch.AutoSize = true;
            this.lPitch.Location = new System.Drawing.Point(3, 18);
            this.lPitch.Name = "lPitch";
            this.lPitch.Size = new System.Drawing.Size(31, 13);
            this.lPitch.TabIndex = 3;
            this.lPitch.Text = "Pitch";
            // 
            // Pitch
            // 
            this.Pitch.DecimalPlaces = 2;
            this.Pitch.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.Pitch.Location = new System.Drawing.Point(40, 16);
            this.Pitch.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.Pitch.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.Pitch.Name = "Pitch";
            this.Pitch.Size = new System.Drawing.Size(60, 20);
            this.Pitch.TabIndex = 6;
            this.Pitch.ValueChanged += new System.EventHandler(this.nRot_ValueChanged);
            // 
            // lYaw
            // 
            this.lYaw.AutoSize = true;
            this.lYaw.Location = new System.Drawing.Point(6, 44);
            this.lYaw.Name = "lYaw";
            this.lYaw.Size = new System.Drawing.Size(28, 13);
            this.lYaw.TabIndex = 2;
            this.lYaw.Text = "Yaw";
            // 
            // Yaw
            // 
            this.Yaw.DecimalPlaces = 2;
            this.Yaw.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.Yaw.Location = new System.Drawing.Point(40, 42);
            this.Yaw.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.Yaw.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.Yaw.Name = "Yaw";
            this.Yaw.Size = new System.Drawing.Size(60, 20);
            this.Yaw.TabIndex = 5;
            this.Yaw.ValueChanged += new System.EventHandler(this.nRot_ValueChanged);
            // 
            // lRoll
            // 
            this.lRoll.AutoSize = true;
            this.lRoll.Location = new System.Drawing.Point(9, 70);
            this.lRoll.Name = "lRoll";
            this.lRoll.Size = new System.Drawing.Size(25, 13);
            this.lRoll.TabIndex = 4;
            this.lRoll.Text = "Roll";
            // 
            // pRotationMatrix
            // 
            this.pRotationMatrix.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pRotationMatrix.Controls.Add(this.n33);
            this.pRotationMatrix.Controls.Add(this.lRotationMatrix);
            this.pRotationMatrix.Controls.Add(this.n23);
            this.pRotationMatrix.Controls.Add(this.n13);
            this.pRotationMatrix.Controls.Add(this.n32);
            this.pRotationMatrix.Controls.Add(this.n22);
            this.pRotationMatrix.Controls.Add(this.n12);
            this.pRotationMatrix.Controls.Add(this.n31);
            this.pRotationMatrix.Controls.Add(this.n21);
            this.pRotationMatrix.Controls.Add(this.n11);
            this.pRotationMatrix.Location = new System.Drawing.Point(3, 34);
            this.pRotationMatrix.Name = "pRotationMatrix";
            this.pRotationMatrix.Size = new System.Drawing.Size(200, 98);
            this.pRotationMatrix.TabIndex = 0;
            // 
            // n33
            // 
            this.n33.DecimalPlaces = 15;
            this.n33.Enabled = false;
            this.n33.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n33.Location = new System.Drawing.Point(134, 68);
            this.n33.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n33.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n33.Name = "n33";
            this.n33.Size = new System.Drawing.Size(60, 20);
            this.n33.TabIndex = 38;
            // 
            // lRotationMatrix
            // 
            this.lRotationMatrix.AutoSize = true;
            this.lRotationMatrix.Location = new System.Drawing.Point(63, -1);
            this.lRotationMatrix.Name = "lRotationMatrix";
            this.lRotationMatrix.Size = new System.Drawing.Size(78, 13);
            this.lRotationMatrix.TabIndex = 1;
            this.lRotationMatrix.Text = "Rotation Matrix";
            // 
            // n23
            // 
            this.n23.DecimalPlaces = 15;
            this.n23.Enabled = false;
            this.n23.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n23.Location = new System.Drawing.Point(134, 42);
            this.n23.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n23.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n23.Name = "n23";
            this.n23.Size = new System.Drawing.Size(60, 20);
            this.n23.TabIndex = 37;
            // 
            // n13
            // 
            this.n13.DecimalPlaces = 15;
            this.n13.Enabled = false;
            this.n13.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n13.Location = new System.Drawing.Point(134, 16);
            this.n13.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n13.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n13.Name = "n13";
            this.n13.Size = new System.Drawing.Size(60, 20);
            this.n13.TabIndex = 36;
            // 
            // n32
            // 
            this.n32.DecimalPlaces = 15;
            this.n32.Enabled = false;
            this.n32.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n32.Location = new System.Drawing.Point(68, 68);
            this.n32.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n32.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n32.Name = "n32";
            this.n32.Size = new System.Drawing.Size(60, 20);
            this.n32.TabIndex = 35;
            // 
            // n22
            // 
            this.n22.DecimalPlaces = 15;
            this.n22.Enabled = false;
            this.n22.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n22.Location = new System.Drawing.Point(68, 42);
            this.n22.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n22.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n22.Name = "n22";
            this.n22.Size = new System.Drawing.Size(60, 20);
            this.n22.TabIndex = 34;
            // 
            // n12
            // 
            this.n12.DecimalPlaces = 15;
            this.n12.Enabled = false;
            this.n12.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n12.Location = new System.Drawing.Point(68, 16);
            this.n12.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n12.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n12.Name = "n12";
            this.n12.Size = new System.Drawing.Size(60, 20);
            this.n12.TabIndex = 33;
            // 
            // n31
            // 
            this.n31.DecimalPlaces = 15;
            this.n31.Enabled = false;
            this.n31.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n31.Location = new System.Drawing.Point(3, 68);
            this.n31.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n31.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n31.Name = "n31";
            this.n31.Size = new System.Drawing.Size(60, 20);
            this.n31.TabIndex = 32;
            // 
            // n21
            // 
            this.n21.DecimalPlaces = 15;
            this.n21.Enabled = false;
            this.n21.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n21.Location = new System.Drawing.Point(2, 42);
            this.n21.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n21.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n21.Name = "n21";
            this.n21.Size = new System.Drawing.Size(60, 20);
            this.n21.TabIndex = 31;
            // 
            // n11
            // 
            this.n11.DecimalPlaces = 15;
            this.n11.Enabled = false;
            this.n11.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.n11.Location = new System.Drawing.Point(2, 16);
            this.n11.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.n11.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.n11.Name = "n11";
            this.n11.Size = new System.Drawing.Size(60, 20);
            this.n11.TabIndex = 30;
            // 
            // lRotation
            // 
            this.lRotation.AutoSize = true;
            this.lRotation.Location = new System.Drawing.Point(155, 373);
            this.lRotation.Name = "lRotation";
            this.lRotation.Size = new System.Drawing.Size(47, 13);
            this.lRotation.TabIndex = 32;
            this.lRotation.Text = "Rotation";
            // 
            // pMovement
            // 
            this.pMovement.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pMovement.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pMovement.Controls.Add(this.pMoveXYZ);
            this.pMovement.Controls.Add(this.panel5);
            this.pMovement.Controls.Add(this.cbManualEdit);
            this.pMovement.Controls.Add(this.nMoveInc);
            this.pMovement.Controls.Add(this.label5);
            this.pMovement.Location = new System.Drawing.Point(3, 233);
            this.pMovement.Name = "pMovement";
            this.pMovement.Size = new System.Drawing.Size(336, 137);
            this.pMovement.TabIndex = 30;
            // 
            // pMoveXYZ
            // 
            this.pMoveXYZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pMoveXYZ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pMoveXYZ.Controls.Add(this.lMoveX);
            this.pMoveXYZ.Controls.Add(this.nMoveX);
            this.pMoveXYZ.Controls.Add(this.lMoveY);
            this.pMoveXYZ.Controls.Add(this.nMoveY);
            this.pMoveXYZ.Controls.Add(this.lMoveZ);
            this.pMoveXYZ.Controls.Add(this.nMoveZ);
            this.pMoveXYZ.Location = new System.Drawing.Point(157, 34);
            this.pMoveXYZ.Name = "pMoveXYZ";
            this.pMoveXYZ.Size = new System.Drawing.Size(172, 98);
            this.pMoveXYZ.TabIndex = 31;
            // 
            // lMoveX
            // 
            this.lMoveX.AutoSize = true;
            this.lMoveX.Location = new System.Drawing.Point(3, 18);
            this.lMoveX.Name = "lMoveX";
            this.lMoveX.Size = new System.Drawing.Size(44, 13);
            this.lMoveX.TabIndex = 21;
            this.lMoveX.Text = "Move X";
            // 
            // nMoveX
            // 
            this.nMoveX.DecimalPlaces = 9;
            this.nMoveX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.nMoveX.Location = new System.Drawing.Point(53, 16);
            this.nMoveX.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nMoveX.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.nMoveX.Name = "nMoveX";
            this.nMoveX.Size = new System.Drawing.Size(112, 20);
            this.nMoveX.TabIndex = 20;
            this.nMoveX.ValueChanged += new System.EventHandler(this.MoveEvent);
            // 
            // lMoveY
            // 
            this.lMoveY.AutoSize = true;
            this.lMoveY.Location = new System.Drawing.Point(3, 70);
            this.lMoveY.Name = "lMoveY";
            this.lMoveY.Size = new System.Drawing.Size(44, 13);
            this.lMoveY.TabIndex = 25;
            this.lMoveY.Text = "Move Z";
            // 
            // nMoveY
            // 
            this.nMoveY.DecimalPlaces = 9;
            this.nMoveY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.nMoveY.Location = new System.Drawing.Point(53, 42);
            this.nMoveY.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nMoveY.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.nMoveY.Name = "nMoveY";
            this.nMoveY.Size = new System.Drawing.Size(112, 20);
            this.nMoveY.TabIndex = 22;
            this.nMoveY.ValueChanged += new System.EventHandler(this.MoveEvent);
            // 
            // lMoveZ
            // 
            this.lMoveZ.AutoSize = true;
            this.lMoveZ.Location = new System.Drawing.Point(3, 44);
            this.lMoveZ.Name = "lMoveZ";
            this.lMoveZ.Size = new System.Drawing.Size(44, 13);
            this.lMoveZ.TabIndex = 23;
            this.lMoveZ.Text = "Move Y";
            // 
            // nMoveZ
            // 
            this.nMoveZ.DecimalPlaces = 9;
            this.nMoveZ.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.nMoveZ.Location = new System.Drawing.Point(53, 68);
            this.nMoveZ.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nMoveZ.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.nMoveZ.Name = "nMoveZ";
            this.nMoveZ.Size = new System.Drawing.Size(112, 20);
            this.nMoveZ.TabIndex = 24;
            this.nMoveZ.ValueChanged += new System.EventHandler(this.MoveEvent);
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.lX);
            this.panel5.Controls.Add(this.nX);
            this.panel5.Controls.Add(this.lY);
            this.panel5.Controls.Add(this.nY);
            this.panel5.Controls.Add(this.nZ);
            this.panel5.Controls.Add(this.lZ);
            this.panel5.Controls.Add(this.lSpawnPoint);
            this.panel5.Location = new System.Drawing.Point(3, 34);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(148, 98);
            this.panel5.TabIndex = 30;
            // 
            // lX
            // 
            this.lX.AutoSize = true;
            this.lX.Location = new System.Drawing.Point(3, 18);
            this.lX.Name = "lX";
            this.lX.Size = new System.Drawing.Size(20, 13);
            this.lX.TabIndex = 4;
            this.lX.Text = "X :";
            // 
            // nX
            // 
            this.nX.DecimalPlaces = 9;
            this.nX.Enabled = false;
            this.nX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.nX.Location = new System.Drawing.Point(29, 16);
            this.nX.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nX.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.nX.Name = "nX";
            this.nX.Size = new System.Drawing.Size(112, 20);
            this.nX.TabIndex = 5;
            this.nX.ValueChanged += new System.EventHandler(this.UpdateNumerics);
            // 
            // lY
            // 
            this.lY.AutoSize = true;
            this.lY.Location = new System.Drawing.Point(3, 44);
            this.lY.Name = "lY";
            this.lY.Size = new System.Drawing.Size(20, 13);
            this.lY.TabIndex = 6;
            this.lY.Text = "Y :";
            // 
            // nY
            // 
            this.nY.DecimalPlaces = 9;
            this.nY.Enabled = false;
            this.nY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.nY.Location = new System.Drawing.Point(29, 42);
            this.nY.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nY.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.nY.Name = "nY";
            this.nY.Size = new System.Drawing.Size(112, 20);
            this.nY.TabIndex = 7;
            this.nY.ValueChanged += new System.EventHandler(this.UpdateNumerics);
            // 
            // nZ
            // 
            this.nZ.DecimalPlaces = 9;
            this.nZ.Enabled = false;
            this.nZ.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.nZ.Location = new System.Drawing.Point(29, 68);
            this.nZ.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nZ.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.nZ.Name = "nZ";
            this.nZ.Size = new System.Drawing.Size(112, 20);
            this.nZ.TabIndex = 8;
            this.nZ.ValueChanged += new System.EventHandler(this.UpdateNumerics);
            // 
            // lZ
            // 
            this.lZ.AutoSize = true;
            this.lZ.Location = new System.Drawing.Point(3, 70);
            this.lZ.Name = "lZ";
            this.lZ.Size = new System.Drawing.Size(20, 13);
            this.lZ.TabIndex = 9;
            this.lZ.Text = "Z :";
            // 
            // lSpawnPoint
            // 
            this.lSpawnPoint.AutoSize = true;
            this.lSpawnPoint.Location = new System.Drawing.Point(48, 0);
            this.lSpawnPoint.Name = "lSpawnPoint";
            this.lSpawnPoint.Size = new System.Drawing.Size(67, 13);
            this.lSpawnPoint.TabIndex = 12;
            this.lSpawnPoint.Text = "Spawn Point";
            // 
            // cbManualEdit
            // 
            this.cbManualEdit.AutoSize = true;
            this.cbManualEdit.Location = new System.Drawing.Point(3, 9);
            this.cbManualEdit.Name = "cbManualEdit";
            this.cbManualEdit.Size = new System.Drawing.Size(82, 17);
            this.cbManualEdit.TabIndex = 29;
            this.cbManualEdit.Text = "Manual Edit";
            this.cbManualEdit.UseVisualStyleBackColor = true;
            this.cbManualEdit.CheckedChanged += new System.EventHandler(this.cbManualEdit_CheckedChanged);
            // 
            // nMoveInc
            // 
            this.nMoveInc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nMoveInc.DecimalPlaces = 9;
            this.nMoveInc.Increment = new decimal(new int[] {
            1,
            0,
            0,
            589824});
            this.nMoveInc.Location = new System.Drawing.Point(211, 8);
            this.nMoveInc.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nMoveInc.Name = "nMoveInc";
            this.nMoveInc.Size = new System.Drawing.Size(112, 20);
            this.nMoveInc.TabIndex = 10;
            this.nMoveInc.ValueChanged += new System.EventHandler(this.nMoveInc_ValueChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(151, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Increment";
            // 
            // bTeleport
            // 
            this.bTeleport.Enabled = false;
            this.bTeleport.Location = new System.Drawing.Point(3, 533);
            this.bTeleport.Name = "bTeleport";
            this.bTeleport.Size = new System.Drawing.Size(104, 23);
            this.bTeleport.TabIndex = 27;
            this.bTeleport.Text = "Teleport To Object";
            this.bTeleport.UseVisualStyleBackColor = true;
            this.bTeleport.Click += new System.EventHandler(this.bTeleport_Click);
            // 
            // editProgress
            // 
            this.editProgress.Location = new System.Drawing.Point(3, 188);
            this.editProgress.Name = "editProgress";
            this.editProgress.Size = new System.Drawing.Size(336, 13);
            this.editProgress.TabIndex = 35;
            this.editProgress.Visible = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(371, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.attachToProcessToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // attachToProcessToolStripMenuItem
            // 
            this.attachToProcessToolStripMenuItem.Name = "attachToProcessToolStripMenuItem";
            this.attachToProcessToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.attachToProcessToolStripMenuItem.Text = "Attach to Process";
            this.attachToProcessToolStripMenuItem.Click += new System.EventHandler(this.attachToProcessToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateFrequencyToolStripMenuItem,
            this.keepInForegroundToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // updateFrequencyToolStripMenuItem
            // 
            this.updateFrequencyToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msToolStripMenuItem,
            this.msToolStripMenuItem1,
            this.msToolStripMenuItem2,
            this.msToolStripMenuItem3});
            this.updateFrequencyToolStripMenuItem.Name = "updateFrequencyToolStripMenuItem";
            this.updateFrequencyToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.updateFrequencyToolStripMenuItem.Text = "Update Frequency";
            // 
            // msToolStripMenuItem
            // 
            this.msToolStripMenuItem.CheckOnClick = true;
            this.msToolStripMenuItem.Name = "msToolStripMenuItem";
            this.msToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.msToolStripMenuItem.Text = "100ms";
            this.msToolStripMenuItem.Click += new System.EventHandler(this.msToolStripMenuItem_Click);
            // 
            // msToolStripMenuItem1
            // 
            this.msToolStripMenuItem1.CheckOnClick = true;
            this.msToolStripMenuItem1.Name = "msToolStripMenuItem1";
            this.msToolStripMenuItem1.Size = new System.Drawing.Size(114, 22);
            this.msToolStripMenuItem1.Text = "250ms";
            this.msToolStripMenuItem1.Click += new System.EventHandler(this.msToolStripMenuItem1_Click);
            // 
            // msToolStripMenuItem2
            // 
            this.msToolStripMenuItem2.Checked = true;
            this.msToolStripMenuItem2.CheckOnClick = true;
            this.msToolStripMenuItem2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.msToolStripMenuItem2.Name = "msToolStripMenuItem2";
            this.msToolStripMenuItem2.Size = new System.Drawing.Size(114, 22);
            this.msToolStripMenuItem2.Text = "500ms";
            this.msToolStripMenuItem2.Click += new System.EventHandler(this.msToolStripMenuItem2_Click);
            // 
            // msToolStripMenuItem3
            // 
            this.msToolStripMenuItem3.CheckOnClick = true;
            this.msToolStripMenuItem3.Name = "msToolStripMenuItem3";
            this.msToolStripMenuItem3.Size = new System.Drawing.Size(114, 22);
            this.msToolStripMenuItem3.Text = "1000ms";
            this.msToolStripMenuItem3.Click += new System.EventHandler(this.msToolStripMenuItem3_Click);
            // 
            // keepInForegroundToolStripMenuItem
            // 
            this.keepInForegroundToolStripMenuItem.CheckOnClick = true;
            this.keepInForegroundToolStripMenuItem.Name = "keepInForegroundToolStripMenuItem";
            this.keepInForegroundToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.keepInForegroundToolStripMenuItem.Text = "Keep in foreground";
            this.keepInForegroundToolStripMenuItem.Click += new System.EventHandler(this.keepInForegroundToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 0;
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 0;
            this.toolTip1.ReshowDelay = 100;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 603);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Forge Assistant";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nSpawnTime)).EndInit();
            this.contextMenu.ResumeLayout(false);
            this.contextMenu.PerformLayout();
            this.pRotation.ResumeLayout(false);
            this.pRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nRotInc)).EndInit();
            this.pEuler.ResumeLayout(false);
            this.pEuler.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Roll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Yaw)).EndInit();
            this.pRotationMatrix.ResumeLayout(false);
            this.pRotationMatrix.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.n33)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n23)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n13)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n32)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n22)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n31)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n11)).EndInit();
            this.pMovement.ResumeLayout(false);
            this.pMovement.PerformLayout();
            this.pMoveXYZ.ResumeLayout(false);
            this.pMoveXYZ.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveZ)).EndInit();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nMoveInc)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lX;
        private System.Windows.Forms.Label lZ;
        private System.Windows.Forms.Label lY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nMoveInc;
        private System.Windows.Forms.NumericUpDown nMoveX;
        private System.Windows.Forms.Label lSpawnPoint;
        private System.Windows.Forms.Label lMoveY;
        private System.Windows.Forms.NumericUpDown nMoveZ;
        private System.Windows.Forms.Label lMoveZ;
        private System.Windows.Forms.NumericUpDown nMoveY;
        private System.Windows.Forms.Label lMoveX;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem attachToProcessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateFrequencyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem msToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem msToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem msToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem msToolStripMenuItem3;
        private System.Windows.Forms.Button bTeleport;
        private System.Windows.Forms.ToolStripMenuItem keepInForegroundToolStripMenuItem;
        private System.Windows.Forms.CheckBox cbManualEdit;
        private System.Windows.Forms.Panel pRotation;
        private System.Windows.Forms.Label lRotation;
        private System.Windows.Forms.Label lMovement;
        private System.Windows.Forms.Panel pMovement;
        private System.Windows.Forms.Label lRotationMatrix;
        private System.Windows.Forms.Panel pRotationMatrix;
        private System.Windows.Forms.NumericUpDown n32;
        private System.Windows.Forms.NumericUpDown n22;
        private System.Windows.Forms.NumericUpDown n12;
        private System.Windows.Forms.NumericUpDown Roll;
        private System.Windows.Forms.NumericUpDown Pitch;
        private System.Windows.Forms.NumericUpDown Yaw;
        private System.Windows.Forms.Label lRoll;
        private System.Windows.Forms.Label lPitch;
        private System.Windows.Forms.Label lYaw;
        private System.Windows.Forms.ColumnHeader colId;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colLocation;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel pMoveXYZ;
        private System.Windows.Forms.NumericUpDown nRotInc;
        private System.Windows.Forms.Label lIncrement;
        private System.Windows.Forms.Panel pEuler;
        private System.Windows.Forms.Label lTotal;
        private System.Windows.Forms.MenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyRotationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteRotationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteLocationAndRotationToolStripMenuItem;
        private System.Windows.Forms.Label lMap;
        private System.Windows.Forms.NumericUpDown nSpawnTime;
        private System.Windows.Forms.Label lSpawnTime;
        public System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Button button2;
        public ListViewNF itemList;
        public System.Windows.Forms.NumericUpDown nX;
        public System.Windows.Forms.NumericUpDown nZ;
        public System.Windows.Forms.NumericUpDown nY;
        public System.Windows.Forms.NumericUpDown n33;
        public System.Windows.Forms.NumericUpDown n23;
        public System.Windows.Forms.NumericUpDown n13;
        public System.Windows.Forms.NumericUpDown n31;
        public System.Windows.Forms.NumericUpDown n21;
        public System.Windows.Forms.NumericUpDown n11;
        private System.Windows.Forms.ToolStripMenuItem copymulti;
        private System.Windows.Forms.ToolStripMenuItem pastemulti;
        private System.Windows.Forms.ToolStripMenuItem pasteLocationMultiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteRotationMultiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteLocationAndRotationMultiToolStripMenuItem;
        public System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.ProgressBar editProgress;
        private System.Windows.Forms.Label lSelected;
        private System.Windows.Forms.Label lIndex;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolStripMenuItem rotateStructureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateAroundYawZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateAroundYpitchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateAroundXrollToolStripMenuItem;
    }
}

