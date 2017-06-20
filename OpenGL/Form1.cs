using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.IO;
using System.Drawing.Imaging;

namespace OpenGL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int BasicProgramID;
        int BasicVertexShader;
        int BasicFragmentShader;
        int vaoHandle;
        int texID;
        private bool InitShaders()
        {
            string glVersion = GL.GetString(StringName.Version);
            string glslVersion = GL.GetString(StringName.ShadingLanguageVersion);
            Bitmap bitmap = new Bitmap("..\\..\\img\\darkPoint.bmp");
            texID = CreateTexture(bitmap, TextureTarget.TextureRectangle);
            GL.BindTexture(TextureTarget.TextureRectangle, 0);
     
            BasicProgramID = GL.CreateProgram();
            loadShader("..\\..\\Shaders\\basic.vert", ShaderType.VertexShader, BasicProgramID, out BasicVertexShader);
            loadShader("..\\..\\Shaders\\basic.frag", ShaderType.FragmentShader, BasicProgramID, out BasicFragmentShader);
            //Now that the shaders are added, the program needs to be linked.
            //Like C code, the code is first compiled, then linked, so that it goes
            //from human-readable code to the machine language needed.
            GL.LinkProgram(BasicProgramID);

            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));
            float[] positionData = { -0.8f, -0.8f, 0.0f, 0.8f, -0.8f, 0.0f, 0.0f, 0.8f, 0.0f };
            float[] textureCoords = { 0, bitmap.Height, bitmap.Width, bitmap.Height, bitmap.Width/2, 0};
            // Create and fill buffer objects
            int[] vboHandlers = new int[2];
            GL.GenBuffers(2, vboHandlers);
            // Fill coordinate buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * positionData.Length), positionData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * textureCoords.Length), textureCoords, BufferUsageHint.StaticDraw);
            // Create vertex object
            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[0]);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[1]);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);

            return true;
        }

        /// <summary>
        /// This creates a new shader (using a value from the ShaderType enum), loads code for it, compiles it, and adds it to our program.
        /// It also prints any errors it found to the console, which is really nice for when you make a mistake in a shader (it will also yell at you if you use deprecated code).
        /// </summary>
        /// <param name="filename">File to load the shader from</param>
        /// <param name="type">Type of shader to load</param>
        /// <param name="program">ID of the program to use the shader with</param>
        /// <param name="address">Address of the compiled shader</param>
        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        private static void Resize(int width, int height)
        {
            if (height == 0)
            {
                height = 1;
            }

            GL.Viewport(0, 0, width, height);
        }

        private void openGlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.ClearColor(Color.AliceBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindImageTexture(0, texID, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32ui);
            GL.UseProgram(BasicProgramID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            openGlControl.SwapBuffers();
            GL.UseProgram(0);
        }

        private void openGlControl_Load(object sender, EventArgs e)
        {
            Resize(openGlControl.Width, openGlControl.Height);
            InitShaders();
        }

       
        private void openGlControl_Resize(object sender, EventArgs e)
        {
           Resize(openGlControl.Width, openGlControl.Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            openGlControl.Width = this.ClientRectangle.Width-24;
            openGlControl.Height = this.ClientRectangle.Height-24;
        }


        private int CreateTexture(Bitmap bitmap, TextureTarget target, bool IsRepeated = false, bool IsSmooth = true)
        {
            try
            {
                int TextureID = 0;
                GL.GenTextures(1, out TextureID);

                GL.BindTexture(target, TextureID);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);

                // Setup filtering
                GL.TexParameter(target, TextureParameterName.TextureWrapS, IsRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge));
                GL.TexParameter(target, TextureParameterName.TextureWrapT, IsRepeated ? Convert.ToInt32(TextureWrapMode.Repeat) : Convert.ToInt32(TextureWrapMode.ClampToEdge));
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, IsSmooth ? Convert.ToInt32(TextureMagFilter.Linear) : Convert.ToInt32(TextureMagFilter.Nearest));
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, IsSmooth ? Convert.ToInt32(TextureMinFilter.Linear) : Convert.ToInt32(TextureMinFilter.Nearest));

                return TextureID;
            }
            catch
            {
                return -1;
            }
        }

    }

}
