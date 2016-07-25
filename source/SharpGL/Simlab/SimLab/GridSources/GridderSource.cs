using GlmNet;
using SharpGL.SceneComponent;
using SharpGL.SceneGraph;
using SimLab.GridSource.Factory;
using SimLab.GridSources;
using SimLab.SimGrid;
using SimLab.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLab.GridSource
{
    /// <summary>
    /// ��������Դ, ��ֵ����ó�ʼ��Init����ʹ��:
    /// GridderSource src = new CatesianGridderSource()
    /// src.NX = 1;
    /// ....
    /// src.Init();
    /// 
    /// 
    /// </summary>
    public abstract class GridderSource
    {
        private GridBufferDataFactory factory;

        private GridIndexer gridIndexer;

        private float zscale=1.0f; //keep scale 


        private int[] gridIndexMapper;
        private GridIndexesMapper blockIndexesMapper = new GridIndexesMapper(null);

        /// <summary>
        /// X�᷽���ϵ���������
        /// </summary>
        public int NX { get; set; }

        /// <summary>
        /// Y�᷽���ϵ���������
        /// </summary>
        public int NY { get; set; }

        /// <summary>
        /// Z�᷽���ϵ���������
        /// </summary>
        public int NZ { get; set; }

        /// <summary>
        /// ��ȡ���������Ԫ��������
        /// </summary>
        public int DimenSize
        {
            get
            {
                return this.NX * this.NY * this.NZ;
            }
        }

        /// <summary>
        /// ÿ������һ��ֵ��ȫ��Ϊ�㣬��ʾȫ��������
        /// </summary>
        private int[] zeroVisibles;


        /// <summary>
        /// ͸��texure����,��СΪDimenSize
        /// </summary>
        private float[] invisibleTextures;

       

        /// <summary>
        /// ���������ٰ���1��Ԫ�أ�����true�����򷵻�false��
        /// </summary>
        public bool IsDimensEmpty
        {
            get
            {
                if (this.NX <= 0 || this.NY <= 0 || this.NZ <= 0)
                    return true;
                return false;
            }
        }

        public int[] ActNums { get; set; }


        /// <summary>
        /// ��һά��������ת��Ϊ��ά��I,J,K����ʾ������������
        /// </summary>
        /// <param name="index">0��ʼ����������</param>
        /// <param name="iv"></param>
        /// <param name="jv"></param>
        /// <param name="kv"></param>
        public void InvertIJK(int index, out int I, out int J, out int K)
        {
             this.gridIndexer.IJKOfIndex(index, out I, out J, out K);
        }

        /// <summary>
        /// �����������λ��
        /// </summary>
        /// <param name="I">�������� I����1��ʼ</param>
        /// <param name="J">�������� J����1��ʼ</param>
        /// <param name="K">�������� K����1��ʼ</param>
        /// <returns></returns>
        protected int GridIndexOf(int I, int J, int K)
        {
            return gridIndexer.IndexOf(I, J, K);
        }

        protected void IJK2Index(int I, int J, int K, out int index)
        {
            index = this.gridIndexer.IndexOf(I, J, K);
            return;
        }

        /// <summary>
        /// �ж������Ƿ��ǻ����
        /// </summary>
        /// <param name="i">�±��1��ʼ</param>
        /// <param name="j">�±��1��ʼ</param>
        /// <param name="k">�±��1��ʼ</param>
        /// <returns></returns>
        public bool IsActiveBlock(int i, int j, int k)
        {
         
            int gridIndex;
            this.IJK2Index(i, j, k, out gridIndex);
            int actnum = this.ActNums[gridIndex];
            if (actnum <= 0) //С�ڻ����0������鶼�Ƿǻ�������
                return false;
            return true;
        }


        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="gridIndex"></param>
        /// <returns></returns>
        public bool IsActiveBlock(int gridIndex)
        {
             return this.ActNums[gridIndex] > 0;
        }


        protected abstract GridBufferDataFactory CreateFactory();
      

        protected GridBufferDataFactory Factory
        {
            get
            {
                if (this.factory == null)
                {
                    this.factory = CreateFactory();
                }
                return factory;
            }
        }


        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="dimenSize">�����С</param>
        /// <param name="value">��ʼֵ</param>
        /// <returns></returns>
        protected int[] InitIntArray(int dimenSize, int value = 1)
        {
            int[] actNums = new int[dimenSize];
            for (int i = 0; i < dimenSize; i++)
            {
                actNums[i] = value;
            }
            return actNums;
        }

        public float[] InitFloatArray(int size, float value = 2)
        {
            float[] array = new float[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = value;
            }
            return array;
        }

        /// <summary>
        /// �����ʼBounds,
        /// </summary>
        protected abstract Rectangle3D InitSourceActiveBounds();

        /// <summary>
        /// ��ʼ��
        /// </summary>
        public virtual void Init()
        {

            if (this.gridIndexer == null)
            {
                this.gridIndexer = new GridIndexer(this.NX, this.NY, this.NZ);
            }

            if (this.ActNums == null)
            {
                this.ActNums = InitIntArray(this.DimenSize,1);
            }
            if (this.zeroVisibles == null)
            {
                this.zeroVisibles = InitIntArray(this.DimenSize, 0);
            }
            if (this.invisibleTextures == null)
            {
               //��ʼ��������
               this.invisibleTextures = InitFloatArray(this.DimenSize, 2);
            }

            this.InitGridCoordinates();

            this.SourceActiveBounds = this.InitSourceActiveBounds();

            this.InitTransform();
        }

        public void InitTransform(){

            Vertex srcMin = this.SourceActiveBounds.Min;
            Vertex srcMax = this.SourceActiveBounds.Max;

            float zflip = 1;
            if(srcMin.Z >=0.0f&&srcMax.Z >=0.0f){
              zflip = -1; //flip the z value
            }

            Vertex srcCenter = this.SourceActiveBounds.Center;

            mat4 flipTransform = glm.scale(mat4.identity(), new vec3(1, 1, zflip));

            mat4 scaleTransform = glm.scale(mat4.identity(),new vec3(1,1,this.ZScale));


            Vertex scaleCenter = flipTransform*scaleTransform * srcCenter;
            //�����������ƶ������ĵ�
            float dx = 0.0f - scaleCenter.X;
            float dy = 0.0f - scaleCenter.Y;
            float dz = 0.0f - scaleCenter.Z;

            mat4 translate = glm.translate(mat4.identity(), new vec3(dx, dy, dz));

            this.FlipTransform = flipTransform;
            this.ScaleTransform = scaleTransform;
            this.TranslateTransform = translate;

            this.ScaleTranslateform =  scaleTransform*translate;

            this.Transform = flipTransform*scaleTransform*translate;


            //Vertex newcenter = this.Transform * srcCenter;
            
            Vertex destMin = this.Transform * this.SourceActiveBounds.Min;
            Vertex destMax = this.Transform * this.SourceActiveBounds.Max;

            //�任�����ά����������
            this.TransformedActiveBounds = new Rectangle3D(destMin, destMax);

        }

        /// <summary>
        /// ��������ӳ������
        /// </summary>
        /// <param name="gridIndexes"></param>
        /// <param name="values"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public TexCoordBuffer CreateTextureCoordinates(int[] gridIndexes, float[] values, float minValue, float maxValue)
        {
            return this.Factory.CreateTextureCoordinates(this, gridIndexes, values, minValue, maxValue);
        }

        public MeshBase CreateMesh()
        {
            MeshBase geometry = this.Factory.CreateMesh(this);
            this.Max = geometry.Max;
            this.Min = geometry.Min;
            return geometry;
        }

      
        public int[] BindVisibles(int[] a1, int[] a2)
        {
            if (a1.Length != a2.Length)
                throw new ArgumentException("array size not equal");
            int length = a1.Length;
            int[] results = new int[length];
            for (int i = 0; i < length; i++)
            {
                if (a1[i] > 0 && a2[i] > 0)
                    results[i] = 1;
                else
                    results[i] = 0;
            }
            return results;
        }


        /// <summary>
        /// ����������ת��Ϊ���ӽ��
        /// </summary>
        /// <param name="gridIndexes"></param>
        /// <returns></returns>
        public int[] ExpandVisibles(int[] gridIndexes)
        {
             int[] gridVisibles = new int[this.DimenSize];
             Array.Copy(this.zeroVisibles, gridVisibles, this.DimenSize);
             for (int i = 0; i < gridIndexes.Length; i++)
             {
                  gridVisibles[gridIndexes[i]] = 1;
             }
             return gridVisibles;
        }

        /// <summary>
        /// ��ʼ���������������
        /// </summary>
        protected abstract void InitGridCoordinates();

        /// <summary>
        /// ��������Ĭ�ϵ�����Texture,ֵΪ��(ֵ����1��
        /// </summary>
        /// <returns></returns>
        public float[] GetInvisibleTextureCoords()
        {
            float[] none = new float[this.DimenSize];
            Array.Copy(this.invisibleTextures, none, this.DimenSize);
            return none;
        }

        /// <summary>
        /// ģ�ͷ�Χ�ı߽���Сֵ
        /// </summary>
        public Vertex Min
        {
            get;
            internal set;
        }

        /// <summary>
        /// ģ�ͷ�Χ�ı߽����ֵ
        /// </summary>
        public Vertex Max
        {
            get;
            internal set;
        }


        /// <summary>
        /// ԭʼ���ݵ���ά���α߽�
        /// </summary>
        public Rectangle3D SourceActiveBounds{
           get;
           internal set;
        }

        /// <summary>
        /// ʹģ������Z�Զ���ת
        /// </summary>
        public mat4 FlipTransform{
           get;
           internal set;
        }

        public mat4 ScaleTransform{
           get;
           internal set;
        }

        /// <summary>
        /// ��ƽ�Ƶı任����
        /// </summary>
        public mat4 TranslateTransform{
          get;
          internal set;
        }


        /// <summary>
        /// ScaleTransform * TranslateFrom
        /// </summary>
        public mat4 ScaleTranslateform{
          get;
          internal set;
        }

        /// <summary>
        /// SourceActiveBounds��DestActiveBounds�ı任
        /// </summary>
        public mat4 Transform{

            get;
            internal set;

        }


        /// <summary>
        /// ���ĵ�������ԭ��
        /// </summary>
        public Rectangle3D TransformedActiveBounds{
           get;
           internal set;
        }

        public object Tag{
          get;set;
        }

        public float ZScale{
          get{ return this.zscale;}
          set{ 
            this.zscale = value;
          }
        }

        public int[] GridIndexMapper{
           get{ return this.gridIndexMapper;}
           set{ 
             this.gridIndexMapper = value;
             this.blockIndexesMapper = new GridIndexesMapper(this.gridIndexMapper);
           }
        }

        

        public int[] MapBlockIndexes(int blockIndex){
         
            return this.blockIndexesMapper.MapIndexes(blockIndex);

        }

    }



}
