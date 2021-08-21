using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhisperMonitor
{
    public partial class Form1 : Form
    {

        TimeSpan minimumEmailInterval = new TimeSpan(0, 5, 0);
        string fileName;
        string senderAddress;
        string senderPassword;
        string recipientAddress;
        List<int> rectangleParams;
        public Form1()
        {
            InitializeComponent();
            //get settings
            senderAddress = getSetting("senderAddress");
            senderPassword = getSetting("senderPassword");
            recipientAddress = getSetting("recipientAddress");
            rectangleParams = getSetting("rectangleParams").Split(',').Select(Int32.Parse).ToList();

            //define the file to save the image showing the whisper
            fileName = Path.Combine(Path.GetTempPath(), "d2.jpg");

            //infinite loop to continually check for whispers
            while (true)
            {
                checkScreenForWhisper();
            }
        }

        public void checkScreenForWhisper()
        {
            Rectangle bounds = new Rectangle(rectangleParams[0], rectangleParams[1], rectangleParams[2], rectangleParams[3]);
            Color whisperColor = Color.FromArgb(37, 255, 0);
            int whisperColorPixelCount = 0;
            int whisperColorPixelCountMinimum = 20;

            if(File.Exists(fileName))
                File.Delete(fileName);

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }                

                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        if (bitmap.GetPixel(i, j) == whisperColor)
                            whisperColorPixelCount++;
                        if (whisperColorPixelCount > whisperColorPixelCountMinimum)
                        {
                            bitmap.Save(fileName, ImageFormat.Jpeg);
                            sendEmail();
                            return;
                        }

                    }
                }
                
            }
        }

        DateTime lastEmail = DateTime.MinValue;

        public void sendEmail()
        {
            if ((DateTime.Now - lastEmail) > minimumEmailInterval)
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.UseDefaultCredentials = false;
                NetworkCredential credentials = new NetworkCredential(senderAddress, senderPassword);
                client.Credentials = credentials;
                client.EnableSsl = true;
                MailMessage message = GetMailWithImg();
                client.Send(message);
                message.Dispose();
                lastEmail = DateTime.Now;
            }
        }

        private MailMessage GetMailWithImg()
        {
            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.AlternateViews.Add(GetEmbeddedImage(fileName));
            mail.From = new MailAddress(senderAddress);
            mail.To.Add(recipientAddress);
            mail.Subject = "D2: you got a whisper!";
            return mail;
        }

        private AlternateView GetEmbeddedImage(String filePath)
        {
            LinkedResource res = new LinkedResource(filePath);
            res.ContentId = Guid.NewGuid().ToString();
            string htmlBody = @"<img src='cid:" + res.ContentId + @"'/>";
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
        }

        static string getSetting(string key)
        {
            var appSettings = ConfigurationManager.AppSettings;
            return appSettings[key];
        }
    }
}
