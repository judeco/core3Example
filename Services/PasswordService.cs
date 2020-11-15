using Interfaces.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Services
{
    public interface IPasswordService
    {
        UserAuthentication HashPassword(string password);

        bool VerifyPassword(string hashedPassword, string enteredPassword);
    }

    public class PasswordService : IPasswordService
    {
        private readonly byte _formatMarker;
        private readonly KeyDerivationPrf _prf; // Requires Microsoft.AspNetCore
        private readonly HashAlgorithmName _hashAlgorithmName;
        private readonly bool _includeHeaderInfo;
        private readonly int _saltLength;
        private readonly int _requestedLength;
        private readonly int _iterCount;

        public PasswordService()
        {
            // IdentityV3
            _formatMarker = 0x01;
            _prf = KeyDerivationPrf.HMACSHA256; // Requires Microsoft.AspNetCore
            _hashAlgorithmName = HashAlgorithmName.SHA256;
            _includeHeaderInfo = true;
            _saltLength = 128 / 8; // bits/1 byte = 16
            _requestedLength = 256 / 8; // bits/1 byte = 32
            _iterCount = 10000;
        }

        public UserAuthentication HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            byte[] salt = new byte[_saltLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var subKey = KeyDerivation.Pbkdf2(password, salt, _prf, _iterCount, _requestedLength);

            var headerByteLength = 1; // Format marker only
            if (_includeHeaderInfo) headerByteLength = 13;

            var outputBytes = new byte[headerByteLength + salt.Length + subKey.Length];

            outputBytes[0] = _formatMarker;

            if (_includeHeaderInfo)
            {
                WriteNetworkByteOrder(outputBytes, 1, (uint)_prf);
                WriteNetworkByteOrder(outputBytes, 5, (uint)_iterCount);
                WriteNetworkByteOrder(outputBytes, 9, (uint)_saltLength);
            }

            Buffer.BlockCopy(salt, 0, outputBytes, headerByteLength, salt.Length);
            Buffer.BlockCopy(subKey, 0, outputBytes, headerByteLength + _saltLength, subKey.Length);
            UserAuthentication userAuthentication = new UserAuthentication(Convert.ToBase64String(salt), Convert.ToBase64String(outputBytes))
            {
                UserId = 0
            };
            return userAuthentication;
            // return Convert.ToBase64String(outputBytes);
        }

        public bool VerifyPassword(string hashedPassword, string enteredPassword)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(hashedPassword)) return false;

            byte[] decodedHashedPassword;
            try
            {
                decodedHashedPassword = Convert.FromBase64String(hashedPassword);
            }
            catch (Exception)
            {
                return false;
            }

            if (decodedHashedPassword.Length == 0) return false;

            // Read the format marker
            var verifyMarker = decodedHashedPassword[0];
            if (_formatMarker != verifyMarker) return false;

            try
            {
                if (_includeHeaderInfo)
                {
                    // Read header information
                    var shaUInt = ReadNetworkByteOrder(decodedHashedPassword, 1);
                    var verifyPrf = shaUInt switch
                    {
                        0 => KeyDerivationPrf.HMACSHA1,
                        1 => KeyDerivationPrf.HMACSHA256,
                        2 => KeyDerivationPrf.HMACSHA512,
                        _ => KeyDerivationPrf.HMACSHA256,
                    };
                    if (_prf != verifyPrf) return false;

                    var verifyAlgorithmName = shaUInt switch
                    {
                        0 => HashAlgorithmName.SHA1,
                        1 => HashAlgorithmName.SHA256,
                        2 => HashAlgorithmName.SHA512,
                        _ => HashAlgorithmName.SHA256,
                    };
                    if (_hashAlgorithmName != verifyAlgorithmName) return false;

                    int iterCountRead = (int)ReadNetworkByteOrder(decodedHashedPassword, 5);
                    if (_iterCount != iterCountRead) return false;

                    int saltLengthRead = (int)ReadNetworkByteOrder(decodedHashedPassword, 9);
                    if (_saltLength != saltLengthRead) return false;
                }

                var headerByteLength = 1; // Format marker only
                if (_includeHeaderInfo) headerByteLength = 13;

                // Read the salt
                byte[] salt = new byte[_saltLength];
                Buffer.BlockCopy(decodedHashedPassword, headerByteLength, salt, 0, salt.Length);

                // Read the subkey (the rest of the payload)
                int subkeyLength = decodedHashedPassword.Length - headerByteLength - salt.Length;

                if (_requestedLength != subkeyLength) return false;

                byte[] expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(decodedHashedPassword, headerByteLength + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // Hash the incoming password and verify it
                var actualSubKey = KeyDerivation.Pbkdf2(enteredPassword, salt, _prf, _iterCount, subkeyLength);

                return ByteArraysEqual(actualSubKey, expectedSubkey);
            }
            catch
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                return false;
            }
        }

        // Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null || a.Length != b.Length) return false;
            var areSame = true;
            for (var i = 0; i < a.Length; i++) { areSame &= (a[i] == b[i]); }
            return areSame;
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24)
                | ((uint)(buffer[offset + 1]) << 16)
                | ((uint)(buffer[offset + 2]) << 8)
                | buffer[offset + 3];
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }
    }
}