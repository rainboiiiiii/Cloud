using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ImageLibrary
{
    public static class DynamicImageUrls
    {
        private static readonly string filePath = Path.Combine(AppContext.BaseDirectory, "imageurls.json");
        private static readonly Random _random = new Random();
        private static List<string> _urls = new List<string>();

        public static int Count => _urls.Count;

        public static void LoadUrls()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"❌ imageurls.json not found at {filePath}");
                    _urls = new List<string>();
                    return;
                }

                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<ImageData>(json);

                _urls = data?.Urls ?? new List<string>();
                Console.WriteLine($"✅ Loaded {_urls.Count} image URLs from imageurls.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load image URLs: {ex.Message}");
                _urls = new List<string>();
            }
        }

        public static string GetRandomImageUrl()
        {
            if (_urls.Count == 0) return null;
            return _urls[_random.Next(_urls.Count)];
        }

        private class ImageData
        {
            public List<string> Urls { get; set; }
        }
    }
}