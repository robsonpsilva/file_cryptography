# CSE310---File Cryptography--Module2

# Overview

This application is part of the CSE-310 course activities, serving as a robust file encryption and decryption console utility.

# Algorithm Summary

The core algorithm used is AES-256 (Advanced Encryption Standard). A strong, unique cryptographic key is generated from the user's passphrase using the PBKDF2 (Password-Based Key Derivation Function 2) function, which includes a randomly generated salt to ensure security and resistance against brute-force attacks.

#  Key Features

* Symmetric Encryption: Encrypts any file content using the highly secure AES-256 standard.

* Key Derivation: Securely transforms the user's password into a strong key using PBKDF2 with 10,000 iterations.

* File Integrity: Reads and verifies the necessary cryptographic parameters (Salt, IV) during decryption, ensuring the file hasn't been corrupted.

* Safety: Automatically deletes the original file after successful encryption.

# How to Run

* Navigate to the project directory in your terminal.

* Compile and run the application: dotnet run

* Follow the on-screen menu prompts to encrypt or decrypt your files.


# Software Description
The developed solution is a C# console utility for secure file encryption and decryption, utilizing the industry-standard AES-256 symmetric algorithm for data protection. The application prioritizes security by employing PBKDF2 (Password-Based Key Derivation Function 2) to safely derive a strong cryptographic key from the user's password. The program's core logic is separated into a user-friendly menu system (Program.cs) and a cryptographic engine (FileCrypt.cs), which handles files of any size using CryptoStream for processing and includes the critical security feature of deleting the original unencrypted file after successful encryption.

# Purpose of creating this software
The goal is to practice basic application development skills by creating an application that encrypts and decrypts a file using modern encryption algorithms. This allows other developers to see and learn the basic structure of an encryption solution in C#.

# Youtube link

[BookStore](https://youtu.be/316Y3SIsgqQ)

# Development Environment

The development environment includes:
a) Visual Studio Code.


#Programming Language

C# (C Sharp) is a modern, object-oriented, and statically-typed programming language created by Microsoft as the primary language for the versatile .NET framework. Designed for reliability and performance, it combines C++ syntax with the safety features of Java. C# is cross-platform and widely used for developing powerful web backends/APIs, desktop applications, and video games (especially with the Unity engine), offering a balance between developer productivity and enterprise-level system control.

# Useful Websites

* [Microsoft Ignite](https://learn.microsoft.com/en-us/dotnet/csharp/)
* [Programiz](https://www.programiz.com/csharp-programming)
* [StackOverflow](https://stackoverflow.com/questions/22385229/best-practices-for-symmetric-encryption-in-net)

