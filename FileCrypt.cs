// FileCryptor.cs
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileCryptoTool
{
    /// <summary>
    /// Structure defining constant cryptographic parameters used throughout the application.
    /// This ensures consistent security settings for key derivation and AES operations.
    /// </summary>
    public struct CryptographyParameters
    {
        public const int KeySize = 256; // AES key size in bits (256-bit is highly recommended for strong security)
        public const int BlockSize = 128; // AES block size in bits (standard for AES)
        public const int SaltSize = 16; // Size of the random salt in bytes. Must be consistent for decryption.
        public const int Iterations = 10000; // Number of PBKDF2 iterations to increase brute-force resistance.
    }

    /// <summary>
    /// Static utility class responsible for handling file encryption and decryption using AES.
    /// </summary>
    public static class FileCryptor
    {
        // Static variable defining the hashing algorithm used for PBKDF2 key derivation.
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// Encrypts the content of an input file and writes the encrypted result to an output file.
        /// </summary>
        /// <param name="inputFile">Path to the original unencrypted file.</param>
        /// <param name="outputFile">Path where the encrypted file will be saved.</param>
        /// <param name="password">The user's passphrase used for key derivation.</param>
        public static void Encrypt(string inputFile, string outputFile, string password)
        {
            // Generates a cryptographically strong, random salt. The salt is crucial 
            // to make dictionary attacks ineffective, even if two files share the same password.
            byte[] salt = RandomNumberGenerator.GetBytes(CryptographyParameters.SaltSize);
            
            // Open file streams for reading the input and writing the encrypted output.
            using (var inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (var outputFileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                // Writes the generated salt to the very beginning of the output file.
                // This salt must be read back during decryption to derive the key correctly.
                outputFileStream.Write(salt, 0, salt.Length);

                // Key derivation using PBKDF2 (Rfc2898DeriveBytes).
                // This process transforms the human-readable password into a strong binary key.
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, CryptographyParameters.Iterations, HashAlgorithm))
                {
                    // Derive the secret key from the password.
                    byte[] key = deriveBytes.GetBytes(CryptographyParameters.KeySize / 8);
                    // Derive the Initialization Vector (IV), which is unique for each encryption operation.
                    byte[] iv = deriveBytes.GetBytes(CryptographyParameters.BlockSize / 8);

                    // Initialize the AES algorithm instance.
                    using (var aes = Aes.Create())
                    {
                        aes.KeySize = CryptographyParameters.KeySize;
                        aes.BlockSize = CryptographyParameters.BlockSize;
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC; // Cipher Block Chaining mode
                        aes.Padding = PaddingMode.PKCS7;

                        // Creates the encryptor object (ICryptoTransform) responsible for the actual cryptographic transformation.
                        using (var encryptor = aes.CreateEncryptor())
                        {
                            // Creates a CryptoStream, which acts as a bridge: data written to it 
                            // is automatically encrypted before being passed to the underlying FileStream.
                            using (var cryptoStream = new CryptoStream(outputFileStream, encryptor, CryptoStreamMode.Write))
                            {
                                // Transfers all data from the input file stream directly through the crypto stream, 
                                // encrypting the entire file content block by block.
                                inputFileStream.CopyTo(cryptoStream);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts the content of an encrypted file and restores the original data to an output file.
        /// </summary>
        /// <param name="inputFile">Path to the encrypted file (.aes).</param>
        /// <param name="outputFile">Path where the decrypted file will be saved.</param>
        /// <param name="password">The user's passphrase used for key derivation.</param>
        public static void Decrypt(string inputFile, string outputFile, string password)
        {
            // Buffer to hold the salt read from the encrypted file header.
            byte[] salt = new byte[CryptographyParameters.SaltSize];
            
            // Open file streams for reading the encrypted input and writing the output.
            using (var inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (var outputFileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                // --- FIX for CA2022: Ensure the full salt size is read ---
                int bytesRead = 0;
                int bytesToRead = CryptographyParameters.SaltSize;
                
                // Loops until exactly 16 bytes of salt have been read from the stream.
                while (bytesToRead > 0)
                {
                    // Reads a chunk of bytes, starting from the current position (bytesRead).
                    int n = inputFileStream.Read(salt, bytesRead, bytesToRead);
                    
                    // If n == 0, we reached the end of the file before reading the full salt header.
                    if (n == 0)
                    {
                        throw new EndOfStreamException("The encrypted file is corrupted or too short (missing the required salt header).");
                    }
                    
                    bytesRead += n;
                    bytesToRead -= n;
                }
                // --- End FIX ---
                
                // Key derivation using PBKDF2 (must use the same salt, password, and iteration count as used during encryption).
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, CryptographyParameters.Iterations, HashAlgorithm))
                {
                    byte[] key = deriveBytes.GetBytes(CryptographyParameters.KeySize / 8);
                    byte[] iv = deriveBytes.GetBytes(CryptographyParameters.BlockSize / 8);

                    using (var aes = Aes.Create())
                    {
                        aes.KeySize = CryptographyParameters.KeySize;
                        aes.BlockSize = CryptographyParameters.BlockSize;
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        
                        // Creates the decryptor object.
                        using (var decryptor = aes.CreateDecryptor())
                        {
                            // Creates a CryptoStream for reading, which automatically decrypts data 
                            // as it is read from the underlying FileStream.
                            try
                            {
                                using (var cryptoStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read))
                                {
                                    // Transfers decrypted data from the crypto stream to the final output file.
                                    cryptoStream.CopyTo(outputFileStream);
                                }
                            }
                            catch (CryptographicException)
                            {
                                // Catches an exception that typically occurs if the key (password) is wrong 
                                // or the file structure is corrupted, as decryption fails.
                                // Clean-up steps: close and delete the partially written output file to prevent data fragments.
                                outputFileStream.Close();
                                File.Delete(outputFile);
                                throw new Exception("Incorrect password, corrupted file, or invalid format.");
                            }
                        }
                    }
                }
            }
        }
    }
}