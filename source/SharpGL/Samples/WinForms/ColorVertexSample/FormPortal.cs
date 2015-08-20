﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorVertexSample
{
    public partial class FormPortal : Form
    {
        public FormPortal()
        {
            InitializeComponent();
        }

        private void btnWell_Click(object sender, EventArgs e)
        {
            (new FormWell()).Show();
        }

        private void btnFormHexahedronGridder_Click(object sender, EventArgs e)
        {
            (new FormHexahedronGridderElement()).Show();
        }

        private void btnFormPointSpriteStringElement_Click(object sender, EventArgs e)
        {
            (new FormPointSpriteStringElement()).Show();
        }

        private void btnFormPointSpriteGridderElement_Click(object sender, EventArgs e)
        {
            (new FormPointSpriteGridderElement()).Show();
        }

        private void btnFormUnStructuredGridderElement_Click(object sender, EventArgs e)
        {
            (new FormUnStructuredGridderElement()).Show();
        }
    }
}