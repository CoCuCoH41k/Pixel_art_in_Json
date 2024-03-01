namespace Structures
{
    internal enum Block_ID
    {
        Pixel = 917,
        Quadro_Pixel = 916,
        Full_Pixel = 955
    }

    internal class Block
    {
        public int obj_id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public int color_id { get; set; }
        public Block(int OBJ_ID, float X, float Y, int COLOR_ID)
        {
            obj_id = OBJ_ID;
            x = X;
            y = Y;
            color_id = COLOR_ID;
        }

        public bool Equals(Block other) => this.obj_id == other.obj_id && this.x == other.x && this.y == other.y && this.color_id == other.color_id;
    }

    internal class Color
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }
        public int a { get; set; }
        public Color(int R, int G, int B, int A)
        {
            r = R; g = G; b = B; a = A;
        }

        public Color(System.Drawing.Color color)
        {
            r = color.R; g = color.G; b = color.B; a = color.A;
        }

        public int[] ToArray() => new int[4] { this.r, this.g, this.b, this.a };

        public bool Equals(Color other) => this.r == other.r && this.g == other.g && this.b == other.b && this.a == other.a;

    }

    internal class JSON_Image
    {
        public int[] size { get; set; } = new int[2];
        public Array[] Blocks { get; set; }
        public Array[] Colors_set { get; set; }
        public JSON_Image(int[] size, Color[] colors_set, Block[] blocks)
        {
            this.size = size;
            Blocks = new Array[blocks.Length];
            Colors_set = new Array[colors_set.Length];

            for (int i = 0; i < blocks.Length; i++) this.Blocks[i] = new float[4] { blocks[i].obj_id, blocks[i].x, blocks[i].y, blocks[i].color_id };
            for (int i = 0; i < colors_set.Length; i++) this.Colors_set[i] = new float[4] { colors_set[i].r, colors_set[i].g, colors_set[i].b, colors_set[i].a };
        }
    }
}
