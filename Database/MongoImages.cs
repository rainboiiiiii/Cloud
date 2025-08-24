using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TheCloud.Database
{
    public class MongoImages
    {
        private readonly IMongoDatabase _db;
        private readonly GridFSBucket _bucket;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        public MongoImages(string connectionString, string databaseName)
        {
            try
            {
                var client = new MongoClient(connectionString);
                _db = client.GetDatabase(databaseName);
                _bucket = new GridFSBucket(_db);
                Console.WriteLine($"✅ Connected to MongoDB database: {databaseName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MongoDB connection failed: {ex.Message}");
                throw;
            }
        }

        // Upload all images from a local folder
        public async Task<string> UploadImagesAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"❌ Folder not found: {folderPath}");
                return $"❌ Folder not found: `{folderPath}`";
            }

            int uploaded = 0;
            int skipped = 0;
            int failed = 0;

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var extension = Path.GetExtension(file).ToLower();
                var fileName = Path.GetFileName(file);

                if (!AllowedExtensions.Contains(extension))
                {
                    Console.WriteLine($"⚠️ Skipping unsupported file: {fileName}");
                    skipped++;
                    continue;
                }

                try
                {
                    // Check for duplicates
                    var existing = await _bucket.Find(Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName)).FirstOrDefaultAsync();
                    if (existing != null)
                    {
                        Console.WriteLine($"⚠️ Skipping duplicate: {fileName}");
                        skipped++;
                        continue;
                    }

                    using var stream = File.OpenRead(file);
                    await _bucket.UploadFromStreamAsync(fileName, stream);
                    Console.WriteLine($"✅ Uploaded: {fileName}");
                    uploaded++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to upload {fileName}: {ex.Message}");
                    failed++;
                }
            }

            return $"✅ Uploaded `{uploaded}` images. ⚠️ Skipped `{skipped}`. ❌ Failed `{failed}`.";
        }

        // Get a random image from GridFS
        public async Task<(Stream Stream, string FileName)> GetRandomImageAsync()
        {
            try
            {
                var files = await _bucket.Find(Builders<GridFSFileInfo>.Filter.Empty).ToListAsync();
                if (files.Count == 0)
                {
                    Console.WriteLine("⚠️ No images found in GridFS.");
                    return (null, null);
                }

                var rand = new Random();
                var chosen = files[rand.Next(files.Count)];

                var stream = await _bucket.OpenDownloadStreamAsync(chosen.Id);
                return (stream, chosen.Filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to retrieve image from GridFS: {ex.Message}");
                return (null, null);
            }
        }
    }
}