// Program.cs
using System;
using System.IO;

namespace FileCryptoTool
{
    /// <summary>
    /// The main application class responsible for user interaction, menu handling,
    /// and routing encryption/decryption requests to the FileCryptor class.
    /// </summary>
    public class Program
    {
        // Control variable to manage the main application loop state.
        private static bool isRunning = true;

        public static void Main(string[] args)
        {
            // Application startup message.
            Console.WriteLine("AES File Encryption Tool");
            
            // Main application loop that continues until the user chooses to exit (Option 4).
            while (isRunning)
            {
                // Variables to store user input for menu selection, file path, and password.
                string? choice;
                string? filePath;
                string? password;

                // Displays the menu options to the user.
                DisplayMenu();

                // Reads the user's choice, trims any whitespace, and converts to lowercase for easy comparison.
                choice = Console.ReadLine()?.Trim().ToLower();

                // Conditional block to handle the user's menu selection.
                if (string.IsNullOrEmpty(choice) || choice == "4")
                {
                    // Exit condition: sets the flag to terminate the main loop.
                    isRunning = false;
                    continue;
                }
                
                // Handles Encrypt (1) or Decrypt (2) operations.
                if (choice == "1" || choice == "2")
                {
                    Console.Write("Enter the full file path: ");
                    filePath = Console.ReadLine();

                    Console.Write("Enter the encryption passphrase: ");
                    password = Console.ReadLine();

                    // Validation: Checks if the file exists and if a password was provided.
                    if (!File.Exists(filePath) || string.IsNullOrEmpty(password))
                    {
                        Console.WriteLine("\n Error: File not found or invalid password.");
                        continue;
                    }

                    try
                    {
                        // Conditional routing based on the user's choice (1 or 2).
                        if (choice == "1")
                        {
                            EncryptFile(filePath, password);
                        }
                        else if (choice == "2")
                        {
                            DecryptFile(filePath, password);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Global error handling for critical cryptographic or IO issues.
                        Console.WriteLine($"\nFATAL ERROR: {ex.Message}");
                    }
                }
                // Handles Algorithm Information (3).
                else if (choice == "3")
                {
                    Console.WriteLine("\nAlgorithm Information: AES (Advanced Encryption Standard) is the most widely used symmetric encryption standard globally. It is combined with PBKDF2 (Password-Based Key Derivation Function 2) to ensure the user's passphrase is transformed into a strong cryptographic key.");
                }
                // Handles invalid input.
                else
                {
                    Console.WriteLine("\nInvalid option. Please try again.");
                }

                Console.WriteLine("\n--- Press ENTER to continue ---");
                Console.ReadLine();
            }

            Console.WriteLine("Program terminated. Goodbye!");
        }

        /// <summary>
        /// Utility function to clear the console and display the application menu options.
        /// </summary>
        private static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Encrypt File");
            Console.WriteLine("2. Decrypt File");
            Console.WriteLine("3. Algorithm Information");
            Console.WriteLine("4. Exit");
            Console.Write("> ");
        }

        /// <summary>
        /// Handles the encryption process flow.
        /// </summary>
        private static void EncryptFile(string filePath, string password)
        {
            Console.WriteLine("\nStarting Encryption...");
            // Defines the output file path by appending the standard '.aes' extension.
            string outputFilePath = filePath + ".aes";
            
            // Calls the core encryption logic implemented in the FileCryptor static class.
            FileCryptor.Encrypt(filePath, outputFilePath, password);
            
            Console.WriteLine($"Encryption Complete. File saved at: {outputFilePath}");
            // Deletes the original unencrypted file to ensure only the encrypted version remains on disk.
            File.Delete(filePath);
            Console.WriteLine($"   (Original file deleted: {filePath})");
        }

        /// <summary>
        /// Handles the decryption process flow.
        /// </summary>
        private static void DecryptFile(string filePath, string password)
        {
            // Pre-validation: Ensures the input file has the expected '.aes' extension.
            if (!filePath.EndsWith(".aes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Error: The file does not appear to be an encrypted file (.aes).");
                return;
            }

            Console.WriteLine("\nStarting Decryption...");
            // Derives the original file path by removing the last 4 characters (".aes").
            string outputFilePath = filePath.Substring(0, filePath.Length - 4);

            // Calls the core decryption logic implemented in the FileCryptor static class.
            FileCryptor.Decrypt(filePath, outputFilePath, password);
            
            Console.WriteLine($"Decryption Complete. File restored at: {outputFilePath}");
        }
    }
}