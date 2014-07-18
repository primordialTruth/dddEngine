using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace dddEngine
{
    class Texture
    {
        private Bitmap bmp;
        private string name;

        //things ill probably need but am unsure about now
        //---some sort of coordinate system for the texture

        public Texture(string loc)
        {
            this.name = loc;
            this.bmp =new Bitmap(loc);
        }

        public Bitmap getBMP(){return this.bmp;}
        public void setBMP(Bitmap inbmp){this.bmp = inbmp;}
    }
}
