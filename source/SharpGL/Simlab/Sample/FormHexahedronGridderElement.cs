﻿using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Effects;
using SharpGL.SceneGraph.Lighting;
using SharpGL.SceneGraph.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL.SceneGraph.Assets;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneComponent;
using SharpGL.SceneComponent.Model;
using SimLab.GridSource;
using SimLab;
using System.Globalization;

namespace Sample
{
    public partial class FormHexahedronGridderElement : Form
    {
        public FormHexahedronGridderElement()
        {
            InitializeComponent();

            InitilizeViewTypeControl();

           //Application.Idle += Application_Idle;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            IPickedGeometry picked = this.sim3D.PickedPrimitive;
            this.lblPickedPrimitive.Text = string.Format("Picked:{0}", picked);
            this.lblPickingInfo.Text = string.Format("Picked:{0}", picked);
        }

      
        private void InitilizeViewTypeControl()
        {
            foreach (string item in Enum.GetNames(typeof(ViewTypes)))
            {
                this.cmbViewType.Items.Add(item);
            }

            foreach (string item in Enum.GetNames(typeof(CameraTypes)))
            {
                this.cmbCameraType.Items.Add(item);
            }
        }

        private float[] initArray(int size, float value)
        {
            float[] result = new float[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = value;
            }
            return result;
        }

        protected void InitSlice(ListBox box, IList<int> slices){
             
             foreach(int coord in slices){
                 box.Items.Add(coord);
             }

        }


        private void InitPropertiesAndSelectDefault(int dimenSize, float minValue, float maxValue)
        {

            GridProperty prop1 = GridPropertyGenerator.CreateGridIndexProperty(dimenSize, "Grid Position");
            GridProperty prop2 = GridPropertyGenerator.CreateRandomProperty(dimenSize, "Random", minValue, maxValue);
            this.cbxGridProperties.Items.Clear();
            this.cbxGridProperties.Items.Add(prop1);
            this.cbxGridProperties.Items.Add(prop2);
            this.cbxGridProperties.SelectedIndex = 0;
        }

        public GridProperty CurrentProperty
        {
            get
            {
                 return cbxGridProperties.SelectedItem as GridProperty;
            }

        }

