using System;
using System.IO;
using System.Linq;

namespace PZZmanager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PZZ Manager v1.0.0\nBy: Penguino\n");
            if (args.Length == 0) Console.WriteLine("Usage: PZZmanager.exe (-p / -r) FOLDER\n|| -p: repack || -r: rename ||\n\nExample: PZZmanager.exe -p \"pl20.pzz_unpack\"");
            if (args.Length == 1) Console.WriteLine("No file or folder input provided.");

            //PZZ Repacker
            if (args.Length == 2 && args[0] == "-p" && !Directory.Exists(args[1])) Console.WriteLine("Directory doesn't exist.");
            if (args.Length == 2 && args[0] == "-p" && Directory.Exists(args[1]))
            {
                Console.WriteLine("// PZZ Repack //");
                var newfile = File.Create(args[1] + ".pzz");
                newfile.Close();
                byte[] newpzz = File.ReadAllBytes(args[1] + ".pzz");
                string filefolder = args[1];
                bool compressed;
                int filecount = 0;
                Array.Resize(ref newpzz, 2048);
                Console.WriteLine("Packing " + args[1] + "...");
                foreach (string file in Directory.EnumerateFiles(filefolder, "*.dat"))
                {
                    filecount++;
                    string filename = Path.GetFileName(file);
                    byte[] filedata = File.ReadAllBytes(file);
                    int filesize = filedata.Length;
                    int sectorsize = 2048;
                    int sectorcount = 0;



                    if (filename.Contains("_compressed")) compressed = true;
                    else compressed = false;

                    string fideID = Path.GetFileNameWithoutExtension(filename).Split('_').Last(); //get file id from filename
                    string fileID = Path.GetFileNameWithoutExtension(filename).Split('_').Reverse().Take(2).Last(); //get file id from 'compressed' filename 

                    for (int i = 0; filesize > i; i += 2048) //calculate filesize sectors
                    {
                        sectorcount = i / 2048;
                        sectorsize = i;
                    }

                    int finalsectorcount = sectorcount + 1;
                    int finalsectorsize = sectorsize + 2048; //ending pad
                    byte[] secsizebytes = BitConverter.GetBytes(finalsectorcount);

                    if (compressed == true)
                    {
                        byte tmpfilecountcom = Convert.ToByte(Int32.Parse(fileID) + 1);
                        Console.WriteLine("ID:" + fileID);
                        Buffer.SetByte(newpzz, 0, tmpfilecountcom); //update file count
                        Buffer.BlockCopy(secsizebytes, 0, newpzz, 0x0 + 0x4 * Int32.Parse(fileID) + 0x4, 2); // 0x800 byte sector count
                        Buffer.SetByte(newpzz, 0x4 * Int32.Parse(fileID) + 0x3 + 0x4, 0x80); //set compression flag
                    }
                    if (compressed == false)
                    {
                        byte tmpfilecountdec = Convert.ToByte(Int32.Parse(fideID) + 1);
                        Console.WriteLine("ID:" + fideID);
                        Buffer.SetByte(newpzz, 0, tmpfilecountdec); //update file count
                        Buffer.BlockCopy(secsizebytes, 0, newpzz, 0x0 + 0x4 * Int32.Parse(fideID) + 0x4, 2); // 0x800 byte sector count
                        Buffer.SetByte(newpzz, 0x4 * Int32.Parse(fideID) + 0x3 + 0x4, 0x00); //remove compression flag
                    }

                    //Console.WriteLine("Sectors:" + finalsectorcount + "\nsectorsize:" + finalsectorsize + "\nfilesize:" + filesize);
                    Array.Resize(ref filedata, finalsectorsize);
                    int oldsize = (newpzz.Length);
                    Array.Resize(ref newpzz, newpzz.Length + finalsectorsize);
                    Buffer.BlockCopy(filedata, 0, newpzz, oldsize, finalsectorsize);
                }
                File.WriteAllBytes(args[1] + ".pzz", newpzz);
                Console.WriteLine("All done.");
            }

            //PZZ Renamer
            if (args.Length == 2 && args[0] == "-r" && !Directory.Exists(args[1])) Console.WriteLine("Directory doesn't exist."); //dat rename
            if (args.Length == 2 && args[0] == "-r" && Directory.Exists(args[1]))
            {
                Console.WriteLine("// PZZ Renamer //");

                Console.WriteLine("Renaming...");
                string filefolder = args[1];

                if (filefolder.Contains("_unpack")) //for unpack folders with compressed files
                {
                    foreach (string file in Directory.EnumerateFiles(filefolder, "*.dat"))
                    {
                        string filename = Path.GetFileName(file);
                        byte[] contents = File.ReadAllBytes(file);
                        if (filename.Contains("_compressed")) Console.WriteLine("Skipping compressed files...");
                        else
                        {
                            int magic0x0 = Buffer.GetByte(contents, 0x0);
                            int magic0x1 = Buffer.GetByte(contents, 0x1);
                            int magic0x2 = Buffer.GetByte(contents, 0x2);
                            int magic0x3 = Buffer.GetByte(contents, 0x3);
                            int magic0x4 = Buffer.GetByte(contents, 0x4);
                            int magic0x5 = Buffer.GetByte(contents, 0x5);
                            int magic0x6 = Buffer.GetByte(contents, 0x6);
                            int magic0x7 = Buffer.GetByte(contents, 0x7);

                            int TXBmagic1 = Buffer.GetByte(contents, 0x200);
                            int TXBmagic2 = Buffer.GetByte(contents, 0x201);
                            int TXBmagic3 = Buffer.GetByte(contents, 0x202);
                            int TXBmagic4 = Buffer.GetByte(contents, 0x203);

                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".amo")) break;
                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".txb")) break;
                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".aan")) break;
                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".tbl")) break;
                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".sdt")) break;
                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".hit")) break;
                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".hits")) break;
                            if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".skl")) break;

                            if (magic0x3 == 00 && magic0x4 == 04 && magic0x5 == 00 && magic0x6 == 00 && magic0x7 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".amo");
                            if (magic0x4 == 64 && magic0x5 == 00 && magic0x6 == 00 && magic0x7 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".aan");
                            if (magic0x4 == 00 && TXBmagic1 == 84 && TXBmagic2 == 73 && TXBmagic3 == 77 && TXBmagic4 == 50) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".txb");
                            if (magic0x4 == 00 && magic0x5 == 00 && magic0x6 == 05 && magic0x7 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".sdt");
                            if (magic0x0 == 32 && magic0x1 == 00 && magic0x2 == 00 && magic0x3 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".hit");
                            if (magic0x0 == 72 && magic0x1 == 73 && magic0x2 == 84 && magic0x3 == 83) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".hits");
                            if (magic0x0 == 00 && magic0x1 == 00 && magic0x2 == 00 && magic0x3 == 192) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".skl");
                        }
                    }
                }

                else foreach (string file in Directory.EnumerateFiles(filefolder, "*.bin")) //for decompressed folders
                    {
                        //penguino drops worst code ever

                        byte[] contents = File.ReadAllBytes(file);

                        int magic0x0 = Buffer.GetByte(contents, 0x0);
                        int magic0x1 = Buffer.GetByte(contents, 0x1);
                        int magic0x2 = Buffer.GetByte(contents, 0x2);
                        int magic0x3 = Buffer.GetByte(contents, 0x3);
                        int magic0x4 = Buffer.GetByte(contents, 0x4);
                        int magic0x5 = Buffer.GetByte(contents, 0x5);
                        int magic0x6 = Buffer.GetByte(contents, 0x6);
                        int magic0x7 = Buffer.GetByte(contents, 0x7);

                        int TXBmagic1 = Buffer.GetByte(contents, 0x200);
                        int TXBmagic2 = Buffer.GetByte(contents, 0x201);
                        int TXBmagic3 = Buffer.GetByte(contents, 0x202);
                        int TXBmagic4 = Buffer.GetByte(contents, 0x203);

                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".amo")) break;
                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".txb")) break;
                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".aan")) break;
                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".tbl")) break;
                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".sdt")) break;
                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".hit")) break;
                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".hits")) break;
                        if (File.Exists(Path.GetFileNameWithoutExtension(file) + ".skl")) break;

                        //amo - model
                        //txb - textures
                        //aan - animation
                        //tbl - misc animation
                        //sdt - shadow data
                        //hit - player collsion
                        //hits - stage collision
                        //skl - armature data 

                        if (magic0x4 == 04 && magic0x5 == 00 && magic0x6 == 00 && magic0x7 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".amo");
                        if (magic0x4 == 64 && magic0x5 == 00 && magic0x6 == 00 && magic0x7 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".aan");
                        if (magic0x4 == 00 && TXBmagic1 == 84 && TXBmagic2 == 73 && TXBmagic3 == 77 && TXBmagic4 == 50) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".txb");
                        if (magic0x4 == 00 && magic0x5 == 00 && magic0x6 == 05 && magic0x7 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".sdt");
                        if (magic0x0 == 32 && magic0x1 == 00 && magic0x2 == 00 && magic0x3 == 00) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".hit");
                        if (magic0x0 == 72 && magic0x1 == 73 && magic0x2 == 84 && magic0x3 == 83) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".hits");
                        if (magic0x0 == 00 && magic0x1 == 00 && magic0x2 == 00 && magic0x3 == 192) File.Move(file, Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + ".skl"); //not sure what other extension to give it
                    }
                Console.WriteLine("All done.");
            }
        }
    }
}
