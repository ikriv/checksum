/* Checksum calculator
 * Copyright (c) 2014 Ivan Krivyakov, http://www.ikriv.com/
 * Licensed under Apache License 2.0
 * http://www.apache.org/licenses/LICENSE-2.0.html
 *
 */
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyTitle("Checksum")]
[assembly: AssemblyDescription("Calculates file checksums")]
[assembly: AssemblyProduct("Checksum")]
[assembly: AssemblyCopyright("Copyright (c) 2014 Ivan Krivyakov")]

namespace Checksum
{
    class Program
    {
        const int RetValSuccess = 0;
        const int RetValInvalidArgs = 1;
        const int RetValException = 2;
        const int RetValWrongChecksum = 128;

        string _algorithmName;
        HashAlgorithm _algorithm;
        string _path;
        byte[] _expectedValue;

        static int Main(string[] args)
        {
            return (new Program()).Run(args);            
        }

        private int Run(string[] args)
        {
            if (!Parse(args))
            {
                ShowUsage(args.Length == 0);
                return RetValInvalidArgs;
            }

            if (!File.Exists(_path))
            {
                Console.Error.WriteLine("ERROR: File not found '{0}'", _path);
                return RetValException;
            }

            try
            {
                return CalculateChecksum();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: {0}", ex);
                return RetValException;
            }
        }

        private int CalculateChecksum()
        {
            using (Stream stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                byte[] checksum = _algorithm.ComputeHash(stream);

                Console.WriteLine(ToHex(checksum));

                if (_expectedValue != null)
                {
                    if (ArraysAreEqual(checksum, _expectedValue))
                    {
                        Console.WriteLine("{0} checksum is valid", _algorithmName);
                        return RetValSuccess;
                    }
                    else
                    {
                        Console.WriteLine("{0} checksum is NOT VALID!!!", _algorithmName);
                        return RetValWrongChecksum;
                    }
                }

                return RetValSuccess;
            }
        }

        private static bool ArraysAreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i=0; i<a.Length; ++i) if (a[i] != b[i]) return false;
            return true;
        }

        private static string ToHex(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        private bool Parse(string[] args)
        {
            int nArgs = args.Length;

            if (nArgs != 2 && nArgs != 3)
            {
                Console.Error.WriteLine("ERROR: Invalid number of arguments");
                return false;
            }

            _algorithmName = args[0];
            _algorithm = CreateAlgorithmByName(_algorithmName);
            if (_algorithm == null) return false;

            _path = args[1];

            if (nArgs == 3)
            {
                _expectedValue = HexToByteArray( args[2], _algorithm.HashSize / 8 );
                if (_expectedValue == null) return false;
            }

            return true;
        }

        private static HashAlgorithm CreateAlgorithmByName(string name)
        {
            if (name == null) return null;
            switch (name.ToLower())
            {
                case "md5": return MD5.Create(); 
                case "sha1": return SHA1.Create();
                case "sha256": return SHA256.Create(); 
                case "sha384": return SHA384.Create();
                case "sha512": return SHA512.Create(); 
                default:
                    Console.Error.WriteLine("ERROR: Invalid or unknown algorithm: " + name);
                    return null;
            }
        }

        private byte[] HexToByteArray(string s, int nBytes)
        {
            if (s.Length != nBytes*2)
            {
                Console.Error.WriteLine("ERROR: Invalid checksum value. Expected {0} bytes represented by {1} hex chars, got {2} chars", nBytes, nBytes*2, s.Length);
                return null;
            }

            byte[] result = new byte[nBytes];

            try
            {
                for (int i = 0; i < nBytes; ++i)
                {
                    string substr = s.Substring(i * 2, 2);
                    result[i] = Convert.ToByte(substr, 16);
                }
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: Invalid hexadecimal value '{0}'", s);
            }

            return result;
        }

        private static void ShowUsage(bool showCopyright)
        {
            if (showCopyright)
            {
                Console.Error.WriteLine(
@"Checksum, Copyright(C) 2014 Ivan Krivyakov.
Calculates and optionally validates file checksum.
");
            }
            else
            {
                Console.Error.WriteLine();
            }

            Console.Error.WriteLine(
@"USAGE: checksum algorithm file [expectedValue]

Supported algorithms: MD5, SHA1, SHA256, SHA384, SHA512

RETURN CODES:
1   - if specified arguments are invalid
2   - if an error occured during computation, such as unreadable file or out of memory
128 - if expected value is specified and the checksum does not match it
0   - in all other cases

Example:
    checksum md5 myfile.txt 123456789abcdef0deadbeeffeedface
");
        }
    }
}
