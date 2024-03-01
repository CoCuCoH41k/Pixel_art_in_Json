using Structures;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;

class MainClass
{
    public static int NULL_cnt, COLOR_cnt = 0;
    public static List<Structures.Color>? pallete;

    public static bool Color_exist(Structures.Color Color)
    {
        if (pallete == null)
        {
            pallete = new List<Structures.Color>();
            pallete.Add(Color);
        }

        foreach (Structures.Color clr in pallete)
        {
            if (clr.Equals(Color)) return true;
        }

        pallete.Add(Color);
        return false;
    }

    public static int Serch_clr_id(Structures.Color color)
    {
        Color_exist(color);
        for (int id = 0; id < pallete.Count; id++)
        {
            if (pallete[id].Equals(color)) return id;
        }
        return -1;
    }

    public static int Get_block_index_by_pos(float x, float y, Block[] blocks)
    {
        Block to_check = null;
        foreach(Block block in blocks)
        {
            if (block.x == x && block.y == y) {
                to_check = block;
                break;
            }
        }
        return blocks.ToList().IndexOf(to_check);


    }

    public static Block Get_block_by_pos(float x, float y, Block[] blocks)
    {
        foreach (Block block in blocks)
        {
            if (block.x == x && block.y == y) 
            {
                return block;
            }
        }
        return null;
    }

    public static int Get_index_by_block(Block block, Block[] blocks)
    {
        return blocks.ToList().IndexOf(block);
    }

    public static bool Is_optimizeble(Block block, Block[] blocks_array, int size, Size img_size)
    {
        int x = (int)block.x;
        int y = (int)block.y;

        if (x + size > img_size.Width || y + size > img_size.Height) return false;
        for (int X = x; X < x + size; X++)
        {
            for (int Y = y; Y < y + size; Y++)
            {
                Block to_check = Get_block_by_pos(X, Y, blocks_array);
                if (to_check == null)
                {
                    return false;
                }
                if (to_check.color_id != block.color_id)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static Block[] Optimize(Block[] blocks, Size size)
    {
        // Optimization algorithm
        List<Block> new_blocks = blocks.ToList();
        List<Block> to_fill = new List<Block>();
        bool flag = false;
        


        for (int pixel_size = 4; pixel_size > 1; pixel_size -= 2)
        {
            List<Block> blocks_to_check = new_blocks.ToList();

            while (true)
            {
                flag = true;

                //List<Block> local_BTC = blocks_to_check;
                foreach (Block block in blocks_to_check.ToList())
                {
                    blocks_to_check.Remove(block);

                    if (Is_optimizeble(block, new_blocks.ToArray(), pixel_size, size))
                    {
                        flag = false;
                        float X = block.x;
                        float Y = block.y;

                        for (int x = (int)X; x < X + pixel_size; x++)
                        {
                            for (int y = (int)Y; y < Y + pixel_size; y++)
                            {
                                int to_remove = Get_block_index_by_pos(x, y, new_blocks.ToList().ToArray<Block>());
                                if (to_remove != -1) new_blocks.RemoveAt(to_remove);
                            }
                        }
                        bool condition = pixel_size == 4;
                        float offset_multipling = condition ? 1.5f : 0.5f;

                        to_fill.Add(new Block(
                                condition ? (int)Block_ID.Full_Pixel : (int)Block_ID.Quadro_Pixel,
                                block.x + offset_multipling,
                                block.y + offset_multipling,
                                block.color_id
                        ));

                        break;
                    }
                }
                if (flag)
                {
                    flag = false;
                    break;
                }
            }
        }
        foreach  (Block block in to_fill)
        {
            new_blocks.Add(block);
        }
        return new_blocks.ToArray();
    }

    public static JSON_Image Convert(Bitmap image)
    {
        image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        Size size = image.Size;
        Block[] blocks = new Block[size.Width * size.Height];
        
        for (int index = 0; index < size.Width * size.Height; index++)
        {
            int xPos = index % size.Width;
            int yPos = (int)(index / size.Width);
            Structures.Color curr_color = new Structures.Color(image.GetPixel(xPos, yPos));
            blocks[index] = new Block(((int)Block_ID.Pixel), xPos, yPos, Serch_clr_id(curr_color));
        }

        List<Block> blocks_to_check = new List<Block>();
        foreach (Block block in blocks)
        {
            if (pallete[block.color_id].a == 0) continue;
            blocks_to_check.Add(block);
        }

        int[] arr_size = new int[2] { size.Width, size.Height };

        Console.WriteLine($"Before optimization: {blocks.Length} blocks");

        Block[] new_blocks = Optimize(blocks_to_check.ToArray<Block>(), size);

        Console.WriteLine($"Optimization result: {new_blocks.Length} blocks");


        return new JSON_Image(arr_size, pallete.ToArray(), new_blocks);
    }
    
    static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();

        Bitmap img = new Bitmap("C:\\Users\\DENIS\\Desktop\\kris_walk.png", false);
        string fileName = "C:\\Users\\DENIS\\Desktop\\kris_walk.json";
         
        stopwatch.Start();
        string jsonString = JsonSerializer.Serialize(Convert(img));
        stopwatch.Stop();

        Console.WriteLine($"Complete in: {stopwatch.ElapsedMilliseconds} milliseconds");
        File.WriteAllText(fileName, jsonString);
    }
}