        int elementCounter = 0;
        private void CreateCatesianGridVisual3D(object sender, EventArgs e)
        {
            try
            {
                int nx = System.Convert.ToInt32(tbNX.Text);
                int ny = System.Convert.ToInt32(tbNY.Text);
                int nz = System.Convert.ToInt32(tbNZ.Text);
                float step = System.Convert.ToSingle(tbColorIndicatorStep.Text);
                //float radius = System.Convert.ToSingle(this.tbRadius.Text);
                float propMin = System.Convert.ToSingle(this.tbxPropertyMinValue.Text,CultureInfo.InvariantCulture);
                float propMax = System.Convert.ToSingle(this.tbxPropertyMaxValue.Text,CultureInfo.InvariantCulture);
                int dimenSize = nx * ny * nz;
                float dx = System.Convert.ToSingle(this.tbDX.Text);
                float dy = System.Convert.ToSingle(this.gbDY.Text);
                float dz = System.Convert.ToSingle(this.tbDZ.Text);
                float[] dxArray = initArray(dimenSize, dx);
                float[] dyArray = initArray(dimenSize, dy);
                float[] dzArray = initArray(dimenSize, dz);
                // use CatesianGridderSource to fill HexahedronGridderElement's content.
                CatesianGridderSource source = new CatesianGridderSource() { NX = nx, NY = ny, NZ = nz, DX = dxArray, DY = dyArray, DZ = dzArray, };
                source.IBlocks = GridBlockHelper.CreateBlockCoords(nx);
                source.JBlocks = GridBlockHelper.CreateBlockCoords(ny);
                source.KBlocks = GridBlockHelper.CreateBlockCoords(nz);
                source.Init();
                InitSlice(lbxNI, source.IBlocks);
                InitSlice(lbxNJ, source.JBlocks);
                InitSlice(lbxNZ, source.KBlocks);
                InitPropertiesAndSelectDefault(dimenSize, propMin, propMax);
                

                ///模拟获得网格属性
                ///获得当前选中的属性


                float minValue = this.CurrentProperty.MinValue;
                float maxValue = this.CurrentProperty.MaxValue;
                step = (maxValue * 1.0f - minValue * 1.0f) / 10;
                int[] gridIndexes = this.CurrentProperty.GridIndexes;
                float[] gridValues = this.CurrentProperty.Values;


                //设置色标的范围
                this.sim3D.SetColorIndicator(minValue, maxValue, step);
                
               
                // use HexahedronGridderElement
                DateTime t0 = DateTime.Now;
                MeshGeometry3D geometry = source.CreateMesh();
                TextureCoordinatesBufferData textureCoodinates  = source.CreateTextureCoordinates(gridIndexes, gridValues, minValue, maxValue);
                Bitmap texture = ColorPaletteHelper.GenBitmap(this.sim3D.uiColorIndicator.Data.ColorPalette);

                //MeshGeometry mesh = HexahedronGridderHelper.CreateMesh(source);
                HexahedronGrid gridder = new HexahedronGrid(this.sim3D.OpenGL, this.sim3D.Scene.CurrentCamera);
                gridder.Init(geometry);
                gridder.RenderGrid = true;
                gridder.RenderGridWireFrame = false;
                gridder.SetTexture(texture);
                gridder.SetTextureCoods(textureCoodinates);

                DateTime t1 = DateTime.Now;
                TimeSpan ts1 = t1 - t0;
                
                //mesh.VertexColors = HexahedronGridderHelper.FromColors(source, gridIndexes, colors, mesh.Visibles);
                //this.DebugMesh(mesh);
               
                //HexahedronGridderElement gridderElement = new HexahedronGridderElement(source, this.scientificVisual3DControl.Scene.CurrentCamera);
                //gridderElement.renderWireframe = false;
                //method1
                //gridderElement.Initialize(this.scientificVisual3DControl.OpenGL);

                //method2
                //gridderElement.Initialize(this.scientificVisual3DControl.OpenGL, mesh);
                DateTime t2 = DateTime.Now;
               
                //gridderElement.SetBoundingBox(mesh.Min, mesh.Max);

                             

                //gridderElement.Name = string.Format("element {0}", elementCounter++);
                //this.scientificVisual3DControl.AddModelElement(gridderElement);

                DateTime t3 = DateTime.Now;
                // update ModelContainer's BoundingBox.
                BoundingBox boundingBox = this.sim3D.ModelContainer.BoundingBox;
                boundingBox.SetBounds(geometry.Min, geometry.Max);
            
                // update ViewType to UserView.
                this.sim3D.ViewType = ViewTypes.UserView;
                this.sim3D.AddModelElement(gridder);
                //mesh.Dispose();

                StringBuilder msgBuilder = new StringBuilder();
                msgBuilder.AppendLine(String.Format("create mesh in {0} secs", (t1 - t0).TotalSeconds));
                msgBuilder.AppendLine(String.Format("init SceneElement in {0} secs", (t2 - t1).TotalSeconds));
                msgBuilder.AppendLine(String.Format("total load in {0} secs", (t2 - t0).TotalSeconds));
                String msg = msgBuilder.ToString();
                MessageBox.Show(msg, "Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private List<string> rangeMin = new List<string>() { "-1000", "1100", "3200" };
        private List<string> rangeMax = new List<string>() { "1000", "3100", "5200" };
        private List<string> stepList = new List<string>() { "110", "110", "100" };
        private int testCaseIndex = 0;

        /// <summary>
        /// quick way to set min and max value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblDebugInfo_Click(object sender, EventArgs e)
        {
            //this.tbRangeMax.Text = rangeMax[testCaseIndex];
            //this.tbRangeMin.Text = rangeMin[testCaseIndex];
            this.tbColorIndicatorStep.Text = stepList[testCaseIndex];
            testCaseIndex = testCaseIndex >= rangeMin.Count - 1 ? 0 : testCaseIndex + 1;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            GC.Collect();
        }

        private void btnClearModels_Click(object sender, EventArgs e)
        {
            this.sim3D.ClearScientificModels();

            GC.Collect();
        }

        private void lblDebugInfo_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                bool depthTest = this.sim3D.OpenGL.IsEnabled(OpenGL.GL_DEPTH_TEST);
                StringBuilder builder = new StringBuilder();
                builder.Append(string.Format("depth test: {0}", depthTest ? "enabled" : "disabled"));
                MessageBox.Show(builder.ToString());
            }
        }

        private void chkRenderContainerBox_CheckedChanged(object sender, EventArgs e)
        {
            this.sim3D.RenderBoundingBox = this.chkRenderContainerBox.Checked;
        }

        private void cmbViewType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = this.cmbViewType.SelectedItem.ToString();
            ViewTypes viewType = (ViewTypes)Enum.Parse(typeof(ViewTypes), selected);
            this.sim3D.ViewType = viewType;
        }

        private void cmbCameraType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = this.cmbCameraType.SelectedItem.ToString();
            CameraTypes cameraType = (CameraTypes)Enum.Parse(typeof(CameraTypes), selected);
            this.sim3D.CameraType = cameraType;
        }

        private void scientificVisual3DControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'r')
            {
                
                this.sim3D.Invalidate();
            }


            if (e.KeyChar == 'c')
            {
                OpenGL gl = this.sim3D.OpenGL;

              
             
            }
        }

        private void FormHexahedronGridder_Load(object sender, EventArgs e)
        {
            string message = string.Format("{0}", "Add模型后，可通过'R'键观察随机隐藏hexahedron的情形。");
            MessageBox.Show(message, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

    }
}