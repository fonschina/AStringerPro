using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace GMap.NET.WindowsForms
{
    public class Render: GLControl
    {
        public Graphics graphicsObjectGDIP;
        private bool started = false;
        private Image objBitmap = new Bitmap(150, 150);

        public bool opengl { get; set; }

        public Render():  this(true)
        {
          //  MakeCurrent();
        }

        public Render(bool dontcare)
        {

            opengl = true;

            graphicsObjectGDIP = System.Drawing.Graphics.FromImage(objBitmap);

            try
            {





            }
            catch (Exception ex) { }

            started = true;
        }


        public static implicit operator Render(Graphics g)
        {
            return new Render(g);
        }

        public Render(Graphics graphics) : this(true)
        {
            //IWindowInfo wi = Utilities.CreateWindowsWindowInfo(graphics.GetHdc());

            //IGraphicsContext context = new GraphicsContext(this.GraphicsMode, wi);
            //context.MakeCurrent(wi);

            //MakeCurrent();

            this.graphicsObjectGDIP = graphics;
        }

        private bool npotSupported { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            int[] viewPort = new int[4];

            GL.GetInteger(GetPName.Viewport, viewPort);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.PushAttrib(AttribMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Texture2D); 
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            string versionString = GL.GetString(StringName.Version);
            string majorString = versionString.Split(' ')[0];
            var v = new Version(majorString);
            npotSupported = v.Major >= 2;

            base.OnLoad(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                if (opengl && !DesignMode)
                {
                    base.OnHandleCreated(e);
                }
            }
            catch (Exception ex) { opengl = false; } // macs/linux fail here
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                if (opengl && !DesignMode)
                {
                    base.OnHandleDestroyed(e);
                }
            }
            catch (Exception ex) {  opengl = false; }
        }



        protected override void OnPaintBackground(PaintEventArgs e)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {

            base.OnPaint(e);
        }

        public override void Refresh()
        {
            //base.Refresh();
        }

        protected override void OnResize(EventArgs e)
        {
            if (DesignMode || !IsHandleCreated || !started)
                return;

            base.OnResize(e);

            graphicsObjectGDIP = System.Drawing.Graphics.FromImage(objBitmap);

            try
            {
                foreach (character texid in charDict.Values)
                {
                    try
                    {
                        texid.bitmap.Dispose();
                    }
                    catch { }
                }
                charDict.Clear();

                if (opengl)
                {
                    foreach (character texid in charDict.Values)
                    {
                        if (texid.gltextureid != 0)
                            GL.DeleteTexture(texid.gltextureid);
                    }
                }
            }
            catch { }

            try
            {
                if (opengl)
                {
                    MakeCurrent();

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, Width, Height, 0, -1, 1);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();

                    GL.Viewport(0, 0, Width, Height);
                }
            }
            catch { }
        }

        public void FillPie(SolidBrush coloruse, float v1, float v2, float widthc1, float widthc2, int v3, int v4)
        {
            //throw new NotImplementedException();
        }

        public void Clear(Color color)
        {
            if (opengl)
            {
                GL.ClearColor(color);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
            else
            {
                graphicsObjectGDIP.Clear(color);
            }
        }

        public void FillPie(SolidBrush solidBrush, int x, int y, int widtharc, int heightarc, int v1, int v2)
        {
            //throw new NotImplementedException();
        }

        const float rad2deg = (float)(180 / Math.PI);
        const float deg2rad = (float)(1.0 / rad2deg);

        public void DrawArc(Pen penn, RectangleF rect, float start, float degrees)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineStrip);

                start = 360 - start;
                start -= 30;

                float x = 0, y = 0;
                for (float i = start; i <= start + degrees; i++)
                {
                    x = (float)Math.Sin(i * deg2rad) * rect.Width / 2;
                    y = (float)Math.Cos(i * deg2rad) * rect.Height / 2;
                    x = x + rect.X + rect.Width / 2;
                    y = y + rect.Y + rect.Height / 2;
                    GL.Vertex2(x, y);
                }
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawArc(penn, rect, start, degrees);
            }
        }

        public void DrawEllipse(Pen penn, Rectangle rect)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineLoop);
                float x, y;
                for (float i = 0; i < 360; i += 1)
                {
                    x = (float)Math.Sin(i * deg2rad) * rect.Width / 2;
                    y = (float)Math.Cos(i * deg2rad) * rect.Height / 2;
                    x = x + rect.X + rect.Width / 2;
                    y = y + rect.Y + rect.Height / 2;
                    GL.Vertex2(x, y);
                }
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawEllipse(penn, rect);
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.ClearOutputChannelColorProfile();
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        int texture;
        Bitmap bitmap = new Bitmap(512, 512);

        public void DrawImage(Image image, Rectangle rectangle, float srcX, float srcY, float srcWidth, float srcHeight,
            GraphicsUnit graphicsUnit, ImageAttributes tileFlipXYAttributes)
        {
            //graphicsObjectGDIP.DrawImage(image, rectangle, srcX, srcY, srcWidth, srcHeight, graphicsUnit, TileFlipXYAttributes);

            DrawImage(image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public void DrawImage(Image image, Rectangle rectangle, int p1, int p2, long p3, long p4, GraphicsUnit graphicsUnit, ImageAttributes TileFlipXYAttributes)
        {
            DrawImage(image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public void DrawImage(Image img, long x, long y, long width, long height)
        {
            DrawImage(img, (int)x, (int)y, (int)width, (int)height);
        }

        private character[] _texture = new character[2];

        public void DrawImage(Image img, int x, int y, int width, int height, int textureno = 0)
        {
            if (opengl)
            {
                if (img == null)
                    return;

                if (_texture[textureno] == null)
                    _texture[textureno] = new character();

                bool npotSupported = true;

                // If the image is already a bitmap and we support NPOT textures then simply use it.
                if (npotSupported && img is Bitmap)
                {
                    _texture[textureno].bitmap = (Bitmap)img;
                }
                else
                {
                    // Otherwise we have to resize img to be POT.
                    _texture[textureno].bitmap = ResizeImage(img, _texture[textureno].bitmap.Width, _texture[textureno].bitmap.Height);
                }

                // generate the texture
                if (_texture[textureno].gltextureid == 0)
                {
                    GL.GenTextures(1, out _texture[textureno].gltextureid);
                }

                GL.BindTexture(TextureTarget.Texture2D, _texture[textureno].gltextureid);

                BitmapData data = _texture[textureno].bitmap.LockBits(
                    new Rectangle(0, 0, _texture[textureno].bitmap.Width, _texture[textureno].bitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // create the texture type/dimensions
                if (_texture[textureno].width != _texture[textureno].bitmap.Width)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                    _texture[textureno].width = data.Width;
                }
                else
                {
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, data.Width, data.Height,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                }

                _texture[textureno].bitmap.UnlockBits(data);

                bool polySmoothEnabled = GL.IsEnabled(EnableCap.PolygonSmooth);
                if (polySmoothEnabled)
                    GL.Disable(EnableCap.PolygonSmooth);

                GL.Enable(EnableCap.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, _texture[textureno].gltextureid);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                GL.Begin(PrimitiveType.TriangleStrip);

                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(x, y);
                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(x, y + height);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(x + width, y);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(x + width, y + height);

                GL.End();

                GL.Disable(EnableCap.Texture2D);

                if (polySmoothEnabled)
                    GL.Enable(EnableCap.PolygonSmooth);
            }
            else
            {
                graphicsObjectGDIP.DrawImage(img, x, y, width, height);
            }
        }

        public void DrawPath(Pen penn, GraphicsPath gp)
        {
            try
            {
                DrawPolygon(penn, gp.PathPoints);
            }
            catch { }
        }

        public void FillPath(Brush brushh, GraphicsPath gp)
        {
            try
            {
                FillPolygon(brushh, gp.PathPoints);
            }
            catch { }
        }

        public void SetClip(Rectangle rect)
        {

        }

        public void ResetClip()
        {

        }

        public void ResetTransform()
        {
            if (opengl)
            {
                GL.LoadIdentity();
            }
            else
            {
                graphicsObjectGDIP.ResetTransform();
            }
        }

        private double xscale, yscale;

        public void ScaleTransform(float xscale, float yscale, MatrixOrder mat)
        {
            this.xscale = xscale;
            this.yscale = yscale;
            Console.WriteLine("ScaleTransform " + xscale + " " + yscale);
            if (opengl)
            {
                GL.Scale(xscale, yscale, 1);
            }
            else
            {
                graphicsObjectGDIP.ScaleTransform(xscale, yscale, mat);
            }
        }

        public void RotateTransform(float angle)
        {
            if (opengl)
            {
                GL.Rotate(angle, 0, 0, 1);
            }
            else
            {
                graphicsObjectGDIP.RotateTransform(angle);
            }
        }

        public void TranslateTransform(float x, float y, MatrixOrder mat = MatrixOrder.Prepend)
        {
            Console.WriteLine("TranslateTransform " + x + " " + y);
            if (opengl)
            {
                GL.Translate(x, y, 0f);
            }
            else
            {
                var before = graphicsObjectGDIP.Transform;
                graphicsObjectGDIP.TranslateTransform(x, y, mat);
                var after = graphicsObjectGDIP.Transform;
            }
        }

        public void FillPolygon(Brush brushh, Point[] list)
        {
            if (opengl)
            {
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Color4(((SolidBrush)brushh).Color);
                foreach (Point pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }
                GL.Vertex2(list[list.Length - 1].X, list[list.Length - 1].Y);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.FillPolygon(brushh, list);
            }
        }

        public void FillPolygon(Brush brushh, PointF[] list)
        {
            if (opengl)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(((SolidBrush)brushh).Color);
                foreach (PointF pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }
                GL.Vertex2(list[0].X, list[0].Y);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.FillPolygon(brushh, list);
            }
        }

        public void DrawPolygon(Pen penn, Point[] list)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineLoop);
                foreach (Point pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawPolygon(penn, list);
            }
        }

        public void DrawPolygon(Pen penn, PointF[] list)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineLoop);
                foreach (PointF pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }

                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawPolygon(penn, list);
            }
        }


        public void FillRectangle(Brush brushh, long x, long y, long width, long height)
        {
            FillRectangle(brushh, new RectangleF(x, y, width, height));
        }

        public void FillRectangle(Brush brushh, RectangleF rectf)
        {
            if (opengl)
            {
                float x1 = rectf.X;
                float y1 = rectf.Y;

                float width = rectf.Width;
                float height = rectf.Height;

                GL.Begin(PrimitiveType.Quads);

                GL.LineWidth(0);

                if (((Type)brushh.GetType()) == typeof(LinearGradientBrush))
                {
                    LinearGradientBrush temp = (LinearGradientBrush)brushh;
                    GL.Color4(temp.LinearColors[0]);
                }
                else
                {
                    GL.Color4(((SolidBrush)brushh).Color.R / 255f, ((SolidBrush)brushh).Color.G / 255f, ((SolidBrush)brushh).Color.B / 255f, ((SolidBrush)brushh).Color.A / 255f);
                }

                GL.Vertex2(x1, y1);
                GL.Vertex2(x1 + width, y1);

                if (((Type)brushh.GetType()) == typeof(LinearGradientBrush))
                {
                    LinearGradientBrush temp = (LinearGradientBrush)brushh;
                    GL.Color4(temp.LinearColors[1]);
                }
                else
                {
                    GL.Color4(((SolidBrush)brushh).Color.R / 255f, ((SolidBrush)brushh).Color.G / 255f, ((SolidBrush)brushh).Color.B / 255f, ((SolidBrush)brushh).Color.A / 255f);
                }

                GL.Vertex2(x1 + width, y1 + height);
                GL.Vertex2(x1, y1 + height);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.FillRectangle(brushh, rectf);
            }
        }

        public void DrawRectangle(Pen penn, RectangleF rect)
        {
            DrawRectangle(penn, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawRectangle(Pen penn, double x1, double y1, double width, double height)
        {

            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(x1, y1);
                GL.Vertex2(x1 + width, y1);
                GL.Vertex2(x1 + width, y1 + height);
                GL.Vertex2(x1, y1 + height);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawRectangle(penn, (float)x1, (float)y1, (float)width, (float)height);
            }
        }

        public void DrawLine(Pen penn, double x1, double y1, double x2, double y2)
        {

            if (opengl)
            {
                GL.Color4(penn.Color);
                GL.LineWidth(penn.Width);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2(x1, y1);
                GL.Vertex2(x2, y2);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawLine(penn, (float)x1, (float)y1, (float)x2, (float)y2);
            }
        }


        /// <summary>
        /// pen for drawstring
        /// </summary>
        Pen P = new Pen(Color.FromArgb(0x26, 0x27, 0x28), 2f);
        /// <summary>
        /// pth for drawstring
        /// </summary>
        GraphicsPath pth = new GraphicsPath();

        Dictionary<int, character> charDict = new Dictionary<int, character>();


        class character
        {
            public Bitmap bitmap;
            public int gltextureid;
            public int width;
            public int size;
        }


        public void DrawString(string EmptyTileText, System.Drawing.Font MissingDataFont, Brush brush, RectangleF rectangleF, StringFormat CenterFormat)
        {
            DrawString(EmptyTileText, MissingDataFont, brush, rectangleF);
            //throw new NotImplementedException();
        }

        public void DrawString(string p1, System.Drawing.Font CopyrightFont, Brush brush, int p2, int p3)
        {
            drawstring(p1, Font, Font.SizeInPoints, (SolidBrush)brush, p2, p3);
        }

        public void DrawString(string p, System.Drawing.Font Font, Brush brush, RectangleF rectangleF)
        {
            drawstring(p, Font, Font.SizeInPoints, (SolidBrush)brush, rectangleF.X, rectangleF.Y);
        }

        void drawstring(string text, Font font, float fontsize, SolidBrush brush, float x, float y)
        {
            if (!opengl)
            {
                drawstring(graphicsObjectGDIP, text, font, fontsize, brush, x, y);
                return;
            }

            if (text == null || text == "")
                return;

            char[] chars = text.ToCharArray();

            float maxy = 1;

            foreach (char cha in chars)
            {
                int charno = (int)cha;

                int charid = charno ^ (int)(fontsize * 1000) ^ brush.Color.ToArgb();

                if (!charDict.ContainsKey(charid))
                {
                    charDict[charid] = new character() { bitmap = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb), size = (int)fontsize };

                    charDict[charid].bitmap.MakeTransparent(Color.Transparent);

                    //charbitmaptexid

                    float maxx = this.Width / 150; // for space


                    // create bitmap
                    using (Graphics gfx = System.Drawing.Graphics.FromImage(charDict[charid].bitmap))
                    {
                        pth.Reset();

                        if (text != null)
                            pth.AddString(cha + "", font.FontFamily, 0, fontsize + 5, new Point((int)0, (int)0), StringFormat.GenericTypographic);

                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        gfx.DrawPath(P, pth);

                        //Draw the face

                        gfx.FillPath(brush, pth);


                        if (pth.PointCount > 0)
                        {
                            foreach (PointF pnt in pth.PathPoints)
                            {
                                if (pnt.X > maxx)
                                    maxx = pnt.X;

                                if (pnt.Y > maxy)
                                    maxy = pnt.Y;
                            }
                        }
                    }

                    charDict[charid].width = (int)(maxx + 2);

                    //charbitmaps[charid] = charbitmaps[charid].Clone(new RectangleF(0, 0, maxx + 2, maxy + 2), charbitmaps[charid].PixelFormat);

                    //charbitmaps[charno * (int)fontsize].Save(charno + " " + (int)fontsize + ".png");

                    // create texture
                    int textureId;
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvModeCombine.Replace);//Important, or wrong color on some computers

                    Bitmap bitmap = charDict[charid].bitmap;
                    GL.GenTextures(1, out textureId);
                    GL.BindTexture(TextureTarget.Texture2D, textureId);

                    BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                    //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    GL.Finish();
                    bitmap.UnlockBits(data);

                    charDict[charid].gltextureid = textureId;
                }

                //GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, charDict[charid].gltextureid);

                float scale = 1.0f;

                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0, 0); GL.Vertex2(x, y);
                GL.TexCoord2(1, 0); GL.Vertex2(x + charDict[charid].bitmap.Width * scale, y);
                GL.TexCoord2(1, 1); GL.Vertex2(x + charDict[charid].bitmap.Width * scale, y + charDict[charid].bitmap.Height * scale);
                GL.TexCoord2(0, 1); GL.Vertex2(x + 0, y + charDict[charid].bitmap.Height * scale);
                GL.End();

                //GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.Texture2D);

                x += charDict[charid].width * scale;
            }
        }



        public void drawstring(Graphics e, string text, Font font, float fontsize, SolidBrush brush, float x, float y)
        {
            if (text == null || text == "")
                return;


            char[] chars = text.ToCharArray();

            float maxy = 0;

            foreach (char cha in chars)
            {
                int charno = (int)cha;

                int charid = charno ^ (int)(fontsize * 1000) ^ brush.Color.ToArgb();

                if (!charDict.ContainsKey(charid))
                {
                    charDict[charid] = new character() { bitmap = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb), size = (int)fontsize };

                    charDict[charid].bitmap.MakeTransparent(Color.Transparent);

                    //charbitmaptexid

                    float maxx = this.Width / 150; // for space


                    // create bitmap
                    using (Graphics gfx = System.Drawing.Graphics.FromImage(charDict[charid].bitmap))
                    {
                        pth.Reset();

                        if (text != null)
                            pth.AddString(cha + "", font.FontFamily, 0, fontsize + 5, new Point((int)0, (int)0), StringFormat.GenericTypographic);

                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        gfx.DrawPath(P, pth);

                        //Draw the face

                        gfx.FillPath(brush, pth);


                        if (pth.PointCount > 0)
                        {
                            foreach (PointF pnt in pth.PathPoints)
                            {
                                if (pnt.X > maxx)
                                    maxx = pnt.X;

                                if (pnt.Y > maxy)
                                    maxy = pnt.Y;
                            }
                        }
                    }

                    charDict[charid].width = (int)(maxx + 2);
                }

                // draw it

                float scale = 1.0f;

                DrawImage(charDict[charid].bitmap, (int)x, (int)y, charDict[charid].bitmap.Width, charDict[charid].bitmap.Height);

                x += charDict[charid].width * scale;
            }

        }



        public TextRenderingHint TextRenderingHint { get; set; }
        public SmoothingMode SmoothingMode { get; set; }
        public CompositingMode CompositingMode { get; set; }
        public InterpolationMode InterpolationMode { get; set; }
        public Matrix Transform { get; set; }

        internal void DrawImage(Image backBuffer, int v1, int v2)
        {
            DrawImage(backBuffer, v1, v2, backBuffer.Width, backBuffer.Height);
        }

        public SizeF MeasureString(string toolTipText, Font font)
        {
            return graphicsObjectGDIP.MeasureString(toolTipText, font);
        }

        public void DrawImageUnscaled(Bitmap icong, int v1, int v2)
        {
            //throw new NotImplementedException();
        }

        public void FillEllipse(Brush red, Rectangle rectangle)
        {
            //throw new NotImplementedException();
        }

        public void DrawArc(Pen pen, float p1, float p2, float v1, float v2, float cog, float alpha)
        {
            //throw new NotImplementedException();
        }
    }
}