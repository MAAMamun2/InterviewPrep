using InterviewPrep.Application.Interfaces;

namespace InterviewPrep.Infrastructure.Auth;

/// <summary>
/// BCrypt, not SHA256/MD5: BCrypt is SALTED and SLOW BY DESIGN (adjustable work factor), which is
/// exactly what you want for password hashing -- it makes brute-forcing leaked hashes expensive.
/// A fast general-purpose hash like SHA256 is the wrong tool here (it's designed to be fast,
/// which is bad for passwords and fine for things like file-integrity checksums). Good one-liner
/// if asked "how do you store passwords."
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public bool Verify(string plainPassword, string hash) => BCrypt.Net.BCrypt.Verify(plainPassword, hash);
}
