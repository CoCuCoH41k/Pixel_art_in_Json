using Structures;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text.Json;

class MainClass
{
    public static List<JSON_Image_raw> JSONI_list = new List<JSON_Image_raw>();
    public static List<Thread> Thread_list = new List<Thread>(); 
    public static bool threads_done = false;

    public static void Threads_check()
    {
        while (!threads_done)
        {
            bool flag = false;
            foreach (Thread t in Thread_list)
            {

                if (t.ThreadState == System.Threading.ThreadState.Running) flag = false;
                if (t.ThreadState == System.Threading.ThreadState.Stopped) flag = true;
            }
            if (flag) break;
        }

        threads_done = true;
    }

    public static bool Color_exist(Structures.Color Color, List<Structures.Color> pallete)
    {
        if (pallete == null) return false;

        foreach (Structures.Color clr in pallete)
        {
            if (clr.Equals(Color)) return true;
        }

        return false;
    }

    public static int Serch_clr_id(Structures.Color color, List<Structures.Color> pallete)
    {
        if (!Color_exist(color, pallete)) {
            return -1;
        } else
        {
            for (int id = 0; id < pallete.Count; id++)
            {
                if (pallete[id].Equals(color)) return id;
            }
            return -1;
        }
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

    public static void Convert_Image(Bitmap image)
    {
        image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        Size size = image.Size;
        Block[] blocks = new Block[size.Width * size.Height];
        List<Structures.Color> pallete = new List<Structures.Color>();
        
        for (int index = 0; index < size.Width * size.Height; index++)
        {
            int xPos = index % size.Width;
            int yPos = (int)(index / size.Width);
            Structures.Color curr_color = new Structures.Color(image.GetPixel(xPos, yPos));

            if (!Color_exist(curr_color, pallete)) pallete.Add(curr_color);

            blocks[index] = new Block(((int)Block_ID.Pixel), xPos, yPos, Serch_clr_id(curr_color, pallete));
        }

        List<Block> blocks_to_check = new List<Block>();
        foreach (Block block in blocks)
        {
            if (pallete[block.color_id].a == 0) continue;
            blocks_to_check.Add(block);
        }

        int[] arr_size = new int[2] { size.Width, size.Height };
        Block[] new_blocks = Optimize(blocks_to_check.ToArray<Block>(), size);

        Console.WriteLine($"Optimization: {blocks.Length} -> {new_blocks.Length} blocks");

        JSONI_list.Add(new JSON_Image_raw(arr_size, new_blocks, pallete));
    }

    public static void For_art(string folder_path)
    {
        string[] files = Directory.GetFiles(folder_path);
        foreach (string file in files)
        {
            Console.WriteLine($"Art to convert: {file.Split("\\").Last()}");
            Bitmap img = new Bitmap(file);

            Thread art_converting = new Thread(() => Convert_Image(img));
            
            Thread_list.Add(art_converting);
            //art_converting.Start();
        }

        foreach (Thread thread in Thread_list)
        {
            thread.Start();

            thread.Join();

            
        }
    }

    public static JSON_Image Merge_JSONI()
    {
        int[] size_end = new int[2];
        Block[] blocks_end = { };
        List<Structures.Color> pallete_end = new List<Structures.Color>();

        for (int index = 0; index < JSONI_list.Count; index++)
        {
            List<Structures.Color> colors = JSONI_list[index].Colors_set.ToList();

            foreach(var color in colors)
            {
                if (!Color_exist(color, pallete_end))
                {
                    pallete_end.Add(color);
                }
            }
        }

        for (int index = 0; index < JSONI_list.Count; index++)
        {
            Block[] blocks = JSONI_list[index].Blocks;
            List<Structures.Color> curr_pallete = JSONI_list[index].Colors_set;

            
            int local_size = JSONI_list[index].size[0];
            Block[] to_merge = new Block[blocks.Length];

            for (int j = 0; j < blocks.Length; j++)
            {
                int new_color_id = Serch_clr_id(curr_pallete[blocks[j].color_id], pallete_end);
                to_merge[j] = new Block(blocks[j].obj_id, blocks[j].x + size_end[0] + 4 * index, blocks[j].y, new_color_id);
            }
            blocks_end = blocks_end.Concat(to_merge).ToArray();
            size_end[0] += JSONI_list[index].size[0];
            size_end[1] = Math.Max(size_end[1], JSONI_list[index].size[1]);
        }

        return new JSON_Image(size_end, pallete_end.ToArray(), blocks_end);
    }

    static void Main(string[] args)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        Stopwatch Converting = new Stopwatch();
        Stopwatch Merging = new Stopwatch();

        string path = null; //"C:\\Users\\DENIS\\Desktop\\json_files\\";
        Console.Write($"Write folder path where the arts located ( Example: {@"C:\Users\USER_NAME\Desktop\art_files"} )\n-> ");
        path = Console.ReadLine();
        if (path.Length == 0)
        {
            Console.WriteLine("Path can't be empty. bro =/");
            Environment.Exit(0);
        }

        string file_result_path = null; // "C:\\Users\\DENIS\\Desktop\\gd gameplay deltarune-undertale\\Undertale-Deltarune-gameplay-in-GD-with-SPWN\\"
        Console.Write($"Write folder path where the converted arts will be located ( Example: {@"C:\Users\USER_NAME\Desktop\art_files"} )\n-> ");
        file_result_path = Console.ReadLine();
        if (file_result_path.Length == 0)
        {
            Console.WriteLine("Path can't be empty. bro =/");
            Environment.Exit(0);
        }

        string name = "art.json";

        Converting.Start();
        
        For_art(path);
        Threads_check();

        Converting.Stop();

        Console.WriteLine($"Complete (1 / 2) converting in: {Converting.ElapsedMilliseconds} milliseconds");

        Merging.Start();

        JSON_Image result = Merge_JSONI();
        string jsonString = JsonSerializer.Serialize(result, options);

        Merging.Stop();

        File.WriteAllText(file_result_path + "\\" + name, jsonString);
        Console.WriteLine($"Complete (2 / 2) merging in: {Merging.ElapsedMilliseconds} milliseconds");

        Console.WriteLine($"All done in: {Converting.ElapsedMilliseconds + Merging.ElapsedMilliseconds} milliseconds");
        Console.WriteLine(
            $"Blocks: {result.Blocks.Length}\n" +
            $"Colors: {result.Colors_set.Length}\n" +
            $"Size: {result.size[0]}x{result.size[1]}");
    }
}