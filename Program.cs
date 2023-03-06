using System.Media;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using System.Drawing;

/// <summary>
/// This project takes the output from a USB webcam and plays a sound when motion is detected.
/// </summary>

namespace MotionSensor
{
    class Program
    {
        const int MIN_SAVE_INTERVAL_SECONDS = 10;
        private static DateTime lastSavedTime;

        static async Task Main(string[] args)
        {
            // Find and initialize the webcam
            MediaCapture mediaCapture = await FindWebcam();
            DateTime lastSoundPlayedTime = DateTime.MinValue;

            // Create SoundFiles folder if it doesn't exist
            string soundFilesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SoundFiles");
            if (!Directory.Exists(soundFilesFolderPath))
            {
                Directory.CreateDirectory(soundFilesFolderPath);
            }

            // Make a MotionDetected  directory to store the images in
            string motionDetectedFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MotionDetected");
            if (!Directory.Exists(motionDetectedFolderPath))
            {
                Directory.CreateDirectory(motionDetectedFolderPath);
            }

            // Populate soundFiles array with all supported sound files in the SoundFiles folder
            string[] supportedExtensions = new[] { ".wav", ".mp3", ".wma", ".mid", ".midi", ".aiff", ".aif", ".m4a" };
            string[] soundFiles = Directory.GetFiles(soundFilesFolderPath, "*.*").Where(file => supportedExtensions.Contains(Path.GetExtension(file))).ToArray();
            
            //TODO: Shuffle array here

            Random random = new Random(); // Random object to select a sound file
            bool playBeep = false;

            if (soundFiles.Length == 0)
            {
                Console.WriteLine("Warning: No sound files found in directory.");
                Console.WriteLine("Playing default sound instead.");
                playBeep = true;
            }


            //TODO: Implement an alteranative to this while loop, as it might as well not even be here. I believe this is the cause of it eating up large amounts of RAM.
            // Check for motion every x time
            while (true)
            {
                // Capture a photo from the webcam
                var photo = new MemoryStream();
                await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), photo.AsRandomAccessStream());

                // Check if there is motion in the photo
                if (IsMotionDetected(photo))
                {
                    if ((DateTime.Now - lastSoundPlayedTime).TotalSeconds >= MIN_SAVE_INTERVAL_SECONDS)
                    {
                        // Display a motion detection alert
                        Console.WriteLine("!Motion Detected!");
                        lastSoundPlayedTime = DateTime.Now;

                        if (playBeep == false)
                        {
                            // Select a random sound file from the list
                            string soundFile = soundFiles[random.Next(soundFiles.Length)];
                            Console.WriteLine("Now playing sound file: " + Path.GetFileName(soundFile));
                            SoundPlayer soundPlayer = new SoundPlayer(soundFile); // Create a new SoundPlayer object with the selected sound file
                            soundPlayer.Play(); // Play the sound file
                        }

                        if(playBeep == true)
                        {
                            Console.WriteLine("Now playing Console.Beep");
                            Console.Beep(1000, 500);
                        }
                    }
                }
            }
        }

        //Find the webcam connected. Note: This found my offbrand USB Webcam right away and I've only been able to test this with my webcam. If this gives anyone errors do let me know.
        static async Task<MediaCapture> FindWebcam()
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (devices.Count > 0)
            {
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = devices[0].Id };
                var mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(settings);
                return mediaCapture;
            }

            Console.WriteLine("No webcam detected.");
            return null;
        }

        //Motion Detection algorithim, it looks for a small change in the pixels of a static photo captured by the webcam and compares the changed pixels to the motionThreshold and pixelThreshold values
        static bool IsMotionDetected(Stream photo)
        {
            const int motionThreshold = 200; // Change this value depending on your environment
            const int pixelThreshold = 120; // Change this value depending on your environment

            Bitmap bitmap = new Bitmap(photo);

            // Get the dimensions of the bitmap
            int width = bitmap.Width;
            int height = bitmap.Height;

            // Create two bitmap objects to compare
            Bitmap bitmap1 = new Bitmap(bitmap);
            Bitmap bitmap2 = new Bitmap(bitmap);

            // Convert both bitmaps to grayscale
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = bitmap1.GetPixel(x, y);
                    int gray = (int)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
                    bitmap1.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    bitmap2.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            // Apply a blur to the second bitmap
            bitmap2 = new Bitmap(bitmap2, new Size(width / 8, height / 8));
            bitmap2 = new Bitmap(bitmap2, new Size(width, height));

            // Calculate the difference between the two bitmaps
            int motionCount = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color1 = bitmap1.GetPixel(x, y);
                    Color color2 = bitmap2.GetPixel(x, y);
                    int diff = Math.Abs(color1.R - color2.R);
                    if (diff > pixelThreshold)
                    {
                        motionCount++;
                    }
                }
            }

            if ((DateTime.Now - lastSavedTime).TotalSeconds < MIN_SAVE_INTERVAL_SECONDS)
            {
                return false;
            }

            // Save the image to file and update lastSavedTime
            lastSavedTime = DateTime.Now;

            // Determine if there is motion
            if (motionCount > motionThreshold)
            {
                string motionDetectedFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MotionDetected");
                string fileName = "MotionDetected" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";
                string filePath = Path.Combine(motionDetectedFolderPath, fileName);
                bitmap.Save(filePath);
                // Delete the oldest photo if there are already 10 photos in the directory
                string[] photoFiles = Directory.GetFiles(motionDetectedFolderPath, "*.jpg");
                if (photoFiles.Length > 10)
                {
                    Array.Sort(photoFiles, new StringComparer());
                    File.Delete(photoFiles[0]);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        // Custom string comparer class for sorting file names by creation time
        class StringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                DateTime xCreationTime = File.GetCreationTime(x);
                DateTime yCreationTime = File.GetCreationTime(y);
                return xCreationTime.CompareTo(yCreationTime);
            }
        }
    }
}