using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace XYSAV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public byte[] sav = new Byte[0x100000];

        private UInt16 ccitt16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) > 0)
                    {
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return crc;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open Save File
            OpenFileDialog opensav = new OpenFileDialog();
            opensav.Filter = "SAV|*.sav;*.bin";
            DialogResult result = opensav.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = opensav.FileName;
                string ext = Path.GetExtension(path);
                sav = File.ReadAllBytes(path);
                if (((sav.Length == 0x100000)))
                {
                    // Unlock GB
                    groupBox1.Enabled = true;
                }
                else
                {
                    string message = "Did not select a valid SAV";
                    string caption = "Input Error";
                    MessageBox.Show(message, caption);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Create Checksum
            uint offset = ToUInt32(T_Offset.Text);
            uint length = ToUInt32(T_Length.Text);
            byte[] data = new Byte[length];
            Array.Copy(sav, offset, data, 0, length);
            for (int i = 0; i < length; i++)
            {
                data[i] = sav[i + offset];
            }
            RTB.Text = ccitt16(data).ToString("X4");
        }
        private static uint ToUInt32(String value)
        {
            if (String.IsNullOrEmpty(value))
                return 0;
            return Convert.ToUInt32(value, 16);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Check all
            RTB.Text = "";
            int invalid = 0;
            uint[] start =  {
                               0x05400,	0x05800,	0x06400,	0x06600,	0x06800,	0x06A00,	0x06C00,	0x06E00,	0x07000,	0x07200,	0x07400,	0x09600,	0x09800,	0x09E00,	0x0A400,	0x0F400,	0x14400,	0x19400,	0x19600,	0x19E00,	0x1A400,	0x1AC00,	0x1B400,	0x1B600,	0x1B800,	0x1BE00,	0x1C000,	0x1C400,	0x1CC00,	0x1CE00,	0x1D000,	0x1D200,	0x1D400,	0x1D600,	0x1DE00,	0x1E400,	0x1E800,	0x20400,	0x20600,	0x20800,	0x20C00,	0x21000,	0x22C00,	0x23000,	0x23800,	0x23C00,	0x24600,	0x24A00,	0x25200,	0x26000,	0x26200,	0x26400,	0x27200,	0x27A00,	0x5C600,
                            };
            uint[] length = {
                                0x000002C8,	0x00000B88,	0x0000002C,	0x00000038,	0x00000150,	0x00000004,	0x00000008,	0x000001C0,	0x000000BE,	0x00000024,	0x00002100,	0x00000140,	0x00000440,	0x00000574,	0x00004E28,	0x00004E28,	0x00004E28,	0x00000170,	0x0000061C,	0x00000504,	0x000006A0,	0x00000644,	0x00000104,	0x00000004,	0x00000420,	0x00000064,	0x000003F0,	0x0000070C,	0x00000180,	0x00000004,	0x0000000C,	0x00000048,	0x00000054,	0x00000644,	0x000005C8,	0x000002F8,	0x00001B40,	0x000001F4,	0x000001F0,	0x00000216,	0x00000390,	0x00001A90,	0x00000308,	0x00000618,	0x0000025C,	0x00000834,	0x00000318,	0x000007D0,	0x00000C48,	0x00000078,	0x00000200,	0x00000C84,	0x00000628,	0x00034AD0,	0x0000E058,
                            };

            int csoff = 0x6A81A;

            if (ModifierKeys == Keys.Control)
            {
                csoff += 0x7F000;
                for (int i = 0; i < start.Length; i++)
                {
                    start[i] += 0x7F000;
                }
            }

            for (int i = 0; i < length.Length; i++)
            {

                byte[] data = new Byte[length[i]];
                Array.Copy(sav, start[i], data, 0, length[i]);
                ushort checksum = ccitt16(data);
                ushort actualsum = (ushort)(sav[csoff + i * 0x8] + sav[csoff + i * 0x8 + 1] * 0x100);
                RTB.Text += i.ToString("X2") + " - " + start[i].ToString("X5") + " - " + length[i].ToString("X8") + " - " + checksum.ToString("X4") + " - " + actualsum.ToString("X4");
                if (checksum != actualsum)
                {
                    RTB.Text += " - INVALID.\r\n";
                    invalid++;
                }
                else { RTB.Text += "\r\n"; }
            }
            RTB.Text += "Done. " + (0x37-invalid).ToString() + "/" + 0x37.ToString();
        }
    }
}
